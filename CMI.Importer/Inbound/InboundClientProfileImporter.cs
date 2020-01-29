using CMI.Common.Logging;
using CMI.Nexus.Model;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using Microsoft.Extensions.Configuration;
using CMI.Importer.DAL;

namespace CMI.Importer
{
    public class InboundClientProfileImporter : InboundBaseImporter
    {
        public InboundClientProfileImporter(
            IServiceProvider serviceProvider,
            IConfiguration configuration
        )
            : base(serviceProvider, configuration)
        {
        }

        public override void Execute()
        {
            Logger.LogInfo(new LogRequest
            {
                OperationName = this.GetType().Name,
                MethodName = "Execute",
                Message = "Client Profile import initiated."
            });

            IEnumerable<ClientProfileDetails> retrievedClientProfiles = null;

            try
            {
                DateTime startDateTime = DateTime.Now;
                retrievedClientProfiles = ImporterProvider.RetrieveClientProfileDataFromExcel();
                DateTime endDateTime = DateTime.Now;
                TimeSpan timeSpan = endDateTime - startDateTime;

                if (retrievedClientProfiles != null && retrievedClientProfiles.Any())
                {
                    Logger.LogDebug(new LogRequest
                    {
                        OperationName = this.GetType().Name,
                        MethodName = "Execute",
                        Message = string.Format("#{0} Client Profile records retrieved in {1} seconds.", retrievedClientProfiles.Count(), timeSpan.TotalSeconds)
                    });

                    var toBeProcessedClientProfiles = ImporterProvider.SaveClientProfilesToDatabase(retrievedClientProfiles);

                    foreach (var clientProfileDetails in toBeProcessedClientProfiles.Where(x => x.IsImportSuccessful == false))
                    {
                        Client client = null;
                        try
                        {
                            client = new Client()
                            {
                                IntegrationId = clientProfileDetails.IntegrationId,
                                FirstName = clientProfileDetails.FirstName,
                                MiddleName = string.IsNullOrEmpty(clientProfileDetails.MiddleName) ? null : clientProfileDetails.MiddleName,
                                LastName = clientProfileDetails.LastName,
                                ClientType = clientProfileDetails.ClientType,
                                TimeZone = clientProfileDetails.TimeZone,
                                Gender = string.IsNullOrEmpty(clientProfileDetails.Gender) ? null : clientProfileDetails.Gender,
                                Ethnicity = string.IsNullOrEmpty(clientProfileDetails.Ethnicity) ? null : clientProfileDetails.Ethnicity,
                                DateOfBirth = clientProfileDetails.DateOfBirth,
                                SupervisingOfficerEmailId = clientProfileDetails.SupervisingOfficerEmailId
                            };

                            if (ClientService.GetClientDetails(client.IntegrationId) == null)
                            {
                                //add new client profile details to Nexus
                                if (ClientService.AddNewClientDetails(client))
                                {
                                    Logger.LogDebug(new LogRequest
                                    {
                                        OperationName = this.GetType().Name,
                                        MethodName = "Execute",
                                        Message = "New Client Profile added successfully.",
                                        NexusData = JsonConvert.SerializeObject(client)
                                    });

                                    clientProfileDetails.IsImportSuccessful = true;
                                }
                            }
                            else
                            {
                                if (ClientService.UpdateClientDetails(client))
                                {
                                    Logger.LogDebug(new LogRequest
                                    {
                                        OperationName = this.GetType().Name,
                                        MethodName = "Execute",
                                        Message = "Existing Client Profile updated successfully.",
                                        NexusData = JsonConvert.SerializeObject(client)
                                    });

                                    clientProfileDetails.IsImportSuccessful = true;
                                }
                            }
                        }
                        catch (CmiException ce)
                        {
                            Logger.LogWarning(new LogRequest
                            {
                                OperationName = this.GetType().Name,
                                MethodName = "Execute",
                                Message = "Error occurred in API while importing a Client Profile.",
                                Exception = ce,
                                NexusData = JsonConvert.SerializeObject(client)
                            });

                            clientProfileDetails.IsImportSuccessful = false;
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError(new LogRequest
                            {
                                OperationName = this.GetType().Name,
                                MethodName = "Execute",
                                Message = "Error occurred while importing a Client Profile.",
                                Exception = ex,
                                NexusData = JsonConvert.SerializeObject(client)
                            });

                            clientProfileDetails.IsImportSuccessful = false;
                        }
                    }

                    ImporterProvider.SaveClientProfilesToDatabase(toBeProcessedClientProfiles);
                }
                else
                {
                    Logger.LogWarning(new LogRequest
                    {
                        OperationName = this.GetType().Name,
                        MethodName = "Execute",
                        Message = "No record available for Client Profile import."
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(new LogRequest
                {
                    OperationName = this.GetType().Name,
                    MethodName = "Execute",
                    Message = "Error occurred while importing Client Profiles.",
                    Exception = ex,
                    NexusData = JsonConvert.SerializeObject(retrievedClientProfiles)
                });
            }

            Logger.LogInfo(new LogRequest
            {
                OperationName = this.GetType().Name,
                MethodName = "Execute",
                Message = "Client Profile import completed."
            });
        }
    }
}
