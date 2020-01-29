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
    public class InboundContactImporter : InboundBaseImporter
    {
        private readonly IContactService contactService;

        public InboundContactImporter(
            IServiceProvider serviceProvider,
            IConfiguration configuration,
            IImporterProvider importerProvider,
            IContactService contactService
        )
            : base(serviceProvider, configuration)
        {
            this.contactService = contactService;
        }

        public override void Execute()
        {
            Logger.LogInfo(new LogRequest
            {
                OperationName = this.GetType().Name,
                MethodName = "Execute",
                Message = "Contact import initiated."
            });

            IEnumerable<ContactDetails> retrievedContacts = null;

            try
            {
                DateTime startDateTime = DateTime.Now;
                retrievedContacts = ImporterProvider.RetrieveContactDataFromExcel();
                DateTime endDateTime = DateTime.Now;
                TimeSpan timeSpan = endDateTime - startDateTime;

                if (retrievedContacts != null && retrievedContacts.Any())
                {
                    Logger.LogDebug(new LogRequest
                    {
                        OperationName = this.GetType().Name,
                        MethodName = "Execute",
                        Message = string.Format("#{0} Contact records retrieved in {1} seconds.", retrievedContacts.Count(), timeSpan.TotalSeconds)
                    });

                    var toBeProcessedContacts = ImporterProvider.SaveContactsToDatabase(retrievedContacts);

                    foreach (var contactDetails in toBeProcessedContacts.Where(x => x.IsImportSuccessful == false))
                    {
                        Contact contact = null;
                        try
                        {
                            contact = new Contact()
                            {
                                ClientId = contactDetails.IntegrationId,
                                ContactId = contactDetails.ContactId,
                                ContactType = contactDetails.ContactType,
                                ContactValue = contactDetails.ContactValue,
                                IsPrimary = true
                            };

                            if (ClientService.GetClientDetails(contact.ClientId) != null)
                            {
                                if (contactService.GetContactDetails(contact.ClientId, contact.ContactId) == null)
                                {
                                    if (contactService.AddNewContactDetails(contact))
                                    {
                                        Logger.LogDebug(new LogRequest
                                        {
                                            OperationName = this.GetType().Name,
                                            MethodName = "Execute",
                                            Message = "New Contact details added successfully.",
                                            NexusData = JsonConvert.SerializeObject(contact)
                                        });

                                        contactDetails.IsImportSuccessful = true;
                                    }
                                }
                                else
                                {
                                    if (contactService.UpdateContactDetails(contact))
                                    {
                                        Logger.LogDebug(new LogRequest
                                        {
                                            OperationName = this.GetType().Name,
                                            MethodName = "Execute",
                                            Message = "Existing Contact details updated successfully.",
                                            NexusData = JsonConvert.SerializeObject(contact)
                                        });

                                        contactDetails.IsImportSuccessful = true;
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
                                    NexusData = JsonConvert.SerializeObject(contact)
                                });

                                contactDetails.IsImportSuccessful = false;
                            }
                        }
                        catch (CmiException ce)
                        {
                            Logger.LogWarning(new LogRequest
                            {
                                OperationName = this.GetType().Name,
                                MethodName = "Execute",
                                Message = "Error occurred in API while importing a Contact.",
                                Exception = ce,
                                NexusData = JsonConvert.SerializeObject(contact)
                            });

                            contactDetails.IsImportSuccessful = false;
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError(new LogRequest
                            {
                                OperationName = this.GetType().Name,
                                MethodName = "Execute",
                                Message = "Error occurred while importing a Contact.",
                                Exception = ex,
                                NexusData = JsonConvert.SerializeObject(contact)
                            });

                            contactDetails.IsImportSuccessful = false;
                        }
                    }

                    ImporterProvider.SaveContactsToDatabase(toBeProcessedContacts);
                }
                else
                {
                    Logger.LogWarning(new LogRequest
                    {
                        OperationName = this.GetType().Name,
                        MethodName = "Execute",
                        Message = "No record available for Contact import."
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(new LogRequest
                {
                    OperationName = this.GetType().Name,
                    MethodName = "Execute",
                    Message = "Error occurred while importing Contacts.",
                    Exception = ex,
                    AutomonData = JsonConvert.SerializeObject(retrievedContacts)
                });
            }

            Logger.LogInfo(new LogRequest
            {
                OperationName = this.GetType().Name,
                MethodName = "Execute",
                Message = "Contact import completed."
            });
        }
    }
}
