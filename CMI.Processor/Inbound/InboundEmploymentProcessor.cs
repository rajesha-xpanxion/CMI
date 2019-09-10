using CMI.Automon.Interface;
using CMI.Automon.Model;
using CMI.Common.Logging;
using CMI.Common.Notification;
using CMI.Nexus.Interface;
using CMI.Nexus.Model;
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

            //load required lookup data
            LoadLookupData();

            IEnumerable<OffenderEmployment> allOffenderEmployments = null;
            Common.Notification.TaskExecutionStatus taskExecutionStatus = new Common.Notification.TaskExecutionStatus { ProcessorType = ProcessorType.Inbound, TaskName = "Process Employments" };

            try
            {
                allOffenderEmployments = offenderEmploymentService.GetAllOffenderEmployments(ProcessorConfig.CmiDbConnString, lastExecutionDateTime, GetOfficerLogonToFilterDataTable(officerLogonsToFilter));

                foreach (var offenderEmploymentDetails in allOffenderEmployments)
                {
                    taskExecutionStatus.AutomonReceivedRecordCount++;

                    Employment employment = null;
                    try
                    {
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

                        if (ClientService.GetClientDetails(employment.ClientId) != null)
                        {
                            if (employmentService.GetEmploymentDetails(employment.ClientId, employment.EmployerId) == null)
                            {
                                if (employment.IsActive && employmentService.AddNewEmploymentDetails(employment))
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
                                else
                                {
                                    taskExecutionStatus.AutomonReceivedRecordCount--;
                                }
                            }
                            else if (!employment.IsActive)
                            {
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
                            }
                            else
                            {
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
                            }
                        }
                        else
                        {
                            taskExecutionStatus.AutomonReceivedRecordCount--;
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
    }
}
