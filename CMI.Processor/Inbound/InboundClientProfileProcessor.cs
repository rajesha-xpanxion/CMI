using CMI.Automon.Interface;
using CMI.Automon.Model;
using CMI.Common.Logging;
using CMI.Nexus.Model;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace CMI.Processor
{
    public class InboundClientProfileProcessor : InboundBaseProcessor
    {
        private readonly IOffenderService offenderService;

        public InboundClientProfileProcessor(
            IServiceProvider serviceProvider,
            IConfiguration configuration,
            IOffenderService offenderService
        )
            : base(serviceProvider, configuration)
        {
            this.offenderService = offenderService;
        }

        public override Common.Notification.TaskExecutionStatus Execute()
        {
            Logger.LogInfo(new LogRequest
            {
                OperationName = "Processor",
                MethodName = "ProcessClientProfiles",
                Message = "Client Profile processing initiated."
            });

            IEnumerable<Offender> allOffenderDetails = null;
            Common.Notification.TaskExecutionStatus taskExecutionStatus = new Common.Notification.TaskExecutionStatus { TaskName = "Process Client Profiles" };

            try
            {
                allOffenderDetails = offenderService.GetAllOffenderDetails(ProcessorConfig.CmiDbConnString, LastExecutionDateTime);

                foreach (var offenderDetails in allOffenderDetails)
                {
                    taskExecutionStatus.AutomonReceivedRecordCount++;

                    Client client = null;
                    try
                    {
                        client = new Client()
                        {
                            IntegrationId = FormatId(offenderDetails.Pin),
                            FirstName = offenderDetails.FirstName,
                            MiddleName = string.IsNullOrEmpty(offenderDetails.MiddleName) ? null : offenderDetails.MiddleName,
                            LastName = offenderDetails.LastName,
                            ClientType = offenderDetails.ClientType,
                            TimeZone = offenderDetails.TimeZone,
                            Gender = offenderDetails.Gender,
                            Ethnicity = MapEthnicity(offenderDetails.Race),
                            DateOfBirth = offenderDetails.DateOfBirth.ToShortDateString(),

                            CaseloadId = MapCaseload(offenderDetails.CaseloadName),
                            SupervisingOfficerEmailId = MapSupervisingOfficer(offenderDetails.OfficerFirstName, offenderDetails.OfficerLastName, offenderDetails.OfficerEmail)
                        };

                        if (ClientService.GetClientDetails(client.IntegrationId) == null)
                        {
                            if (ClientService.AddNewClientDetails(client))
                            {
                                taskExecutionStatus.NexusAddRecordCount++;

                                Logger.LogDebug(new LogRequest
                                {
                                    OperationName = "Processor",
                                    MethodName = "ProcessClientProfiles",
                                    Message = "New Client Profile added successfully.",
                                    AutomonData = JsonConvert.SerializeObject(offenderDetails),
                                    NexusData = JsonConvert.SerializeObject(client)
                                });
                            }
                        }
                        else
                        {
                            if (ClientService.UpdateClientDetails(client))
                            {
                                taskExecutionStatus.NexusUpdateRecordCount++;

                                Logger.LogDebug(new LogRequest
                                {
                                    OperationName = "Processor",
                                    MethodName = "ProcessClientProfiles",
                                    Message = "Existing Client Profile updated successfully.",
                                    AutomonData = JsonConvert.SerializeObject(offenderDetails),
                                    NexusData = JsonConvert.SerializeObject(client)
                                });
                            }
                        }
                    }
                    catch (CmiException ce)
                    {
                        taskExecutionStatus.NexusFailureRecordCount++;

                        Logger.LogWarning(new LogRequest
                        {
                            OperationName = "Processor",
                            MethodName = "ProcessClientProfiles",
                            Message = "Error occurred in API while processing a Client Profile.",
                            Exception = ce,
                            AutomonData = JsonConvert.SerializeObject(offenderDetails),
                            NexusData = JsonConvert.SerializeObject(client)
                        });
                    }
                    catch (Exception ex)
                    {
                        taskExecutionStatus.NexusFailureRecordCount++;

                        Logger.LogError(new LogRequest
                        {
                            OperationName = "Processor",
                            MethodName = "ProcessClientProfiles",
                            Message = "Error occurred while processing a Client Profile.",
                            Exception = ex,
                            AutomonData = JsonConvert.SerializeObject(offenderDetails),
                            NexusData = JsonConvert.SerializeObject(client)
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
                    OperationName = "Processor",
                    MethodName = "ProcessClientProfiles",
                    Message = "Error occurred while processing Client Profiles.",
                    Exception = ex,
                    AutomonData = JsonConvert.SerializeObject(allOffenderDetails)
                });
            }

            Logger.LogInfo(new LogRequest
            {
                OperationName = "Processor",
                MethodName = "ProcessClientProfiles",
                Message = "Client Profile processing completed.",
                CustomParams = JsonConvert.SerializeObject(taskExecutionStatus)
            });

            return taskExecutionStatus;
        }

        private string MapEthnicity(string automonEthnicity)
        {
            return
                (LookupService.Ethnicities != null && LookupService.Ethnicities.Any(e => e.Equals(automonEthnicity, StringComparison.InvariantCultureIgnoreCase)))
                ? automonEthnicity
                : DAL.Constants.EthnicityUnknown;
        }

        private string MapCaseload(string automonCaseloadName)
        {
            if (LookupService.CaseLoads != null && LookupService.CaseLoads.Any(c => c.Name.Equals(automonCaseloadName, StringComparison.InvariantCultureIgnoreCase)))
            {
                return LookupService.CaseLoads.FirstOrDefault(c => c.Name.Equals(automonCaseloadName, StringComparison.InvariantCultureIgnoreCase)).Id.ToString();
            }

            return null;
        }

        private string MapSupervisingOfficer(string automonFirstName, string automonLastName, string automonEmailAddress)
        {
            if (LookupService.SupervisingOfficers != null && LookupService.SupervisingOfficers.Any(s => s.FirstName.Equals(automonFirstName, StringComparison.InvariantCultureIgnoreCase) && s.LastName.Equals(automonLastName, StringComparison.InvariantCultureIgnoreCase) && s.Email.Equals(automonEmailAddress, StringComparison.InvariantCultureIgnoreCase)))
            {
                return LookupService.SupervisingOfficers.FirstOrDefault(s => s.FirstName.Equals(automonFirstName, StringComparison.InvariantCultureIgnoreCase) && s.LastName.Equals(automonLastName, StringComparison.InvariantCultureIgnoreCase) && s.Email.Equals(automonEmailAddress, StringComparison.InvariantCultureIgnoreCase)).Email;
            }

            return null;
        }
    }
}
