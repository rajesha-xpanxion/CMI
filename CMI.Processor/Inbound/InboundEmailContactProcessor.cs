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

        public override Common.Notification.TaskExecutionStatus Execute(DateTime? lastExecutionDateTime, IEnumerable<string> officerLogonsToFilter)
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
            Common.Notification.TaskExecutionStatus taskExecutionStatus = new Common.Notification.TaskExecutionStatus { ProcessorType = ProcessorType.Inbound, TaskName = "Process Email Contacts" };

            try
            {
                allOffenderEmails = offenderEmailService.GetAllOffenderEmails(ProcessorConfig.CmiDbConnString, lastExecutionDateTime, GetOfficerLogonToFilterDataTable(officerLogonsToFilter));

                //log number of records received from Automon
                Logger.LogDebug(new LogRequest
                {
                    OperationName = this.GetType().Name,
                    MethodName = "Execute",
                    Message = "Offender Email records received from Automon.",
                    CustomParams = allOffenderEmails.Count().ToString()
                });

                foreach (var offenderEmailDetails in allOffenderEmails)
                {
                    taskExecutionStatus.AutomonReceivedRecordCount++;

                    Contact contact = null;
                    try
                    {
                        contact = new Contact()
                        {
                            ClientId = FormatId(offenderEmailDetails.Pin),
                            ContactId = string.Format("{0}-{1}", FormatId(offenderEmailDetails.Pin), offenderEmailDetails.Id),
                            ContactType = DAL.Constants.ContactTypeEmailNexus,
                            ContactValue = offenderEmailDetails.EmailAddress,
                            IsPrimary = offenderEmailDetails.IsPrimary,
                            IsActive = offenderEmailDetails.IsActive
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
                                        Message = "New Client Email Contact details added successfully.",
                                        AutomonData = JsonConvert.SerializeObject(offenderEmailDetails),
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
                                        Message = "Existing Client Email Contact details deleted successfully.",
                                        AutomonData = JsonConvert.SerializeObject(offenderEmailDetails),
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
                                        Message = "Existing Client Email Contact details updated successfully.",
                                        AutomonData = JsonConvert.SerializeObject(offenderEmailDetails),
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
    }
}
