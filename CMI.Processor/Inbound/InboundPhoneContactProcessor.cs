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

        public override Common.Notification.TaskExecutionStatus Execute(DateTime? lastExecutionDateTime, IEnumerable<string> officerLogonsToFilter)
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
            Common.Notification.TaskExecutionStatus taskExecutionStatus = new Common.Notification.TaskExecutionStatus { ProcessorType = ProcessorType.Inbound, TaskName = "Process Phone Contacts" };

            try
            {
                allOffenderPhones = offenderPhoneService.GetAllOffenderPhones(ProcessorConfig.CmiDbConnString, lastExecutionDateTime, GetOfficerLogonToFilterDataTable(officerLogonsToFilter));

                //log number of records received from Automon
                if (allOffenderPhones.Any())
                {
                    Logger.LogDebug(new LogRequest
                    {
                        OperationName = this.GetType().Name,
                        MethodName = "Execute",
                        Message = "Offender Phone records received from Automon.",
                        CustomParams = allOffenderPhones.Count().ToString()
                    });
                }

                foreach (var offenderPhoneDetails in allOffenderPhones)
                {
                    taskExecutionStatus.AutomonReceivedRecordCount++;

                    Contact contact = null;
                    try
                    {
                        contact = new Contact()
                        {
                            ClientId = FormatId(offenderPhoneDetails.Pin),
                            ContactId = string.Format("{0}-{1}", FormatId(offenderPhoneDetails.Pin), offenderPhoneDetails.Id),
                            ContactType = MapContactType(offenderPhoneDetails.PhoneNumberType),
                            ContactValue = offenderPhoneDetails.Phone,
                            IsPrimary = offenderPhoneDetails.IsPrimary,
                            IsActive = offenderPhoneDetails.IsActive,
                            Comment = 
                                string.IsNullOrEmpty(offenderPhoneDetails.Comment)
                                ? null
                                : offenderPhoneDetails.Comment.Replace(Environment.NewLine, " ").Replace("\"", @"""")
                        };

                        if (ClientService.GetClientDetails(contact.ClientId) != null)
                        {
                            if (contactService.GetContactDetails(contact.ClientId, contact.ContactId) == null)
                            {
                                if (contact.IsActive && contactService.AddNewContactDetails(contact))
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
                                else
                                {
                                    taskExecutionStatus.AutomonReceivedRecordCount--;
                                }
                            }
                            else if (!contact.IsActive)
                            {
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
                            }
                            else
                            {
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
                taskExecutionStatus.IsSuccessful = taskExecutionStatus.NexusFailureRecordCount == 0;
            }
            catch (Exception ex)
            {
                taskExecutionStatus.IsSuccessful = false;

                Logger.LogError(new LogRequest
                {
                    OperationName = this.GetType().Name,
                    MethodName = "Execute",
                    Message = "Error occurred while processing Contacts.",
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
    }
}
