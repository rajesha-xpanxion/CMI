using CMI.Automon.Interface;
using CMI.Automon.Model;
using CMI.Common.Logging;
using CMI.Common.Notification;
using CMI.Nexus.Interface;
using CMI.Nexus.Model;
using CMI.Processor.DAL;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CMI.Processor
{
    public class InboundEmploymentProcessor : InboundBaseProcessor
    {
        private readonly IOffenderEmploymentService offenderEmploymentService;
        private readonly IEmploymentService employmentService;

        public InboundEmploymentProcessor(
            IServiceProvider serviceProvider,
            IConfiguration configuration,
            IOffenderEmploymentService offenderEmploymentService,
            IEmploymentService employmentService
        )
            : base(serviceProvider, configuration)
        {
            this.offenderEmploymentService = offenderEmploymentService;
            this.employmentService = employmentService;
        }

        public override TaskExecutionStatus Execute(DateTime? lastExecutionDateTime, IEnumerable<string> officerLogonsToFilter)
        {
            Logger.LogInfo(new LogRequest
            {
                OperationName = this.GetType().Name,
                MethodName = "Execute",
                Message = "Employment processing initiated."
            });

            IEnumerable<OffenderEmployment> allOffenderEmployments = null;
            TaskExecutionStatus taskExecutionStatus = new TaskExecutionStatus { ProcessorType = Common.Notification.ProcessorType.Inbound, TaskName = "Process Employments" };

            try
            {
                //retrieve data from Automon for processing
                allOffenderEmployments = offenderEmploymentService.GetAllOffenderEmployments(ProcessorConfig.CmiDbConnString, lastExecutionDateTime, GetOfficerLogonToFilterDataTable(officerLogonsToFilter));

                //check if there are any records to process
                if (allOffenderEmployments.Any())
                {
                    //check if there are any records to process
                    Logger.LogDebug(new LogRequest
                    {
                        OperationName = this.GetType().Name,
                        MethodName = "Execute",
                        Message = "Offender Employment records received from Automon.",
                        CustomParams = allOffenderEmployments.Count().ToString()
                    });

                    //retrieve distinct list of offender pin
                    List<string> distinctOffenderPin = allOffenderEmployments.Select(p => p.Pin).Distinct().ToList();

                    //iterate through each of offender pin
                    foreach (string currentOffenderPin in distinctOffenderPin)
                    {
                        //check if client exist on Nexus side for current offender pin. This is to avoid validation errors from Nexus API in further calls.
                        if (ClientService.GetClientDetails(currentOffenderPin) != null)
                        {
                            //get all employments for given offender pin
                            var allExistingEmploymentDetails = employmentService.GetAllEmploymentDetails(currentOffenderPin);

                            if (allExistingEmploymentDetails != null && allExistingEmploymentDetails.Any())
                            {
                                //set ClientId value
                                allExistingEmploymentDetails.ForEach(ea => ea.ClientId = currentOffenderPin);
                            }

                            //iterate through each of offender employment details for current offender pin
                            foreach (var offenderEmploymentDetails in allOffenderEmployments.Where(a => a.Pin.Equals(currentOffenderPin, StringComparison.InvariantCultureIgnoreCase)))
                            {
                                taskExecutionStatus.AutomonReceivedRecordCount++;

                                Employment employment = null;
                                try
                                {
                                    //transform offender employment details in Nexus compliant model
                                    employment = new Employment()
                                    {
                                        ClientId = FormatId(offenderEmploymentDetails.Pin),
                                        EmployerId = string.Format("{0}-{1}", FormatId(offenderEmploymentDetails.Pin), offenderEmploymentDetails.Id),
                                        Employer = offenderEmploymentDetails.OrganizationName,
                                        Occupation = offenderEmploymentDetails.JobTitle,
                                        WorkAddress = offenderEmploymentDetails.OrganizationAddress,
                                        WorkPhone = offenderEmploymentDetails.OrganizationPhone,
                                        Wage = string.IsNullOrEmpty(offenderEmploymentDetails.PayRate) ? null : string.Concat(offenderEmploymentDetails.PayRate.Where(y => y.Equals('.') || Char.IsDigit(y))),
                                        WageUnit = MapWageUnit(offenderEmploymentDetails.PayFrequency),
                                        WorkEnvironment = offenderEmploymentDetails.WorkType,
                                        IsActive = offenderEmploymentDetails.IsActive
                                    };

                                    //get crud action type based on comparison
                                    switch (GetCrudActionType(employment, allExistingEmploymentDetails))
                                    {
                                        case CrudActionType.Add:
                                            if (employmentService.AddNewEmploymentDetails(employment))
                                            {
                                                taskExecutionStatus.NexusAddRecordCount++;

                                                Logger.LogDebug(new LogRequest
                                                {
                                                    OperationName = this.GetType().Name,
                                                    MethodName = "Execute",
                                                    Message = "New Client Employment details added successfully.",
                                                    AutomonData = JsonConvert.SerializeObject(offenderEmploymentDetails),
                                                    NexusData = JsonConvert.SerializeObject(employment)
                                                });
                                            }
                                            break;
                                        case CrudActionType.Update:
                                            if (employmentService.UpdateEmploymentDetails(employment))
                                            {
                                                taskExecutionStatus.NexusUpdateRecordCount++;

                                                Logger.LogDebug(new LogRequest
                                                {
                                                    OperationName = this.GetType().Name,
                                                    MethodName = "Execute",
                                                    Message = "Existing Client Employment details updated successfully.",
                                                    AutomonData = JsonConvert.SerializeObject(offenderEmploymentDetails),
                                                    NexusData = JsonConvert.SerializeObject(employment)
                                                });
                                            }
                                            break;
                                        case CrudActionType.Delete:
                                            if (employmentService.DeleteEmploymentDetails(employment.ClientId, employment.EmployerId))
                                            {
                                                taskExecutionStatus.NexusDeleteRecordCount++;

                                                Logger.LogDebug(new LogRequest
                                                {
                                                    OperationName = this.GetType().Name,
                                                    MethodName = "Execute",
                                                    Message = "Existing Client Employment details deleted successfully.",
                                                    AutomonData = JsonConvert.SerializeObject(offenderEmploymentDetails),
                                                    NexusData = JsonConvert.SerializeObject(employment)
                                                });
                                            }
                                            break;
                                        default:
                                            taskExecutionStatus.AutomonReceivedRecordCount--;
                                            break;
                                    }
                                }
                                catch (CmiException ce)
                                {
                                    taskExecutionStatus.NexusFailureRecordCount++;

                                    Logger.LogWarning(new LogRequest
                                    {
                                        OperationName = this.GetType().Name,
                                        MethodName = "Execute",
                                        Message = "Error occurred in API while processing a Client Employment.",
                                        Exception = ce,
                                        AutomonData = JsonConvert.SerializeObject(offenderEmploymentDetails),
                                        NexusData = JsonConvert.SerializeObject(employment)
                                    });
                                }
                                catch (Exception ex)
                                {
                                    taskExecutionStatus.NexusFailureRecordCount++;

                                    Logger.LogError(new LogRequest
                                    {
                                        OperationName = this.GetType().Name,
                                        MethodName = "Execute",
                                        Message = "Error occurred while processing a Client Employment.",
                                        Exception = ex,
                                        AutomonData = JsonConvert.SerializeObject(offenderEmploymentDetails),
                                        NexusData = JsonConvert.SerializeObject(employment)
                                    });
                                }
                            }
                        }
                    }
                }

                taskExecutionStatus.IsSuccessful = taskExecutionStatus.NexusFailureRecordCount == 0;
            }
            catch (Exception ex)
            {
                taskExecutionStatus.IsSuccessful = false;

                Logger.LogError(new LogRequest
                {
                    OperationName = this.GetType().Name,
                    MethodName = "Execute",
                    Message = "Error occurred while processing Employments.",
                    Exception = ex,
                    AutomonData = JsonConvert.SerializeObject(allOffenderEmployments)
                });
            }

            Logger.LogInfo(new LogRequest
            {
                OperationName = this.GetType().Name,
                MethodName = "Execute",
                Message = "Employments processing completed.",
                CustomParams = JsonConvert.SerializeObject(taskExecutionStatus)
            });

            return taskExecutionStatus;
        }

        protected override void LoadLookupData()
        {
            throw new NotImplementedException();
        }

        private string MapWageUnit(string automonPayFrequency)
        {
            string nexusWageUnit = string.Empty;

            switch (automonPayFrequency)
            {
                case "Hourly":
                    nexusWageUnit = "per Hour";
                    break;
                case "Weekly":
                    nexusWageUnit = "per Week";
                    break;
                case "Monthly":
                    nexusWageUnit = "per Month";
                    break;
                case "Annually":
                    nexusWageUnit = "Annually";
                    break;
            }

            return nexusWageUnit;
        }

        private CrudActionType GetCrudActionType(Employment employment, IEnumerable<Employment> employments)
        {
            //check if list is null, YES = return Add Action type
            if (employments == null)
            {
                return CrudActionType.Add;
            }

            //try to get existing record using ClientId & EmployerId
            Employment existingEmployment = employments.Where(a
                =>
                    a.ClientId.Equals(employment.ClientId, StringComparison.InvariantCultureIgnoreCase)
                    && a.EmployerId.Equals(employment.EmployerId, StringComparison.InvariantCultureIgnoreCase)
            )
            .FirstOrDefault();

            //check if record already exists
            if (existingEmployment == null)
            {
                //record does not exist.
                //check if record is active. YES = return Add action type, NO = return None action type
                if (employment.IsActive)
                {
                    return CrudActionType.Add;
                }
                else
                {
                    return CrudActionType.None;
                }
            }
            else
            {
                //record already exist.
                //check if record is active. YES = compare it with existing record. NO = return Delete action type
                if (employment.IsActive)
                {
                    //record is active.
                    //compare with existing record. Equal = return None action type, Not Equal = return Update action type
                    if (employment.Equals(existingEmployment))
                    {
                        return CrudActionType.None;
                    }
                    else
                    {
                        return CrudActionType.Update;
                    }
                }
                else
                {
                    return CrudActionType.Delete;
                }
            }
        }
    }
}
