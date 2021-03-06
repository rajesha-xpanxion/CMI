﻿using CMI.DAL.Dest;
using CMI.DAL.Source;
using System;
using System.Collections.Generic;
using System.Linq;
using CMI.DAL.Dest.Models;
using CMI.Common.Logging;
using Newtonsoft.Json;
using Microsoft.Extensions.Options;

namespace CMI.Processor
{
    public class Processor
    {
        #region Private Member Variables
        #region Source Service Providers
        private readonly IOffenderService offenderService;
        private readonly IOffenderAddressService offenderAddressService;
        private readonly IOffenderPhoneService offenderPhoneService;
        private readonly IOffenderEmailService offenderEmailService;
        private readonly IOffenderCaseService offenderCaseService;
        private readonly IOffenderNoteService offenderNoteService;
        #endregion

        #region Destination Service Providers
        private readonly IClientService clientService;
        private readonly IAddressService addressService;
        private readonly IContactService contactService;
        private readonly ICaseService caseService;
        private readonly INoteService noteService;
        private readonly ILookupService lookupService;
        #endregion

        DateTime? lastExecutionDateTime;
        private readonly DAL.ExecutionStatus processorExecutionStatus = null;

        private readonly ILogger logger;
        private readonly DAL.IProcessorProvider processorProvider;
        private readonly Common.Notification.IEmailNotificationProvider emailNotificationProvider;

        private readonly DAL.ProcessorConfig processorConfig;

        private readonly List<Common.Notification.TaskExecutionStatus> taskExecutionStatuses = null;
        #endregion

        #region Constructor
        public Processor(
            //source service providers
            IOffenderService offenderService,
            IOffenderAddressService offenderAddressService,
            IOffenderPhoneService offenderPhoneService,
            IOffenderEmailService offenderEmailService,
            IOffenderCaseService offenderCaseService,
            IOffenderNoteService offenderNoteService,
            //destination service providers
            IClientService clientService,
            IAddressService addressService,
            IContactService contactService,
            ICaseService caseService,
            INoteService noteService,
            ILookupService lookupService,

            ILogger logger,

            DAL.IProcessorProvider processorProvider,
            Common.Notification.IEmailNotificationProvider emailNotificationProvider,
            IOptions<DAL.ProcessorConfig> processorConfig
        )
        {
            //source service providers
            this.offenderService = offenderService;
            this.offenderAddressService = offenderAddressService;
            this.offenderPhoneService = offenderPhoneService;
            this.offenderEmailService = offenderEmailService;
            this.offenderCaseService = offenderCaseService;
            this.offenderNoteService = offenderNoteService;
            //destination service providers
            this.clientService = clientService;
            this.addressService = addressService;
            this.contactService = contactService;
            this.caseService = caseService;
            this.noteService = noteService;
            this.lookupService = lookupService;

            this.logger = logger;
            this.processorProvider = processorProvider;
            this.emailNotificationProvider = emailNotificationProvider;

            this.processorConfig = processorConfig.Value;

            taskExecutionStatuses = new List<Common.Notification.TaskExecutionStatus>();
            processorExecutionStatus = new DAL.ExecutionStatus { ExecutedOn = DateTime.Now, IsSuccessful = true, NumTaskProcessed = 0, NumTaskSucceeded = 0, NumTaskFailed = 0 };
        }
        #endregion

        #region Public Methods
        public void Execute()
        {
            //log info message for start of processing
            logger.LogInfo(new LogRequest
            {
                OperationName = "Processor",
                MethodName = "Execute",
                Message = "Processor execution initiated."
            });

            //get last execution date time so that differential dataset is pulled from source
            RetrieveLastExecutionDateTime();

            //first loads required lookup data
            LoadLookupData();

            //process client profiles
            if (processorConfig.StagesToProcess != null && processorConfig.StagesToProcess.Any(a => a.Equals(DAL.ProcessorStage.ProcessClientProfiles, StringComparison.InvariantCultureIgnoreCase)))
            {
                UpdateExecutionStatus(ProcessClientProfiles());
            }

            //process client addresses
            if (processorConfig.StagesToProcess != null && processorConfig.StagesToProcess.Any(a => a.Equals(DAL.ProcessorStage.ProcessAddresses, StringComparison.InvariantCultureIgnoreCase)))
            {
                UpdateExecutionStatus(ProcessAddresses());
            }

            //process client phone contacts
            if (processorConfig.StagesToProcess != null && processorConfig.StagesToProcess.Any(a => a.Equals(DAL.ProcessorStage.ProcessPhoneContacts, StringComparison.InvariantCultureIgnoreCase)))
            {
                UpdateExecutionStatus(ProcessPhoneContacts());
            }

            //process client email contacts
            if (processorConfig.StagesToProcess != null && processorConfig.StagesToProcess.Any(a => a.Equals(DAL.ProcessorStage.ProcessEmailContacts, StringComparison.InvariantCultureIgnoreCase)))
            {
                UpdateExecutionStatus(ProcessEmailContacts());
            }

            //process client cases
            if (processorConfig.StagesToProcess != null && processorConfig.StagesToProcess.Any(a => a.Equals(DAL.ProcessorStage.ProcessCases, StringComparison.InvariantCultureIgnoreCase)))
            {
                UpdateExecutionStatus(ProcessCases());
            }

            //process client notes
            if (processorConfig.StagesToProcess != null && processorConfig.StagesToProcess.Any(a => a.Equals(DAL.ProcessorStage.ProcessNotes, StringComparison.InvariantCultureIgnoreCase)))
            {
                UpdateExecutionStatus(ProcessNotes());
            }

            //derive final processor execution status and save it to database
            processorExecutionStatus.ExecutionStatusMessage = processorExecutionStatus.IsSuccessful 
                ? "Processor execution completed successfully." 
                : "Processor execution failed. Please check logs for more details.";
            
            //save execution status details in history table
            SaveExecutionStatus(processorExecutionStatus);

            //send execution status report email
            var executionStatusReportEmailRequest = new Common.Notification.ExecutionStatusReportEmailRequest
            {
                ToEmailAddress = processorConfig.ExecutionStatusReportReceiverEmailAddresses,
                Subject = processorConfig.ExecutionStatusReportEmailSubject,
                TaskExecutionStatuses = taskExecutionStatuses
            };

            var response = emailNotificationProvider.SendExecutionStatusReportEmail(executionStatusReportEmailRequest);

            if (response.IsSuccessful)
            {
                logger.LogInfo(new LogRequest
                {
                    OperationName = "Processor",
                    MethodName = "Execute",
                    Message = "Execution status report email sent successfully.",
                    CustomParams = JsonConvert.SerializeObject(executionStatusReportEmailRequest)
                });
            }
            else
            {
                logger.LogWarning(new LogRequest
                {
                    OperationName = "Processor",
                    MethodName = "Execute",
                    Message = "Error occurred while sending execution status report email.",
                    Exception = response.Exception,
                    CustomParams = JsonConvert.SerializeObject(executionStatusReportEmailRequest)
                });
            }

            //log info message for end of processing
            logger.LogInfo(new LogRequest
            {
                OperationName = "Processor",
                MethodName = "Execute",
                Message = "Processor execution completed.",
                CustomParams = JsonConvert.SerializeObject(processorExecutionStatus)
            });
        }
        #endregion

