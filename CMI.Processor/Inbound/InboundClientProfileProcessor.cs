using CMI.Automon.Interface;
using CMI.Automon.Model;
using CMI.Common.Logging;
using CMI.Nexus.Model;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using Microsoft.Extensions.Configuration;
using CMI.Common.Notification;

namespace CMI.Processor
{
    public class InboundClientProfileProcessor : InboundBaseProcessor
    {
        private readonly IOffenderService offenderService;
        private readonly IOffenderProfilePictureService offenderProfilePictureService;

        public InboundClientProfileProcessor(
            IServiceProvider serviceProvider,
            IConfiguration configuration,
            IOffenderService offenderService,
            IOffenderProfilePictureService offenderProfilePictureService
        )
            : base(serviceProvider, configuration)
        {
            this.offenderService = offenderService;
            this.offenderProfilePictureService = offenderProfilePictureService;
        }

        public override Common.Notification.TaskExecutionStatus Execute(DateTime? lastExecutionDateTime)
        {
            Logger.LogInfo(new LogRequest
            {
                OperationName = this.GetType().Name,
                MethodName = "Execute",
                Message = "Client Profile processing initiated."
            });

            //load required lookup data
            LoadLookupData();

            IEnumerable<Offender> allOffenderDetails = null;
            Common.Notification.TaskExecutionStatus taskExecutionStatus = new Common.Notification.TaskExecutionStatus { ProcessorType = ProcessorType.Inbound, TaskName = "Process Client Profiles" };

            try
            {
                allOffenderDetails = offenderService.GetAllOffenderDetails(ProcessorConfig.CmiDbConnString, lastExecutionDateTime);

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
                            Ethnicity = MapEthnicity(offenderDetails.RaceDescription, offenderDetails.RacePermDesc),
                            DateOfBirth = offenderDetails.DateOfBirth.ToShortDateString(),

                            CaseloadId = MapCaseload(offenderDetails.CaseloadName),
                            SupervisingOfficerEmailId = MapSupervisingOfficer(offenderDetails.OfficerFirstName, offenderDetails.OfficerLastName, offenderDetails.OfficerEmail)
                        };

                        //check if client already exists
                        if (ClientService.GetClientDetails(client.IntegrationId) == null)
                        {
                            //add new client profile details to Nexus
                            if (ClientService.AddNewClientDetails(client))
                            {
                                //increase add record count
                                taskExecutionStatus.NexusAddRecordCount++;

                                Logger.LogDebug(new LogRequest
                                {
                                    OperationName = this.GetType().Name,
                                    MethodName = "Execute",
                                    Message = "New Client Profile added successfully.",
                                    AutomonData = JsonConvert.SerializeObject(offenderDetails),
                                    NexusData = JsonConvert.SerializeObject(client)
                                });

                                //get offender mugshot photo details
                                OffenderMugshot offenderMugshot = offenderProfilePictureService.GetOffenderMugshotPhoto(ProcessorConfig.CmiDbConnString, offenderDetails.Pin);
                                ClientProfilePicture clientProfilePicture = null;

                                //check if there is any mugshot photo set for given offender
                                if (offenderMugshot != null && offenderMugshot.DocumentData!= null)
                                {
                                    clientProfilePicture = new ClientProfilePicture
                                    {
                                        IntegrationId = FormatId(offenderMugshot.Pin),
                                        ImageBase64String = Convert.ToBase64String(offenderMugshot.DocumentData)
                                    };
                                    if (ClientService.AddNewClientProfilePicture(clientProfilePicture))
                                    {
                                        Logger.LogDebug(new LogRequest
                                        {
                                            OperationName = this.GetType().Name,
                                            MethodName = "Execute",
                                            Message = "New Client Profile Picture added successfully.",
                                            AutomonData = JsonConvert.SerializeObject(offenderMugshot),
                                            NexusData = JsonConvert.SerializeObject(clientProfilePicture)
                                        });
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (ClientService.UpdateClientDetails(client))
                            {
                                taskExecutionStatus.NexusUpdateRecordCount++;

                                Logger.LogDebug(new LogRequest
                                {
                                    OperationName = this.GetType().Name,
                                    MethodName = "Execute",
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
                            OperationName = this.GetType().Name,
                            MethodName = "Execute",
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
                            OperationName = this.GetType().Name,
                            MethodName = "Execute",
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
                    OperationName = this.GetType().Name,
                    MethodName = "Execute",
                    Message = "Error occurred while processing Client Profiles.",
                    Exception = ex,
                    AutomonData = JsonConvert.SerializeObject(allOffenderDetails)
                });
            }

            Logger.LogInfo(new LogRequest
            {
                OperationName = this.GetType().Name,
                MethodName = "Execute",
                Message = "Client Profile processing completed.",
                CustomParams = JsonConvert.SerializeObject(taskExecutionStatus)
            });

            return taskExecutionStatus;
        }

        protected override void LoadLookupData()
        {
            //load Ethnicities lookup data
            try
            {
                if (LookupService.Ethnicities != null)
                {
                    Logger.LogDebug(new LogRequest
                    {
                        OperationName = this.GetType().Name,
                        MethodName = "LoadLookupData",
                        Message = "Successfully retrieved Ethnicities from lookup",
                        CustomParams = JsonConvert.SerializeObject(LookupService.Ethnicities)
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(new LogRequest
                {
                    OperationName = this.GetType().Name,
                    MethodName = "LoadLookupData",
                    Message = "Error occurred while loading Ethnicities lookup data",
                    Exception = ex
                });
            }

            //load CaseLoads lookup data
            try
            {
                if (LookupService.CaseLoads != null)
                {
                    Logger.LogDebug(new LogRequest
                    {
                        OperationName = this.GetType().Name,
                        MethodName = "LoadLookupData",
                        Message = "Successfully retrieved Caseloads from lookup",
                        CustomParams = JsonConvert.SerializeObject(LookupService.CaseLoads)
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(new LogRequest
                {
                    OperationName = this.GetType().Name,
                    MethodName = "LoadLookupData",
                    Message = "Error occurred while loading CaseLoads lookup data",
                    Exception = ex
                });
            }

            //load SupervisingOfficers lookup data
            try
            {
                if (LookupService.SupervisingOfficers != null)
                {
                    Logger.LogDebug(new LogRequest
                    {
                        OperationName = this.GetType().Name,
                        MethodName = "LoadLookupData",
                        Message = "Successfully retrieved SupervisingOfficers from lookup",
                        CustomParams = JsonConvert.SerializeObject(LookupService.SupervisingOfficers)
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(new LogRequest
                {
                    OperationName = this.GetType().Name,
                    MethodName = "LoadLookupData",
                    Message = "Error occurred while loading SupervisingOfficers lookup data",
                    Exception = ex
                });
            }

            //load Genders lookup data
            try
            {
                if (LookupService.Genders != null)
                {
                    Logger.LogDebug(new LogRequest
                    {
                        OperationName = this.GetType().Name,
                        MethodName = "LoadLookupData",
                        Message = "Successfully retrieved Genders from lookup",
                        CustomParams = JsonConvert.SerializeObject(LookupService.Genders)
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(new LogRequest
                {
                    OperationName = this.GetType().Name,
                    MethodName = "LoadLookupData",
                    Message = "Error occurred while loading Genders lookup data",
                    Exception = ex
                });
            }

            //load TimeZones lookup data
            try
            {
                if (LookupService.TimeZones != null)
                {
                    Logger.LogDebug(new LogRequest
                    {
                        OperationName = this.GetType().Name,
                        MethodName = "LoadLookupData",
                        Message = "Successfully retrieved TimeZones from lookup",
                        CustomParams = JsonConvert.SerializeObject(LookupService.TimeZones)
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(new LogRequest
                {
                    OperationName = this.GetType().Name,
                    MethodName = "LoadLookupData",
                    Message = "Error occurred while loading TimeZones lookup data",
                    Exception = ex
                });
            }

            //load ClientTypes lookup data
            try
            {
                if (LookupService.ClientTypes != null)
                {
                    Logger.LogDebug(new LogRequest
                    {
                        OperationName = this.GetType().Name,
                        MethodName = "LoadLookupData",
                        Message = "Successfully retrieved ClientTypes from lookup",
                        CustomParams = JsonConvert.SerializeObject(LookupService.ClientTypes)
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(new LogRequest
                {
                    OperationName = this.GetType().Name,
                    MethodName = "LoadLookupData",
                    Message = "Error occurred while loading ClientTypes lookup data",
                    Exception = ex
                });
            }
        }

        private string MapEthnicity(string automonRaceDescription, string automonRacePermDesc)
        {
            string mappedEthnicity = DAL.Constants.EthnicityUnknown;

            if(LookupService.Ethnicities != null && LookupService.Ethnicities.Any())
            {
                if(LookupService.Ethnicities.Any(e => e.Equals(automonRaceDescription, StringComparison.InvariantCultureIgnoreCase)))
                {
                    mappedEthnicity = automonRaceDescription;
                }
                else if (LookupService.Ethnicities.Any(e => e.Equals(automonRacePermDesc, StringComparison.InvariantCultureIgnoreCase)))
                {
                    mappedEthnicity = automonRacePermDesc;
                }
            }

            return mappedEthnicity;
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
