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
    public class InboundEmailContactProcessor : InboundBaseProcessor
    {
        private readonly IOffenderEmailService offenderEmailService;
        private readonly IContactService contactService;

        public InboundEmailContactProcessor(
            IServiceProvider serviceProvider,
            IConfiguration configuration,
            IOffenderEmailService offenderEmailService,
            IContactService contactService
        )
            : base(serviceProvider, configuration)
        {
            this.offenderEmailService = offenderEmailService;
            this.contactService = contactService;
        }

        public override TaskExecutionStatus Execute(DateTime? lastExecutionDateTime, IEnumerable<string> officerLogonsToFilter)
        {
            Logger.LogInfo(new LogRequest
            {
                OperationName = this.GetType().Name,
                MethodName = "Execute",
                Message = "Email Contact processing initiated."
            });

            //load required lookup data
            LoadLookupData();

            IEnumerable<OffenderEmail> allOffenderEmails = null;
            TaskExecutionStatus taskExecutionStatus = new TaskExecutionStatus { ProcessorType = Common.Notification.ProcessorType.Inbound, TaskName = "Process Email Contacts" };

            try
            {
                //retrieve data from Automon for processing
                allOffenderEmails = offenderEmailService.GetAllOffenderEmails(ProcessorConfig.CmiDbConnString, lastExecutionDateTime, GetOfficerLogonToFilterDataTable(officerLogonsToFilter));

                //check if there are any records to process
                if (allOffenderEmails.Any())
                {
                    //log number of records received from Automon
                    Logger.LogDebug(new LogRequest
                    {
                        OperationName = this.GetType().Name,
                        MethodName = "Execute",
                        Message = "Offender Email records received from Automon.",
                        CustomParams = allOffenderEmails.Count().ToString()
                    });

                    //retrieve distinct list of offender pin
                    List<string> distinctOffenderPin = allOffenderEmails.Select(p => p.Pin).Distinct().ToList();

                    //iterate through each of offender pin
                    foreach (string currentOffenderPin in distinctOffenderPin)
                    {
                        //check if client exist on Nexus side for current offender pin. This is to avoid validation errors from Nexus API in further calls.
                        if (ClientService.GetClientDetails(currentOffenderPin) != null)
                        {
                            //get all contacts for given offender pin
                            var allExistingContactDetails = contactService.GetAllContactDetails(currentOffenderPin);

                            if (allExistingContactDetails != null && allExistingContactDetails.Any())
                            {
                                allExistingContactDetails = allExistingContactDetails.Where(e => e.ContactType.Equals(DAL.Constants.ContactTypeEmailNexus, StringComparison.InvariantCultureIgnoreCase)).ToList();
                                //set ClientId value
                                allExistingContactDetails.ForEach(ea => ea.ClientId = currentOffenderPin);
                            }

                            //iterate through each of offender email details for current offender pin
                            foreach (var offenderEmailDetails in allOffenderEmails.Where(a => a.Pin.Equals(currentOffenderPin, StringComparison.InvariantCultureIgnoreCase)))
                            {
                                taskExecutionStatus.AutomonReceivedRecordCount++;

                                Contact contact = null;
                                try
                                {
                                    //transform offender email details in Nexus compliant model
                                    contact = new Contact()
                                    {
                                        ClientId = FormatId(offenderEmailDetails.Pin),
                                        ContactId = string.Format("{0}-{1}", FormatId(offenderEmailDetails.Pin), offenderEmailDetails.Id),
                                        ContactType = DAL.Constants.ContactTypeEmailNexus,
                                        ContactValue = offenderEmailDetails.EmailAddress,
                                        IsPrimary = offenderEmailDetails.IsPrimary,
                                        IsActive = offenderEmailDetails.IsActive
                                    };

                                    //get crud action type based on comparison
                                    switch (GetCrudActionType(contact, allExistingContactDetails))
                                    {
                                        case CrudActionType.Add:
                                            if (contactService.AddNewContactDetails(contact))
                                            {
                                                taskExecutionStatus.NexusAddRecordCount++;

                                                Logger.LogDebug(new LogRequest
                                                {
                                                    OperationName = this.GetType().Name,
                                                    MethodName = "Execute",
                                                    Message = "New Client Email Contact details added successfully.",
                                                    AutomonData = JsonConvert.SerializeObject(offenderEmailDetails),
                                                    NexusData = JsonConvert.SerializeObject(contact)
                                                });
                                            }
                                            break;
                                        case CrudActionType.Update:
                                            if (contactService.UpdateContactDetails(contact))
                                            {
                                                taskExecutionStatus.NexusUpdateRecordCount++;

                                                Logger.LogDebug(new LogRequest
                                                {
                                                    OperationName = this.GetType().Name,
                                                    MethodName = "Execute",
                                                    Message = "Existing Client Email Contact details updated successfully.",
                                                    AutomonData = JsonConvert.SerializeObject(offenderEmailDetails),
                                                    NexusData = JsonConvert.SerializeObject(contact)
                                                });
                                            }
                                            break;
                                        case CrudActionType.Delete:
                                            if (contactService.DeleteContactDetails(contact.ClientId, contact.ContactId))
                                            {
                                                taskExecutionStatus.NexusDeleteRecordCount++;

                                                Logger.LogDebug(new LogRequest
                                                {
                                                    OperationName = this.GetType().Name,
                                                    MethodName = "Execute",
                                                    Message = "Existing Client Email Contact details deleted successfully.",
                                                    AutomonData = JsonConvert.SerializeObject(offenderEmailDetails),
                                                    NexusData = JsonConvert.SerializeObject(contact)
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
                                        Message = "Error occurred in API while processing a Client Email Contact.",
                                        Exception = ce,
                                        AutomonData = JsonConvert.SerializeObject(offenderEmailDetails),
                                        NexusData = JsonConvert.SerializeObject(contact)
                                    });
                                }
                                catch (Exception ex)
                                {
                                    taskExecutionStatus.NexusFailureRecordCount++;

                                    Logger.LogError(new LogRequest
                                    {
                                        OperationName = this.GetType().Name,
                                        MethodName = "Execute",
                                        Message = "Error occurred while processing a Client Email Contact.",
                                        Exception = ex,
                                        AutomonData = JsonConvert.SerializeObject(offenderEmailDetails),
                                        NexusData = JsonConvert.SerializeObject(contact)
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
                    Message = "Error occurred while processing Email Contacts.",
                    Exception = ex,
                    AutomonData = JsonConvert.SerializeObject(allOffenderEmails)
                });
            }

            Logger.LogInfo(new LogRequest
            {
                OperationName = this.GetType().Name,
                MethodName = "Execute",
                Message = "Email Contact processing completed.",
                CustomParams = JsonConvert.SerializeObject(taskExecutionStatus)
            });

            return taskExecutionStatus;
        }

        protected override void LoadLookupData()
        {
            //load ContactTypes lookup data
            try
            {
                if (LookupService.ContactTypes != null)
                {
                    Logger.LogDebug(new LogRequest
                    {
                        OperationName = this.GetType().Name,
                        MethodName = "LoadLookupData",
                        Message = "Successfully retrieved ContactTypes from lookup",
                        CustomParams = JsonConvert.SerializeObject(LookupService.ContactTypes)
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(new LogRequest
                {
                    OperationName = this.GetType().Name,
                    MethodName = "LoadLookupData",
                    Message = "Error occurred while loading ContactTypes lookup data",
                    Exception = ex
                });
            }
        }

        private CrudActionType GetCrudActionType(Contact contact, IEnumerable<Contact> contacts)
        {
            //check if list is null, YES = return Add Action type
            if (contacts == null)
            {
                return CrudActionType.Add;
            }

            //try to get existing record using ClientId & ContactId
            Contact existingContact = contacts.Where(a
                =>
                    a.ClientId.Equals(contact.ClientId, StringComparison.InvariantCultureIgnoreCase)
                    && a.ContactId.Equals(contact.ContactId, StringComparison.InvariantCultureIgnoreCase)
            )
            .FirstOrDefault();

            //check if record already exists
            if (existingContact == null)
            {
                //record does not exist.
                //check if record is active. YES = return Add action type, NO = return None action type
                if (contact.IsActive)
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
                if (contact.IsActive)
                {
                    //record is active.
                    //compare with existing record. Equal = return None action type, Not Equal = return Update action type
                    if (contact.Equals(existingContact))
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