        #region Private Processor Methods
        private Common.Notification.TaskExecutionStatus ProcessClientProfiles()
        {
            logger.LogInfo(new LogRequest
            {
                OperationName = "Processor",
                MethodName = "ProcessClientProfiles",
                Message = "Client Profile processing initiated."
            });

            IEnumerable<Offender> allOffenderDetails = null;
            Common.Notification.TaskExecutionStatus taskExecutionStatus = new Common.Notification.TaskExecutionStatus { TaskName = "Process Client Profiles" };

            try
            {
                allOffenderDetails = offenderService.GetAllOffenderDetails(processorConfig.CmiDbConnString, lastExecutionDateTime);

                foreach (var offenderDetails in allOffenderDetails)
                {
                    taskExecutionStatus.SourceReceivedRecordCount++;

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

                        if (clientService.GetClientDetails(client.IntegrationId) == null)
                        {
                            if (clientService.AddNewClientDetails(client))
                            {
                                taskExecutionStatus.DestAddRecordCount++;

                                logger.LogDebug(new LogRequest
                                {
                                    OperationName = "Processor",
                                    MethodName = "ProcessClientProfiles",
                                    Message = "New Client Profile added successfully.",
                                    SourceData = JsonConvert.SerializeObject(offenderDetails),
                                    DestData = JsonConvert.SerializeObject(client)
                                });
                            }
                        }
                        else
                        {
                            if (clientService.UpdateClientDetails(client))
                            {
                                taskExecutionStatus.DestUpdateRecordCount++;

                                logger.LogDebug(new LogRequest
                                {
                                    OperationName = "Processor",
                                    MethodName = "ProcessClientProfiles",
                                    Message = "Existing Client Profile updated successfully.",
                                    SourceData = JsonConvert.SerializeObject(offenderDetails),
                                    DestData = JsonConvert.SerializeObject(client)
                                });
                            }
                        }
                    }
                    catch (CmiException ce)
                    {
                        taskExecutionStatus.DestFailureRecordCount++;

                        logger.LogWarning(new LogRequest
                        {
                            OperationName = "Processor",
                            MethodName = "ProcessClientProfiles",
                            Message = "Error occurred in API while processing a Client Profile.",
                            Exception = ce,
                            SourceData = JsonConvert.SerializeObject(offenderDetails),
                            DestData = JsonConvert.SerializeObject(client)
                        });
                    }
                    catch (Exception ex)
                    {
                        taskExecutionStatus.DestFailureRecordCount++;

                        logger.LogError(new LogRequest
                        {
                            OperationName = "Processor",
                            MethodName = "ProcessClientProfiles",
                            Message = "Error occurred while processing a Client Profile.",
                            Exception = ex,
                            SourceData = JsonConvert.SerializeObject(offenderDetails),
                            DestData = JsonConvert.SerializeObject(client)
                        });
                    }
                }

                taskExecutionStatus.IsSuccessful = taskExecutionStatus.DestFailureRecordCount == 0;
            }
            catch (Exception ex)
            {
                taskExecutionStatus.IsSuccessful = false;
                logger.LogError(new LogRequest
                {
                    OperationName = "Processor",
                    MethodName = "ProcessClientProfiles",
                    Message = "Error occurred while processing Client Profiles.",
                    Exception = ex,
                    SourceData = JsonConvert.SerializeObject(allOffenderDetails)
                });
            }

            logger.LogInfo(new LogRequest
            {
                OperationName = "Processor",
                MethodName = "ProcessClientProfiles",
                Message = "Client Profile processing completed.",
                CustomParams = JsonConvert.SerializeObject(taskExecutionStatus)
            });

            return taskExecutionStatus;
        }

