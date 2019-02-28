﻿using CMI.Automon.Interface;
using CMI.Automon.Model;
using CMI.Common.Logging;
using CMI.Nexus.Interface;
using CMI.Nexus.Model;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

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

        public override Common.Notification.TaskExecutionStatus Execute()
        {
            Logger.LogInfo(new LogRequest
            {
                OperationName = "Processor",
                MethodName = "ProcessEmailContacts",
                Message = "Email Contact processing initiated."
            });

            IEnumerable<OffenderEmail> allOffenderEmails = null;
            Common.Notification.TaskExecutionStatus taskExecutionStatus = new Common.Notification.TaskExecutionStatus { TaskName = "Process Email Contacts" };

            try
            {
                allOffenderEmails = offenderEmailService.GetAllOffenderEmails(ProcessorConfig.CmiDbConnString, LastExecutionDateTime);

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
                                        OperationName = "Processor",
                                        MethodName = "ProcessEmailContacts",
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
                                        OperationName = "Processor",
                                        MethodName = "ProcessEmailContacts",
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
                                        OperationName = "Processor",
                                        MethodName = "ProcessEmailContacts",
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
                            OperationName = "Processor",
                            MethodName = "ProcessEmailContacts",
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
                            OperationName = "Processor",
                            MethodName = "ProcessEmailContacts",
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
                    OperationName = "Processor",
                    MethodName = "ProcessEmailContacts",
                    Message = "Error occurred while processing Email Contacts.",
                    Exception = ex,
                    AutomonData = JsonConvert.SerializeObject(allOffenderEmails)
                });
            }

            Logger.LogInfo(new LogRequest
            {
                OperationName = "Processor",
                MethodName = "ProcessEmailContacts",
                Message = "Email Contact processing completed.",
                CustomParams = JsonConvert.SerializeObject(taskExecutionStatus)
            });

            return taskExecutionStatus;
        }
    }
}