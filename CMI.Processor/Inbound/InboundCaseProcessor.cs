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

        public override Common.Notification.TaskExecutionStatus Execute(DateTime? lastExecutionDateTime, IEnumerable<string> officerLogonsToFilter)
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
            Common.Notification.TaskExecutionStatus taskExecutionStatus = new Common.Notification.TaskExecutionStatus { ProcessorType = ProcessorType.Inbound, TaskName = "Process Cases" };
            DateTime currentTimestamp = DateTime.Now;

            try
            {
                allOffenderCaseDetails = offenderCaseService.GetAllOffenderCases(ProcessorConfig.CmiDbConnString, lastExecutionDateTime, GetOfficerLogonToFilterDataTable(officerLogonsToFilter));

                //log number of records received from Automon
                if (allOffenderCaseDetails.Any())
                {
                    Logger.LogDebug(new LogRequest
                    {
                        OperationName = this.GetType().Name,
                        MethodName = "Execute",
                        Message = "Offender Case records received from Automon.",
                        CustomParams = allOffenderCaseDetails.Count().ToString()
                    });
                }

                foreach (var offenderCaseDetails in allOffenderCaseDetails.GroupBy(x => new { x.Pin, x.CaseNumber }).Select(y => y.First()))
                {
                    taskExecutionStatus.AutomonReceivedRecordCount++;

                    Case @case = null;
                    try
                    {
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

                        if (ClientService.GetClientDetails(@case.ClientId) != null)
                        {
                            if (caseService.GetCaseDetailsUsingAllEndPoint(@case.ClientId, @case.CaseNumber) == null)
                            {
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
                            }
                            else
                            {
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
    }
}