        private Common.Notification.TaskExecutionStatus ProcessAddresses()
        {
            logger.LogInfo(new LogRequest
            {
                OperationName = "Processor",
                MethodName = "ProcessAddresses",
                Message = "Address processing initiated."
            });

            IEnumerable<OffenderAddress> allOffenderAddresses = null;
            Common.Notification.TaskExecutionStatus taskExecutionStatus = new Common.Notification.TaskExecutionStatus { TaskName = "Process Addresses" };

            try
            {
                allOffenderAddresses = offenderAddressService.GetAllOffenderAddresses(processorConfig.CmiDbConnString, lastExecutionDateTime);

                foreach (var offenderAddressDetails in allOffenderAddresses)
                {
                    taskExecutionStatus.SourceReceivedRecordCount++;

                    Address address = null;
                    try
                    {
                        address = new Address()
                        {
                            ClientId = FormatId(offenderAddressDetails.Pin),
                            AddressId = string.Format("{0}-{1}", FormatId(offenderAddressDetails.Pin), offenderAddressDetails.Id),
                            AddressType = MapAddressType(offenderAddressDetails.AddressType),
                            FullAddress = MapFullAddress(offenderAddressDetails.Line1, offenderAddressDetails.Line2, offenderAddressDetails.City, offenderAddressDetails.State, offenderAddressDetails.Zip),
                            IsPrimary = offenderAddressDetails.IsPrimary,
                            Comment = string.IsNullOrEmpty(offenderAddressDetails.Comment) ? offenderAddressDetails.Comment : offenderAddressDetails.Comment.Replace("/", "-"),
                            IsActive = offenderAddressDetails.IsActive
                        };

                        if (clientService.GetClientDetails(address.ClientId) != null)
                        {
                            if (addressService.GetAddressDetails(address.ClientId, address.AddressId) == null)
                            {
                                if (address.IsActive && addressService.AddNewAddressDetails(address))
                                {
                                    taskExecutionStatus.DestAddRecordCount++;

                                    logger.LogDebug(new LogRequest
                                    {
                                        OperationName = "Processor",
                                        MethodName = "ProcessAddresses",
                                        Message = "New Client Address details added successfully.",
                                        SourceData = JsonConvert.SerializeObject(offenderAddressDetails),
                                        DestData = JsonConvert.SerializeObject(address)
                                    });
                                }
                                else
                                {
                                    taskExecutionStatus.SourceReceivedRecordCount--;
                                }
                            }
                            else if (!address.IsActive)
                            {
                                if (addressService.DeleteAddressDetails(address.ClientId, address.AddressId))
                                {
                                    taskExecutionStatus.DestDeleteRecordCount++;

                                    logger.LogDebug(new LogRequest
                                    {
                                        OperationName = "Processor",
                                        MethodName = "ProcessAddresses",
                                        Message = "Existing Client Address details deleted successfully.",
                                        SourceData = JsonConvert.SerializeObject(offenderAddressDetails),
                                        DestData = JsonConvert.SerializeObject(address)
                                    });
                                }
                            }
                            else
                            {
                                if (addressService.UpdateAddressDetails(address))
                                {
                                    taskExecutionStatus.DestUpdateRecordCount++;

                                    logger.LogDebug(new LogRequest
                                    {
                                        OperationName = "Processor",
                                        MethodName = "ProcessAddresses",
                                        Message = "Existing Client Address details updated successfully.",
                                        SourceData = JsonConvert.SerializeObject(offenderAddressDetails),
                                        DestData = JsonConvert.SerializeObject(address)
                                    });
                                }
                            }
                        }
                        else
                        {
                            taskExecutionStatus.SourceReceivedRecordCount--;
                        }
                    }
                    catch (CmiException ce)
                    {
                        taskExecutionStatus.DestFailureRecordCount++;

                        logger.LogWarning(new LogRequest
                        {
                            OperationName = "Processor",
                            MethodName = "ProcessAddresses",
                            Message = "Error occurred in API while processing a Client Address.",
                            Exception = ce,
                            SourceData = JsonConvert.SerializeObject(offenderAddressDetails),
                            DestData = JsonConvert.SerializeObject(address)
                        });
                    }
                    catch (Exception ex)
                    {
                        taskExecutionStatus.DestFailureRecordCount++;

                        logger.LogError(new LogRequest
                        {
                            OperationName = "Processor",
                            MethodName = "ProcessAddresses",
                            Message = "Error occurred while processing a Client Address.",
                            Exception = ex,
                            SourceData = JsonConvert.SerializeObject(offenderAddressDetails),
                            DestData = JsonConvert.SerializeObject(address)
                        });
                    }
                }

                taskExecutionStatus.IsSuccessful = taskExecutionStatus.DestFailureRecordCount == 0;
            }
            catch (Exception ex)
            {
                taskExecutionStatus.IsSuccessful = false;

                logger.LogError(new LogRequest
                {
                    OperationName = "Processor",
                    MethodName = "ProcessAddresses",
                    Message = "Error occurred while processing Addresses.",
                    Exception = ex,
                    SourceData = JsonConvert.SerializeObject(allOffenderAddresses)
                });
            }

            logger.LogInfo(new LogRequest
            {
                OperationName = "Processor",
                MethodName = "ProcessAddresses",
                Message = "Addresses processing completed.",
                CustomParams = JsonConvert.SerializeObject(taskExecutionStatus)
            });

            return taskExecutionStatus;
        }

