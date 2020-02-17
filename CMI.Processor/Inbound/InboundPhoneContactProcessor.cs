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
    public class InboundPhoneContactProcessor : InboundBaseProcessor
    {
        private readonly IOffenderPhoneService offenderPhoneService;
        private readonly IContactService contactService;

        public InboundPhoneContactProcessor(
            IServiceProvider serviceProvider,
            IConfiguration configuration,
            IOffenderPhoneService offenderPhoneService,
            IContactService contactService
        )
            : base(serviceProvider, configuration)
        {
            this.offenderPhoneService = offenderPhoneService;
            this.contactService = contactService;
        }

        public override TaskExecutionStatus Execute(DateTime? lastExecutionDateTime, IEnumerable<string> officerLogonsToFilter)
        {
            
            Logger.LogInfo(new LogRequest
            {
                OperationName = this.GetType().Name,
                MethodName = "Execute",
                Message = "Phone Contact processing initiated."
            });

            //load required lookup data
            LoadLookupData();

            IEnumerable<OffenderPhone> allOffenderPhones = null;
            TaskExecutionStatus taskExecutionStatus = new TaskExecutionStatus { ProcessorType = Common.Notification.ProcessorType.Inbound, TaskName = "Process Phone Contacts" };

            try
            {
                //retrieve data from Automon for processing
                allOffenderPhones = offenderPhoneService.GetAllOffenderPhones(ProcessorConfig.CmiDbConnString, lastExecutionDateTime, GetOfficerLogonToFilterDataTable(officerLogonsToFilter));

                //check if there are any records to process
                if (allOffenderPhones.Any())
                {
                    //log number of records received from Automon
                    Logger.LogDebug(new LogRequest
                    {
                        OperationName = this.GetType().Name,
                        MethodName = "Execute",
                        Message = "Offender Phone records received from Automon.",
                        CustomParams = allOffenderPhones.Count().ToString()
                    });

                    //retrieve distinct list of offender pin
                    List<string> distinctOffenderPin = allOffenderPhones.Select(p => p.Pin).Distinct().ToList();

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
                                allExistingContactDetails = allExistingContactDetails.Where(e => !e.ContactType.Equals(DAL.Constants.ContactTypeEmailNexus, StringComparison.InvariantCultureIgnoreCase)).ToList();
                                //set ClientId value
                                allExistingContactDetails.ForEach(ea => ea.ClientId = currentOffenderPin);
                            }

                            //iterate through each of offender phone details for current offender pin
                            foreach (var offenderPhoneDetails in allOffenderPhones.Where(a => a.Pin.Equals(currentOffenderPin, StringComparison.InvariantCultureIgnoreCase)))
                            {
                                taskExecutionStatus.AutomonReceivedRecordCount++;

                                Contact contact = null;
                                try
                                {
                                    //transform offender phone details in Nexus compliant model
                                    contact = new Contact()
                                    {
                                        ClientId = FormatId(offenderPhoneDetails.Pin),
                                        ContactId = string.Format("{0}-{1}", FormatId(offenderPhoneDetails.Pin), offenderPhoneDetails.Id),
                                        ContactType = MapContactType(offenderPhoneDetails.PhoneNumberType),
                                        ContactValue = offenderPhoneDetails.Phone,
                                        Comment = FormatComment(offenderPhoneDetails.Comment),
                                        IsPrimary = offenderPhoneDetails.IsPrimary,
                                        IsActive = offenderPhoneDetails.IsActive
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
                                                    Message = "New Client Phone Contact details added successfully.",
                                                    AutomonData = JsonConvert.SerializeObject(offenderPhoneDetails),
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
                                                    Message = "Existing Client Phone Contact details updated successfully.",
                                                    AutomonData = JsonConvert.SerializeObject(offenderPhoneDetails),
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
                                                    Message = "Existing Client Phone Contact details deleted successfully.",
                                                    AutomonData = JsonConvert.SerializeObject(offenderPhoneDetails),
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
                                        Message = "Error occurred in API while processing a Client Phone Contact.",
                                        Exception = ce,
                                        AutomonData = JsonConvert.SerializeObject(offenderPhoneDetails),
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
                                        Message = "Error occurred while processing a Client Phone Contact.",
                                        Exception = ex,
                                        AutomonData = JsonConvert.SerializeObject(offenderPhoneDetails),
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
                    Message = "Error occurred while processing Phone Contacts.",
                    Exception = ex,
                    AutomonData = JsonConvert.SerializeObject(allOffenderPhones)
                });
            }

            Logger.LogInfo(new LogRequest
            {
                OperationName = this.GetType().Name,
                MethodName = "Execute",
                Message = "Phone Contact processing completed.",
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

        private string MapContactType(string automonContactType)
        {
            string nexusContactType = string.Empty;

            switch (automonContactType)
            {
                case "Fax":
                    nexusContactType = "Fax";
                    break;
                case "Mobile":
                case "Message":
                    nexusContactType = "Cell Phone";
                    break;
                case "Residential":
                    nexusContactType = "Home Phone";
                    break;
                case "Office":
                    nexusContactType = "Work Phone";
                    break;
                default:
                    nexusContactType = "Other";
                    break;
            }

            return nexusContactType;
        }

        private string FormatComment(string automonComment)
        {
            if (string.IsNullOrEmpty(automonComment))
            {
                return null;
            }

            string formattedComment = automonComment.Replace(Environment.NewLine, " ").Replace("\"", @"""").Replace("+", string.Empty);

            if (formattedComment.Length > 200)
            {
                formattedComment = formattedComment.Take(200).ToString();

                Logger.LogDebug(new LogRequest
                {
                    OperationName = this.GetType().Name,
                    MethodName = "Execute",
                    Message = "Phone Contact Comment found to be greater than 200 characters.",
                    AutomonData = JsonConvert.SerializeObject(automonComment),
                    NexusData = JsonConvert.SerializeObject(formattedComment)
                });
            }

            return formattedComment;
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
