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
using System.Text;

namespace CMI.Processor
{
    public class InboundCaseProcessor : InboundBaseProcessor
    {
        private readonly IOffenderCaseService offenderCaseService;
        private readonly ICaseService caseService;

        public InboundCaseProcessor(
            IServiceProvider serviceProvider,
            IConfiguration configuration,
            IOffenderCaseService offenderCaseService,
            ICaseService caseService
        )
            : base(serviceProvider, configuration)
        {
            this.offenderCaseService = offenderCaseService;
            this.caseService = caseService;
        }

        public override TaskExecutionStatus Execute(DateTime? lastExecutionDateTime, IEnumerable<string> officerLogonsToFilter)
        {
            Logger.LogInfo(new LogRequest
            {
                OperationName = this.GetType().Name,
                MethodName = "Execute",
                Message = "Case processing intiated."
            });

            //load required lookup data
            LoadLookupData();

            IEnumerable<OffenderCase> allOffenderCaseDetails = null;
            TaskExecutionStatus taskExecutionStatus = new TaskExecutionStatus { ProcessorType = Common.Notification.ProcessorType.Inbound, TaskName = "Process Cases" };
            DateTime currentTimestamp = DateTime.Now;

            try
            {
                //retrieve data from Automon for processing
                allOffenderCaseDetails = offenderCaseService.GetAllOffenderCases(ProcessorConfig.CmiDbConnString, lastExecutionDateTime, GetOfficerLogonToFilterDataTable(officerLogonsToFilter));

                //check if there are any records to process
                if (allOffenderCaseDetails.Any())
                {
                    //log number of records received from Automon
                    Logger.LogDebug(new LogRequest
                    {
                        OperationName = this.GetType().Name,
                        MethodName = "Execute",
                        Message = "Offender Case records received from Automon.",
                        CustomParams = allOffenderCaseDetails.Count().ToString()
                    });

                    //retrieve distinct list of offender pin
                    List<string> distinctOffenderPin = allOffenderCaseDetails.Select(p => p.Pin).Distinct().ToList();

                    //iterate through each of offender pin
                    foreach (string currentOffenderPin in distinctOffenderPin)
                    {
                        //check if client exist on Nexus side for current offender pin. This is to avoid validation errors from Nexus API in further calls.
                        if (ClientService.GetClientDetails(currentOffenderPin) != null)
                        {
                            //get all notes for given offender pin
                            var allExistingCaseDetails = caseService.GetAllCaseDetails(currentOffenderPin);

                            if (allExistingCaseDetails != null && allExistingCaseDetails.Any())
                            {
                                //set ClientId value
                                allExistingCaseDetails.ForEach(ea => ea.ClientId = currentOffenderPin);
                            }

                            //iterate through each of offender case details for current offender pin
                            foreach (var offenderCaseDetails in allOffenderCaseDetails.Where(a => a.Pin.Equals(currentOffenderPin, StringComparison.InvariantCultureIgnoreCase)).GroupBy(x => new { x.Pin, x.CaseNumber }).Select(y => y.First()))
                            {
                                taskExecutionStatus.AutomonReceivedRecordCount++;

                                Case @case = null;
                                try
                                {
                                    //transform offender note details in Nexus compliant model
                                    @case = new Case()
                                    {
                                        ClientId = FormatId(offenderCaseDetails.Pin),
                                        CaseNumber = offenderCaseDetails.CaseNumber,
                                        CaseDate = offenderCaseDetails.CaseDate.HasValue ? offenderCaseDetails.CaseDate.Value.ToShortDateString() : currentTimestamp.ToString(),
                                        StartDate = offenderCaseDetails.SupervisionStartDate.HasValue ? offenderCaseDetails.SupervisionStartDate.Value.ToShortDateString() : null,
                                        EndDate = offenderCaseDetails.SupervisionEndDate.HasValue ? offenderCaseDetails.SupervisionEndDate.Value.ToShortDateString() : null,
                                        Status = offenderCaseDetails.CaseStatus,
                                        EndReason = string.IsNullOrEmpty(offenderCaseDetails.ClosureReason) ? null : offenderCaseDetails.ClosureReason,
                                        Offenses = allOffenderCaseDetails
                                            .Where(z => z.Pin == offenderCaseDetails.Pin && z.CaseNumber == offenderCaseDetails.CaseNumber && !string.IsNullOrEmpty(z.OffenseLabel))
                                            .Select(p => new Offense
                                            {
                                                Label = p.OffenseLabel,
                                                Statute = p.OffenseStatute,
                                                Date = p.OffenseDate.HasValue ? p.OffenseDate.Value.ToShortDateString() : null,
                                                Category = MapOffenseCategory(p.OffenseCategory),
                                                IsPrimary = p.IsPrimary
                                            }).ToList()
                                    };

                                    //get crud action type based on comparison
                                    switch (GetCrudActionType(@case, allExistingCaseDetails))
                                    {
                                        case CrudActionType.Add:
                                            if (caseService.AddNewCaseDetails(@case))
                                            {
                                                taskExecutionStatus.NexusAddRecordCount++;

                                                Logger.LogDebug(new LogRequest
                                                {
                                                    OperationName = this.GetType().Name,
                                                    MethodName = "Execute",
                                                    Message = "New Client Case details added successfully.",
                                                    AutomonData = JsonConvert.SerializeObject(offenderCaseDetails),
                                                    NexusData = JsonConvert.SerializeObject(@case)
                                                });
                                            }
                                            break;
                                        case CrudActionType.Update:
                                            if (caseService.UpdateCaseDetails(@case))
                                            {
                                                taskExecutionStatus.NexusUpdateRecordCount++;

                                                Logger.LogDebug(new LogRequest
                                                {
                                                    OperationName = this.GetType().Name,
                                                    MethodName = "Execute",
                                                    Message = "Existing Client Case details updated successfully.",
                                                    AutomonData = JsonConvert.SerializeObject(offenderCaseDetails),
                                                    NexusData = JsonConvert.SerializeObject(@case)
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
                                        Message = "Error occurred in API while processing a Client Case.",
                                        Exception = ce,
                                        AutomonData = JsonConvert.SerializeObject(offenderCaseDetails),
                                        NexusData = JsonConvert.SerializeObject(@case)
                                    });
                                }
                                catch (Exception ex)
                                {
                                    taskExecutionStatus.NexusFailureRecordCount++;

                                    Logger.LogError(new LogRequest
                                    {
                                        OperationName = this.GetType().Name,
                                        MethodName = "Execute",
                                        Message = "Error occurred while processing a Client Case.",
                                        Exception = ex,
                                        AutomonData = JsonConvert.SerializeObject(offenderCaseDetails),
                                        NexusData = JsonConvert.SerializeObject(@case)
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
                    Message = "Error occurred while processing Cases.",
                    Exception = ex,
                    AutomonData = JsonConvert.SerializeObject(allOffenderCaseDetails)
                });
            }

            Logger.LogInfo(new LogRequest
            {
                OperationName = this.GetType().Name,
                MethodName = "Execute",
                Message = "Case processing completed.",
                CustomParams = JsonConvert.SerializeObject(taskExecutionStatus)
            });

            return taskExecutionStatus;
        }

        protected override void LoadLookupData()
        {
            //load OffenseCategory lookup data
            try
            {
                if (LookupService.OffenseCategories != null)
                {
                    Logger.LogDebug(new LogRequest
                    {
                        OperationName = this.GetType().Name,
                        MethodName = "LoadLookupData",
                        Message = "Successfully retrieved OffenseCategories from lookup",
                        CustomParams = JsonConvert.SerializeObject(LookupService.OffenseCategories)
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(new LogRequest
                {
                    OperationName = this.GetType().Name,
                    MethodName = "LoadLookupData",
                    Message = "Error occurred while loading OffenseCategories lookup data",
                    Exception = ex
                });
            }
        }

        private string MapOffenseCategory(string automonOffenseCategory)
        {
            if (LookupService.OffenseCategories != null && LookupService.OffenseCategories.Any(c => c.Equals(automonOffenseCategory, StringComparison.InvariantCultureIgnoreCase)))
            {
                return automonOffenseCategory;
            }

            return null;
        }

        private CrudActionType GetCrudActionType(Case @case, IEnumerable<Case> cases)
        {
            //check if list is null, YES = return Add Action type
            if (cases == null)
            {
                return CrudActionType.Add;
            }

            //try to get existing record using ClientId & CaseNumber
            Case existingCase = cases.Where(a
                =>
                    a.ClientId.Equals(@case.ClientId, StringComparison.InvariantCultureIgnoreCase)
                    && a.CaseNumber.Equals(@case.CaseNumber, StringComparison.InvariantCultureIgnoreCase)
            )
            .FirstOrDefault();

            //check if record already exists
            if (existingCase == null)
            {
                //record does not exist
                return CrudActionType.Add;
            }
            else
            {
                //record already exist.
                //compare with existing record. Equal = return None action type, Not Equal = return Update action type
                if (@case.Equals(existingCase))
                {
                    return CrudActionType.None;
                }
                else
                {
                    return CrudActionType.Update;
                }
            }
        }
    }
}