        private Common.Notification.TaskExecutionStatus ProcessPhoneContacts()
        {
            logger.LogInfo(new LogRequest
            {
                OperationName = "Processor",
                MethodName = "ProcessPhoneContacts",
                Message = "Phone Contact processing initiated."
            });

            IEnumerable<OffenderPhone> allOffenderPhones = null;
            Common.Notification.TaskExecutionStatus taskExecutionStatus = new Common.Notification.TaskExecutionStatus { TaskName = "Process Phone Contacts" };

            try
            {
                allOffenderPhones = offenderPhoneService.GetAllOffenderPhones(processorConfig.CmiDbConnString, lastExecutionDateTime);

                foreach (var offenderPhoneDetails in allOffenderPhones)
                {
                    taskExecutionStatus.SourceReceivedRecordCount++;

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
                            Comment = string.IsNullOrEmpty(offenderPhoneDetails.Comment) ? offenderPhoneDetails.Comment : offenderPhoneDetails.Comment.Replace("/", "-"),
                            IsActive = offenderPhoneDetails.IsActive
                        };

                        if (clientService.GetClientDetails(contact.ClientId) != null)
                        {
                            if (contactService.GetContactDetails(contact.ClientId, contact.ContactId) == null)
                            {
                                if (contact.IsActive && contactService.AddNewContactDetails(contact))
                                {
                                    taskExecutionStatus.DestAddRecordCount++;

                                    logger.LogDebug(new LogRequest
                                    {
                                        OperationName = "Processor",
                                        MethodName = "ProcessPhoneContacts",
                                        Message = "New Client Phone Contact details added successfully.",
                                        SourceData = JsonConvert.SerializeObject(offenderPhoneDetails),
                                        DestData = JsonConvert.SerializeObject(contact)
                                    });
                                }
                                else
                                {
                                    taskExecutionStatus.SourceReceivedRecordCount--;
                                }
                            }
                            else if (!contact.IsActive)
                            {
                                if (contactService.DeleteContactDetails(contact.ClientId, contact.ContactId))
                                {
                                    taskExecutionStatus.DestDeleteRecordCount++;

                                    logger.LogDebug(new LogRequest
                                    {
                                        OperationName = "Processor",
                                        MethodName = "ProcessPhoneContacts",
                                        Message = "Existing Client Phone Contact details deleted successfully.",
                                        SourceData = JsonConvert.SerializeObject(offenderPhoneDetails),
                                        DestData = JsonConvert.SerializeObject(contact)
                                    });
                                }
                            }
                            else
                            {
                                if (contactService.UpdateContactDetails(contact))
                                {
                                    taskExecutionStatus.DestUpdateRecordCount++;

                                    logger.LogDebug(new LogRequest
                                    {
                                        OperationName = "Processor",
                                        MethodName = "ProcessPhoneContacts",
                                        Message = "Existing Client Phone Contact details updated successfully.",
                                        SourceData = JsonConvert.SerializeObject(offenderPhoneDetails),
                                        DestData = JsonConvert.SerializeObject(contact)
                                    });
                                }
                            }
                        }
                        else
                        {
                            taskExecutionStatus.SourceReceivedRecordCount--;
                        }
                    }
                    catch (CmiException ce)
                    {
                        taskExecutionStatus.DestFailureRecordCount++;

                        logger.LogWarning(new LogRequest
                        {
                            OperationName = "Processor",
                            MethodName = "ProcessPhoneContacts",
                            Message = "Error occurred in API while processing a Client Phone Contact.",
                            Exception = ce,
                            SourceData = JsonConvert.SerializeObject(offenderPhoneDetails),
                            DestData = JsonConvert.SerializeObject(contact)
                        });
                    }
                    catch (Exception ex)
                    {
                        taskExecutionStatus.DestFailureRecordCount++;

                        logger.LogError(new LogRequest
                        {
                            OperationName = "Processor",
                            MethodName = "ProcessPhoneContacts",
                            Message = "Error occurred while processing a Client Phone Contact.",
                            Exception = ex,
                            SourceData = JsonConvert.SerializeObject(offenderPhoneDetails),
                            DestData = JsonConvert.SerializeObject(contact)
                        });
                    }
                }
                taskExecutionStatus.IsSuccessful = taskExecutionStatus.DestFailureRecordCount == 0;
            }
            catch (Exception ex)
            {
                taskExecutionStatus.IsSuccessful = false;

                logger.LogError(new LogRequest
                {
                    OperationName = "Processor",
                    MethodName = "ProcessPhoneContacts",
                    Message = "Error occurred while processing Contacts.",
                    Exception = ex,
                    SourceData = JsonConvert.SerializeObject(allOffenderPhones)
                });
            }

            logger.LogInfo(new LogRequest
            {
                OperationName = "Processor",
                MethodName = "ProcessPhoneContacts",
                Message = "Phone Contact processing completed.",
                CustomParams = JsonConvert.SerializeObject(taskExecutionStatus)
            });

            return taskExecutionStatus;
        }

        private Common.Notification.TaskExecutionStatus ProcessEmailContacts()
        {
            logger.LogInfo(new LogRequest
            {
                OperationName = "Processor",
                MethodName = "ProcessEmailContacts",
                Message = "Email Contact processing initiated."
            });

            IEnumerable<OffenderEmail> allOffenderEmails = null;
            Common.Notification.TaskExecutionStatus taskExecutionStatus = new Common.Notification.TaskExecutionStatus { TaskName = "Process Email Contacts" };

            try
            {
                allOffenderEmails = offenderEmailService.GetAllOffenderEmails(processorConfig.CmiDbConnString, lastExecutionDateTime);

                foreach (var offenderEmailDetails in allOffenderEmails)
                {
                    taskExecutionStatus.SourceReceivedRecordCount++;

                    Contact contact = null;
                    try
                    {
                        contact = new Contact()
                        {
                            ClientId = FormatId(offenderEmailDetails.Pin),
                            ContactId = string.Format("{0}-{1}", FormatId(offenderEmailDetails.Pin), offenderEmailDetails.Id),
                            ContactType = DAL.Constants.ContactTypeEmailDest,
                            ContactValue = offenderEmailDetails.EmailAddress,
                            IsPrimary = offenderEmailDetails.IsPrimary,
                            IsActive = offenderEmailDetails.IsActive
                        };

                        if (clientService.GetClientDetails(contact.ClientId) != null)
                        {
                            if (contactService.GetContactDetails(contact.ClientId, contact.ContactId) == null)
                            {
                                if (contact.IsActive && contactService.AddNewContactDetails(contact))
                                {
                                    taskExecutionStatus.DestAddRecordCount++;

                                    logger.LogDebug(new LogRequest
                                    {
                                        OperationName = "Processor",
                                        MethodName = "ProcessEmailContacts",
                                        Message = "New Client Email Contact details added successfully.",
                                        SourceData = JsonConvert.SerializeObject(offenderEmailDetails),
                                        DestData = JsonConvert.SerializeObject(contact)
                                    });
                                }
                                else
                                {
                                    taskExecutionStatus.SourceReceivedRecordCount--;
                                }
                            }
                            else if (!contact.IsActive)
                            {
                                if (contactService.DeleteContactDetails(contact.ClientId, contact.ContactId))
                                {
                                    taskExecutionStatus.DestDeleteRecordCount++;

                                    logger.LogDebug(new LogRequest
                                    {
                                        OperationName = "Processor",
                                        MethodName = "ProcessEmailContacts",
                                        Message = "Existing Client Email Contact details deleted successfully.",
                                        SourceData = JsonConvert.SerializeObject(offenderEmailDetails),
                                        DestData = JsonConvert.SerializeObject(contact)
                                    });
                                }
                            }
                            else
                            {
                                if (contactService.UpdateContactDetails(contact))
                                {
                                    taskExecutionStatus.DestUpdateRecordCount++;

                                    logger.LogDebug(new LogRequest
                                    {
                                        OperationName = "Processor",
                                        MethodName = "ProcessEmailContacts",
                                        Message = "Existing Client Email Contact details updated successfully.",
                                        SourceData = JsonConvert.SerializeObject(offenderEmailDetails),
                                        DestData = JsonConvert.SerializeObject(contact)
                                    });
                                }
                            }
                        }
                        else
                        {
                            taskExecutionStatus.SourceReceivedRecordCount--;
                        }
                    }
                    catch (CmiException ce)
                    {
                        taskExecutionStatus.DestFailureRecordCount++;

                        logger.LogWarning(new LogRequest
                        {
                            OperationName = "Processor",
                            MethodName = "ProcessEmailContacts",
                            Message = "Error occurred in API while processing a Client Email Contact.",
                            Exception = ce,
                            SourceData = JsonConvert.SerializeObject(offenderEmailDetails),
                            DestData = JsonConvert.SerializeObject(contact)
                        });
                    }
                    catch (Exception ex)
                    {
                        taskExecutionStatus.DestFailureRecordCount++;

                        logger.LogError(new LogRequest
                        {
                            OperationName = "Processor",
                            MethodName = "ProcessEmailContacts",
                            Message = "Error occurred while processing a Client Email Contact.",
                            Exception = ex,
                            SourceData = JsonConvert.SerializeObject(offenderEmailDetails),
                            DestData = JsonConvert.SerializeObject(contact)
                        });
                    }
                }
                taskExecutionStatus.IsSuccessful = taskExecutionStatus.DestFailureRecordCount == 0;
            }
            catch (Exception ex)
            {
                taskExecutionStatus.IsSuccessful = false;

                logger.LogError(new LogRequest
                {
                    OperationName = "Processor",
                    MethodName = "ProcessEmailContacts",
                    Message = "Error occurred while processing Email Contacts.",
                    Exception = ex,
                    SourceData = JsonConvert.SerializeObject(allOffenderEmails)
                });
            }

            logger.LogInfo(new LogRequest
            {
                OperationName = "Processor",
                MethodName = "ProcessEmailContacts",
                Message = "Email Contact processing completed.",
                CustomParams = JsonConvert.SerializeObject(taskExecutionStatus)
            });

            return taskExecutionStatus;
        }

        private Common.Notification.TaskExecutionStatus ProcessCases()
        {
            logger.LogInfo(new LogRequest
            {
                OperationName = "Processor",
                MethodName = "ProcessCases",
                Message = "Case processing intiated."
            });

            IEnumerable<OffenderCase> allOffenderCaseDetails = null;
            Common.Notification.TaskExecutionStatus taskExecutionStatus = new Common.Notification.TaskExecutionStatus { TaskName = "Process Cases" };
            DateTime currentTimestamp = DateTime.Now;

            try
            {
                allOffenderCaseDetails = offenderCaseService.GetAllOffenderCases(processorConfig.CmiDbConnString, lastExecutionDateTime);

                foreach (var offenderCaseDetails in allOffenderCaseDetails.GroupBy(x => new { x.Pin, x.CaseNumber }).Select(y => y.First()))
                {
                    taskExecutionStatus.SourceReceivedRecordCount++;

                    Case @case = null;
                    try
                    {
                        @case = new Case()
                        {
                            ClientId = FormatId(offenderCaseDetails.Pin),
                            CaseNumber = offenderCaseDetails.CaseNumber,
                            CaseDate = offenderCaseDetails.CaseDate.HasValue ? offenderCaseDetails.CaseDate.Value.ToShortDateString() : currentTimestamp.ToString(),
                            StartDate = offenderCaseDetails.SupervisionStartDate.HasValue ? offenderCaseDetails.SupervisionStartDate.Value.ToShortDateString() : null,
                            EndDate = offenderCaseDetails.SupervisionEndDate.HasValue ? offenderCaseDetails.SupervisionEndDate.Value.ToShortDateString() : null,
                            Status = offenderCaseDetails.CaseStatus,
                            EndReason = string.IsNullOrEmpty(offenderCaseDetails.ClosureReason) ? null : offenderCaseDetails.ClosureReason,
                            Offenses = allOffenderCaseDetails.Where(z => z.Pin == offenderCaseDetails.Pin && z.CaseNumber == offenderCaseDetails.CaseNumber).Select(p => new Offense
                            {
                                Label = p.OffenseLabel,
                                Statute = p.OffenseStatute,
                                Date = p.OffenseDate.HasValue ? p.OffenseDate.Value.ToShortDateString() : string.Empty,
                                Category = MapOffenseCategory(p.OffenseCategory),
                                IsPrimary = p.IsPrimary
                            }).ToList()
                        };

                        if (clientService.GetClientDetails(@case.ClientId) != null)
                        {
                            if (caseService.GetCaseDetails(@case.ClientId, @case.CaseNumber) == null)
                            {
                                if (caseService.AddNewCaseDetails(@case))
                                {
                                    taskExecutionStatus.DestAddRecordCount++;

                                    logger.LogDebug(new LogRequest
                                    {
                                        OperationName = "Processor",
                                        MethodName = "ProcessCases",
                                        Message = "New Client Case details added successfully.",
                                        SourceData = JsonConvert.SerializeObject(offenderCaseDetails),
                                        DestData = JsonConvert.SerializeObject(@case)
                                    });
                                }
                            }
                            else
                            {
                                if (caseService.UpdateCaseDetails(@case))
                                {
                                    taskExecutionStatus.DestUpdateRecordCount++;

                                    logger.LogDebug(new LogRequest
                                    {
                                        OperationName = "Processor",
                                        MethodName = "ProcessCases",
                                        Message = "Existing Client Case details updated successfully.",
                                        SourceData = JsonConvert.SerializeObject(offenderCaseDetails),
                                        DestData = JsonConvert.SerializeObject(@case)
                                    });
                                }
                            }
                        }
                        else
                        {
                            taskExecutionStatus.SourceReceivedRecordCount--;
                        }
                    }
                    catch (CmiException ce)
                    {
                        taskExecutionStatus.DestFailureRecordCount++;

                        logger.LogWarning(new LogRequest
                        {
                            OperationName = "Processor",
                            MethodName = "ProcessCases",
                            Message = "Error occurred in API while processing a Client Case.",
                            Exception = ce,
                            SourceData = JsonConvert.SerializeObject(offenderCaseDetails),
                            DestData = JsonConvert.SerializeObject(@case)
                        });
                    }
                    catch (Exception ex)
                    {
                        taskExecutionStatus.DestFailureRecordCount++;

                        logger.LogError(new LogRequest
                        {
                            OperationName = "Processor",
                            MethodName = "ProcessCases",
                            Message = "Error occurred while processing a Client Case.",
                            Exception = ex,
                            SourceData = JsonConvert.SerializeObject(offenderCaseDetails),
                            DestData = JsonConvert.SerializeObject(@case)
                        });
                    }
                }
                taskExecutionStatus.IsSuccessful = taskExecutionStatus.DestFailureRecordCount == 0;
            }
            catch (Exception ex)
            {
                taskExecutionStatus.IsSuccessful = false;

                logger.LogError(new LogRequest
                {
                    OperationName = "Processor",
                    MethodName = "ProcessCases",
                    Message = "Error occurred while processing Cases.",
                    Exception = ex,
                    SourceData = JsonConvert.SerializeObject(allOffenderCaseDetails)
                });
            }

            logger.LogInfo(new LogRequest
            {
                OperationName = "Processor",
                MethodName = "ProcessCases",
                Message = "Case processing completed.",
                CustomParams = JsonConvert.SerializeObject(taskExecutionStatus)
            });

            return taskExecutionStatus;
        }

        private Common.Notification.TaskExecutionStatus ProcessNotes()
        {
            logger.LogInfo(new LogRequest
            {
                OperationName = "Processor",
                MethodName = "ProcessNotes",
                Message = "Note processing intiated."
            });

            IEnumerable<OffenderNote> allOffenderNoteDetails = null;
            Common.Notification.TaskExecutionStatus taskExecutionStatus = new Common.Notification.TaskExecutionStatus { TaskName = "Process Notes" };

            try
            {
                allOffenderNoteDetails = offenderNoteService.GetAllOffenderNotes(processorConfig.CmiDbConnString, lastExecutionDateTime);

                foreach (var offenderNoteDetails in allOffenderNoteDetails)
                {
                    taskExecutionStatus.SourceReceivedRecordCount++;

                    Note note = null;
                    try
                    {
                        note = new Note()
                        {
                            ClientId = FormatId(offenderNoteDetails.Pin),
                            NoteId = FormatId(Convert.ToString(offenderNoteDetails.Id)),
                            NoteText = offenderNoteDetails.Text,
                            NoteDatetime = offenderNoteDetails.Date.ToString(),
                            NoteType = offenderNoteDetails.NoteType
                        };

                        if (clientService.GetClientDetails(note.ClientId) != null)
                        {
                            if (noteService.GetNoteDetails(note.ClientId, note.NoteId) == null)
                            {
                                if (noteService.AddNewNoteDetails(note))
                                {
                                    taskExecutionStatus.DestAddRecordCount++;

                                    logger.LogDebug(new LogRequest
                                    {
                                        OperationName = "Processor",
                                        MethodName = "ProcessNotes",
                                        Message = "New Client Note details added successfully.",
                                        SourceData = JsonConvert.SerializeObject(offenderNoteDetails),
                                        DestData = JsonConvert.SerializeObject(note)
                                    });
                                }
                            }
                            else
                            {
                                if (noteService.UpdateNoteDetails(note))
                                {
                                    taskExecutionStatus.DestUpdateRecordCount++;

                                    logger.LogDebug(new LogRequest
                                    {
                                        OperationName = "Processor",
                                        MethodName = "ProcessNotes",
                                        Message = "Existing Client Note details updated successfully.",
                                        SourceData = JsonConvert.SerializeObject(offenderNoteDetails),
                                        DestData = JsonConvert.SerializeObject(note)
                                    });
                                }
                            }
                        }
                        else
                        {
                            taskExecutionStatus.SourceReceivedRecordCount--;
                        }
                    }
                    catch (CmiException ce)
                    {
                        taskExecutionStatus.DestFailureRecordCount++;

                        logger.LogWarning(new LogRequest
                        {
                            OperationName = "Processor",
                            MethodName = "ProcessNotes",
                            Message = "Error occurred in API while processing a Client Note.",
                            Exception = ce,
                            SourceData = JsonConvert.SerializeObject(offenderNoteDetails),
                            DestData = JsonConvert.SerializeObject(note)
                        });
                    }
                    catch (Exception ex)
                    {
                        taskExecutionStatus.DestFailureRecordCount++;

                        logger.LogError(new LogRequest
                        {
                            OperationName = "Processor",
                            MethodName = "ProcessNotes",
                            Message = "Error occurred while processing a Client Note.",
                            Exception = ex,
                            SourceData = JsonConvert.SerializeObject(offenderNoteDetails),
                            DestData = JsonConvert.SerializeObject(note)
                        });
                    }
                }

                taskExecutionStatus.IsSuccessful = taskExecutionStatus.DestFailureRecordCount == 0;
            }
            catch (Exception ex)
            {
                taskExecutionStatus.IsSuccessful = false;

                logger.LogError(new LogRequest
                {
                    OperationName = "Processor",
                    MethodName = "ProcessNotes",
                    Message = "Error occurred while processing Notes.",
                    Exception = ex,
                    SourceData = JsonConvert.SerializeObject(allOffenderNoteDetails)
                });
            }

            logger.LogInfo(new LogRequest
            {
                OperationName = "Processor",
                MethodName = "ProcessNotes",
                Message = "Notes processing completed.",
                CustomParams = JsonConvert.SerializeObject(taskExecutionStatus)
            });

            return taskExecutionStatus;
        }
        #endregion

        #region Private Helper Methods
        private void LoadLookupData()
        {
            //load AddressTypes lookup data
            try
            {
                if (lookupService.AddressTypes != null)
                {
                    logger.LogDebug(new LogRequest
                    {
                        OperationName = "Processor",
                        MethodName = "LoadLookupData",
                        Message = "Successfully retrieved AddressTypes from lookup",
                        CustomParams = JsonConvert.SerializeObject(lookupService.AddressTypes)
                    });
                }
            }
            catch(Exception ex)
            {
                logger.LogError(new LogRequest
                {
                    OperationName = "Processor",
                    MethodName = "LoadLookupData",
                    Message = "Error occurred while loading AddressTypes lookup data",
                    Exception = ex
                });
            }

            //load CaseLoads lookup data
            try
            {
                if (lookupService.CaseLoads != null)
                {
                    logger.LogDebug(new LogRequest
                    {
                        OperationName = "Processor",
                        MethodName = "LoadLookupData",
                        Message = "Successfully retrieved Caseloads from lookup",
                        CustomParams = JsonConvert.SerializeObject(lookupService.CaseLoads)
                    });
                }
            }
            catch (Exception ex)
            {
                logger.LogError(new LogRequest
                {
                    OperationName = "Processor",
                    MethodName = "LoadLookupData",
                    Message = "Error occurred while loading CaseLoads lookup data",
                    Exception = ex
                });
            }

            //load ClientTypes lookup data
            try
            {
                if (lookupService.ClientTypes != null)
                {
                    logger.LogDebug(new LogRequest
                    {
                        OperationName = "Processor",
                        MethodName = "LoadLookupData",
                        Message = "Successfully retrieved ClientTypes from lookup",
                        CustomParams = JsonConvert.SerializeObject(lookupService.ClientTypes)
                    });
                }
            }
            catch (Exception ex)
            {
                logger.LogError(new LogRequest
                {
                    OperationName = "Processor",
                    MethodName = "LoadLookupData",
                    Message = "Error occurred while loading ClientTypes lookup data",
                    Exception = ex
                });
            }

            //load ContactTypes lookup data
            try
            {
                if (lookupService.ContactTypes != null)
                {
                    logger.LogDebug(new LogRequest
                    {
                        OperationName = "Processor",
                        MethodName = "LoadLookupData",
                        Message = "Successfully retrieved ContactTypes from lookup",
                        CustomParams = JsonConvert.SerializeObject(lookupService.ContactTypes)
                    });
                }
            }
            catch (Exception ex)
            {
                logger.LogError(new LogRequest
                {
                    OperationName = "Processor",
                    MethodName = "LoadLookupData",
                    Message = "Error occurred while loading ContactTypes lookup data",
                    Exception = ex
                });
            }

            //load Ethnicities lookup data
            try
            {
                if (lookupService.Ethnicities != null)
                {
                    logger.LogDebug(new LogRequest
                    {
                        OperationName = "Processor",
                        MethodName = "LoadLookupData",
                        Message = "Successfully retrieved Ethnicities from lookup",
                        CustomParams = JsonConvert.SerializeObject(lookupService.Ethnicities)
                    });
                }
            }
            catch (Exception ex)
            {
                logger.LogError(new LogRequest
                {
                    OperationName = "Processor",
                    MethodName = "LoadLookupData",
                    Message = "Error occurred while loading Ethnicities lookup data",
                    Exception = ex
                });
            }

            //load Genders lookup data
            try
            {
                if (lookupService.Genders != null)
                {
                    logger.LogDebug(new LogRequest
                    {
                        OperationName = "Processor",
                        MethodName = "LoadLookupData",
                        Message = "Successfully retrieved Genders from lookup",
                        CustomParams = JsonConvert.SerializeObject(lookupService.Genders)
                    });
                }
            }
            catch (Exception ex)
            {
                logger.LogError(new LogRequest
                {
                    OperationName = "Processor",
                    MethodName = "LoadLookupData",
                    Message = "Error occurred while loading Genders lookup data",
                    Exception = ex
                });
            }

            //load SupervisingOfficers lookup data
            try
            {
                if (lookupService.SupervisingOfficers != null)
                {
                    logger.LogDebug(new LogRequest
                    {
                        OperationName = "Processor",
                        MethodName = "LoadLookupData",
                        Message = "Successfully retrieved SupervisingOfficers from lookup",
                        CustomParams = JsonConvert.SerializeObject(lookupService.SupervisingOfficers)
                    });
                }
            }
            catch (Exception ex)
            {
                logger.LogError(new LogRequest
                {
                    OperationName = "Processor",
                    MethodName = "LoadLookupData",
                    Message = "Error occurred while loading SupervisingOfficers lookup data",
                    Exception = ex
                });
            }

            //load TimeZones lookup data
            try
            {
                if (lookupService.TimeZones != null)
                {
                    logger.LogDebug(new LogRequest
                    {
                        OperationName = "Processor",
                        MethodName = "LoadLookupData",
                        Message = "Successfully retrieved TimeZones from lookup",
                        CustomParams = JsonConvert.SerializeObject(lookupService.TimeZones)
                    });
                }
            }
            catch (Exception ex)
            {
                logger.LogError(new LogRequest
                {
                    OperationName = "Processor",
                    MethodName = "LoadLookupData",
                    Message = "Error occurred while loading TimeZones lookup data",
                    Exception = ex
                });
            }

            //load OffenseCategory lookup data
            try
            {
                if (lookupService.OffenseCategories != null)
                {
                    logger.LogDebug(new LogRequest
                    {
                        OperationName = "Processor",
                        MethodName = "LoadLookupData",
                        Message = "Successfully retrieved OffenseCategories from lookup",
                        CustomParams = JsonConvert.SerializeObject(lookupService.OffenseCategories)
                    });
                }
            }
            catch (Exception ex)
            {
                logger.LogError(new LogRequest
                {
                    OperationName = "Processor",
                    MethodName = "LoadLookupData",
                    Message = "Error occurred while loading OffenseCategories lookup data",
                    Exception = ex
                });
            }
        }

        private void UpdateExecutionStatus(Common.Notification.TaskExecutionStatus taskExecutionStatus)
        {
            processorExecutionStatus.NumTaskProcessed++;
            if (taskExecutionStatus != null)
            {
                if (taskExecutionStatus.IsSuccessful)
                {
                    processorExecutionStatus.NumTaskSucceeded++;
                }
                else
                {
                    processorExecutionStatus.NumTaskFailed++;
                }

                processorExecutionStatus.IsSuccessful = processorExecutionStatus.IsSuccessful & taskExecutionStatus.IsSuccessful;
                taskExecutionStatuses.Add(taskExecutionStatus);
            }
        }

        #region Processor DAL Methods
        private void RetrieveLastExecutionDateTime()
        {
            try
            {
                lastExecutionDateTime = processorProvider.GetLastExecutionDateTime();

                logger.LogInfo(new LogRequest
                {
                    OperationName = "Processor",
                    MethodName = "RetrieveLastExecutionDateTime",
                    Message = "Successfully retrieved Last Execution Date Time",
                    CustomParams = JsonConvert.SerializeObject(lastExecutionDateTime)
                });
            }
            catch (Exception ex)
            {
                logger.LogError(new LogRequest
                {
                    OperationName = "Processor",
                    MethodName = "RetrieveLastExecutionDateTime",
                    Message = "Error occurred while retriving processor last execution status.",
                    Exception = ex
                });
            }
        }

        private void SaveExecutionStatus(DAL.ExecutionStatus executionStatus)
        {
            try
            {
                processorProvider.SaveExecutionStatus(executionStatus);
            }
            catch (Exception ex)
            {
                logger.LogError(new LogRequest
                {
                    OperationName = "Processor",
                    MethodName = "SaveExecutionStatus",
                    Message = "Error occurred while saving processor execution status.",
                    Exception = ex,
                    CustomParams = JsonConvert.SerializeObject(executionStatus)
                });
            }
        }

        private string FormatId(string oldId)
        {
            string newId = string.Empty;
            try
            {
                if (oldId.Length >= CMI.DAL.Dest.Nexus.Constants.ExpectedMinLenghOfId)
                {
                    newId = oldId;
                }
                else
                {
                    string[] zeros = Enumerable.Repeat("0", (CMI.DAL.Dest.Nexus.Constants.ExpectedMinLenghOfId - oldId.Length)).ToArray();

                    newId = string.Format("{0}{1}", string.Join("", zeros), oldId);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(new LogRequest
                {
                    OperationName = "Processor",
                    MethodName = "FormatId",
                    Message = "Error occurred while formatting Id",
                    Exception = ex
                });
            }

            return newId;
        }
        #endregion

        #region Mapping Helper Methods
        private string MapFullAddress(string line1, string line2, string city, string state, string zip)
        {
            string destFullAddress = string.Empty;

            if(!string.IsNullOrEmpty(line1))
            {
                destFullAddress += line1 + ", ";
            }

            if (!string.IsNullOrEmpty(line2))
            {
                destFullAddress += line2 + ", ";
            }

            if (!string.IsNullOrEmpty(city))
            {
                destFullAddress += city + ", ";
            }

            if (!string.IsNullOrEmpty(state))
            {
                destFullAddress += state + ", ";
            }

            if (!string.IsNullOrEmpty(zip))
            {
                destFullAddress += zip + ", ";
            }

            destFullAddress = destFullAddress.Trim(new char[] { ',', ' ' });

            return string.IsNullOrEmpty(destFullAddress) ? destFullAddress : destFullAddress.Replace("/", "-");
        }

        private string MapAddressType(string sourceAddressType)
        {
            string destAddressType = string.Empty;

            switch (sourceAddressType)
            {
                case "Mailing":
                    destAddressType = "Shipping Address";
                    break;
                case "Residential":
                    destAddressType = "Home Address";
                    break;
                case "Work/Business":
                    destAddressType = "Work Address";
                    break;
                default:
                    destAddressType = "Other";
                    break;
            }

            return destAddressType;
        }

        private string MapContactType(string sourceContactType)
        {
            string destContactType = string.Empty;

            switch (sourceContactType)
            {
                case "Fax":
                    destContactType = "Fax";
                    break;
                case "Mobile":
                case "Message":
                    destContactType = "Cell Phone";
                    break;
                case "Residential":
                    destContactType = "Home Phone";
                    break;
                case "Office":
                    destContactType = "Work Phone";
                    break;
                default:
                    destContactType = "Other";
                    break;
            }

            return destContactType;
        }

        private string MapEthnicity(string sourceEthnicity)
        {
            return 
                (lookupService.Ethnicities != null && lookupService.Ethnicities.Any(e => e.Equals(sourceEthnicity, StringComparison.InvariantCultureIgnoreCase))) 
                ? sourceEthnicity 
                : DAL.Constants.EthnicityUnknown;
        }

        private string MapCaseload(string sourceCaseloadName)
        {
            if (lookupService.CaseLoads != null && lookupService.CaseLoads.Any(c => c.Name.Equals(sourceCaseloadName, StringComparison.InvariantCultureIgnoreCase)))
            {
                return lookupService.CaseLoads.FirstOrDefault(c => c.Name.Equals(sourceCaseloadName, StringComparison.InvariantCultureIgnoreCase)).Id.ToString();
            }

            return null;
        }

        private string MapSupervisingOfficer(string sourceFirstName, string sourceLastName, string sourceEmailAddress)
        {
            if (lookupService.SupervisingOfficers != null && lookupService.SupervisingOfficers.Any(s => s.FirstName.Equals(sourceFirstName, StringComparison.InvariantCultureIgnoreCase) && s.LastName.Equals(sourceLastName, StringComparison.InvariantCultureIgnoreCase) && s.Email.Equals(sourceEmailAddress, StringComparison.InvariantCultureIgnoreCase)))
            {
                return lookupService.SupervisingOfficers.FirstOrDefault(s => s.FirstName.Equals(sourceFirstName, StringComparison.InvariantCultureIgnoreCase) && s.LastName.Equals(sourceLastName, StringComparison.InvariantCultureIgnoreCase) && s.Email.Equals(sourceEmailAddress, StringComparison.InvariantCultureIgnoreCase)).Email;
            }

            return null;
        }

        private string MapOffenseCategory(string sourceOffenseCategory)
        {
            if (lookupService.OffenseCategories != null && lookupService.OffenseCategories.Any(c => c.Equals(sourceOffenseCategory, StringComparison.InvariantCultureIgnoreCase)))
            {
                return sourceOffenseCategory;
            }

            return null;
        }
        #endregion
        #endregion
    }
}
