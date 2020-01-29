using CMI.Common.Logging;
using CMI.Importer.DAL;
using CMI.Nexus.Interface;
using CMI.Nexus.Model;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CMI.Importer
{
    public class InboundCourtCaseImporter : InboundBaseImporter
    {
        private readonly ICaseService caseService;

        public InboundCourtCaseImporter(
            IServiceProvider serviceProvider,
            IConfiguration configuration,
            ICaseService caseService
        )
            : base(serviceProvider, configuration)
        {
            this.caseService = caseService;
        }

        public override void Execute()
        {
            Logger.LogInfo(new LogRequest
            {
                OperationName = this.GetType().Name,
                MethodName = "Execute",
                Message = "Case import intiated."
            });

            IEnumerable<CourtCaseDetails> retrievedCourtCases = null;

            try
            {
                DateTime startDateTime = DateTime.Now;
                retrievedCourtCases = ImporterProvider.RetrieveCourtCaseDataFromExcel();
                DateTime endDateTime = DateTime.Now;

                TimeSpan timeSpan = endDateTime - startDateTime;

                if (retrievedCourtCases != null && retrievedCourtCases.Any())
                {
                    Logger.LogDebug(new LogRequest
                    {
                        OperationName = this.GetType().Name,
                        MethodName = "Execute",
                        Message = string.Format("#{0} Case records retrieved in {1} seconds.", retrievedCourtCases.Count(), timeSpan.TotalSeconds)
                    });

                    var toBeProcessedCourtCases = ImporterProvider.SaveCourtCasesToDatabase(retrievedCourtCases);

                    foreach (var courtCaseDetails in toBeProcessedCourtCases.Where(x => x.IsImportSuccessful == false))
                    {
                        Case @case = null;
                        try
                        {
                            @case = new Case()
                            {
                                ClientId = courtCaseDetails.IntegrationId,
                                CaseNumber = courtCaseDetails.CaseNumber,
                                CaseDate = courtCaseDetails.CaseDate,
                                Status = courtCaseDetails.Status,
                                EndDate = string.IsNullOrEmpty(courtCaseDetails.EndDate) ? null : courtCaseDetails.EndDate,
                                EarlyReleaseDate = string.IsNullOrEmpty(courtCaseDetails.EarlyReleaseDate) ? null : courtCaseDetails.EarlyReleaseDate,
                                EndReason = string.IsNullOrEmpty(courtCaseDetails.EndReason) ? null : courtCaseDetails.EndReason
                            };

                            if (ClientService.GetClientDetails(@case.ClientId) != null)
                            {
                                if (caseService.GetCaseDetailsUsingAllEndPoint(@case.ClientId, @case.CaseNumber) == null)
                                {
                                    if (caseService.AddNewCaseDetails(@case))
                                    {
                                        Logger.LogDebug(new LogRequest
                                        {
                                            OperationName = this.GetType().Name,
                                            MethodName = "Execute",
                                            Message = "New Case details added successfully.",
                                            NexusData = JsonConvert.SerializeObject(@case)
                                        });

                                        courtCaseDetails.IsImportSuccessful = true;
                                    }
                                }
                                else
                                {
                                    if (caseService.UpdateCaseDetails(@case))
                                    {
                                        Logger.LogDebug(new LogRequest
                                        {
                                            OperationName = this.GetType().Name,
                                            MethodName = "Execute",
                                            Message = "Existing Case details updated successfully.",
                                            NexusData = JsonConvert.SerializeObject(@case)
                                        });

                                        courtCaseDetails.IsImportSuccessful = true;
                                    }
                                }
                            }
                            else
                            {
                                Logger.LogWarning(new LogRequest
                                {
                                    OperationName = this.GetType().Name,
                                    MethodName = "Execute",
                                    Message = "Client Profile does not exist.",
                                    NexusData = JsonConvert.SerializeObject(@case)
                                });

                                courtCaseDetails.IsImportSuccessful = false;
                            }
                        }
                        catch (CmiException ce)
                        {
                            Logger.LogWarning(new LogRequest
                            {
                                OperationName = this.GetType().Name,
                                MethodName = "Execute",
                                Message = "Error occurred in API while importing a Case.",
                                Exception = ce,
                                NexusData = JsonConvert.SerializeObject(@case)
                            });

                            courtCaseDetails.IsImportSuccessful = false;
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError(new LogRequest
                            {
                                OperationName = this.GetType().Name,
                                MethodName = "Execute",
                                Message = "Error occurred while importing a Case.",
                                Exception = ex,
                                NexusData = JsonConvert.SerializeObject(@case)
                            });

                            courtCaseDetails.IsImportSuccessful = false;
                        }
                    }

                    ImporterProvider.SaveCourtCasesToDatabase(toBeProcessedCourtCases);
                }
                else
                {
                    Logger.LogWarning(new LogRequest
                    {
                        OperationName = this.GetType().Name,
                        MethodName = "Execute",
                        Message = "No record available for Case import."
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(new LogRequest
                {
                    OperationName = this.GetType().Name,
                    MethodName = "Execute",
                    Message = "Error occurred while importing Cases.",
                    Exception = ex,
                    NexusData = JsonConvert.SerializeObject(retrievedCourtCases)
                });
            }

            Logger.LogInfo(new LogRequest
            {
                OperationName = this.GetType().Name,
                MethodName = "Execute",
                Message = "Case import completed."
            });
        }
    }
}
