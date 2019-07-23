using CMI.Automon.Interface;
using CMI.Automon.Model;
using CMI.Common.Logging;
using CMI.Common.Notification;
using CMI.MessageRetriever.Model;
using CMI.Nexus.Interface;
using CMI.Nexus.Model;
using CMI.Nexus.Service;
using CMI.Processor.DAL;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CMI.Processor
{
    public class OutboundNewClientProfileProcessor : OutboundBaseProcessor
    {
        private readonly IOffenderService offenderService;
        private readonly IOffenderPersonalDetailsService offenderPersonalDetailsService;
        private readonly IOffenderEmailService offenderEmailService;
        private readonly IOffenderAddressService offenderAddressService;
        private readonly IOffenderPhoneService offenderPhoneService;
        private readonly ICommonService commonService;
        private readonly IAddressService addressService;
        private readonly IContactService contactService;

        public OutboundNewClientProfileProcessor(
            IServiceProvider serviceProvider,
            IConfiguration configuration,
            IOffenderService offenderService,
            IOffenderPersonalDetailsService offenderPersonalDetailsService,
            IOffenderEmailService offenderEmailService,
            IOffenderAddressService offenderAddressService,
            IOffenderPhoneService offenderPhoneService,
            ICommonService commonService,
            IAddressService addressService,
            IContactService contactService
        )
            : base(serviceProvider, configuration)
        {
            this.offenderService = offenderService;
            this.offenderPersonalDetailsService = offenderPersonalDetailsService;
            this.offenderEmailService = offenderEmailService;
            this.offenderAddressService = offenderAddressService;
            this.offenderPhoneService = offenderPhoneService;

            this.commonService = commonService;
            this.addressService = addressService;
            this.contactService = contactService;
        }

        public override TaskExecutionStatus Execute(IEnumerable<OutboundMessageDetails> messages, DateTime messagesReceivedOn)
        {
            Logger.LogInfo(new LogRequest
            {
                OperationName = this.GetType().Name,
                MethodName = "Execute",
                Message = "New Client Profile activity processing initiated."
            });

            TaskExecutionStatus taskExecutionStatus = new TaskExecutionStatus
            {
                ProcessorType = Common.Notification.ProcessorType.Outbound,
                TaskName = "New Client Profile",
                IsSuccessful = true,
                NexusReceivedMessageCount = messages.Count()
            };

            try
            {
                foreach (OutboundMessageDetails message in messages)
                {
                    OffenderDetails offenderDetails = null;
                    message.IsProcessed = true;
                    try
                    {
                        offenderDetails = (OffenderDetails)ConvertResponseToObject<NewClientProfileActivityResponse>(
                            message.ClientIntegrationId,
                            message.ActivityIdentifier,
                            message.AutomonIdentifier,
                            RetrieveActivityDetails<NewClientProfileActivityResponse>(message.Details),
                            message.ActionUpdatedBy
                        );

                        //add new offender with basic details
                        offenderDetails.Pin = offenderService.SaveOffenderDetails(ProcessorConfig.CmiDbConnString, offenderDetails);

                        //check if saving details to Automon was successsful
                        if (string.IsNullOrEmpty(offenderDetails.Pin))
                        {
                            throw new CmiException("New offender details could not be saved in Automon.");
                        }

                        //save new identifier in message details
                        message.AutomonIdentifier = offenderDetails.Pin;

                        //derive current integration id & new integration id & flag whether integration id has been changed or not
                        string currentIntegrationId = message.ActivityIdentifier, newIntegrationId = offenderDetails.Pin;
                        bool isIntegrationIdUpdated = !currentIntegrationId.Equals(newIntegrationId, StringComparison.InvariantCultureIgnoreCase);

                        //update integration identifier in Nexus if it is updated
                        if (isIntegrationIdUpdated)
                        {
                            //commonService.UpdateId(currentIntegrationId, new ReplaceIntegrationIdDetails { ElementType = DataElementType.Client, CurrentIntegrationId = currentIntegrationId, NewIntegrationId = newIntegrationId });
                        }

                        //check if personal details processing enabled
                        if (
                            ProcessorConfig.OutboundProcessorConfig.ActivitySubTypesToProcess != null
                            && ProcessorConfig.OutboundProcessorConfig.ActivitySubTypesToProcess.Any(a => a.Equals(OutboundProcessorClientProfileActivitySubType.PersonalDetails, StringComparison.InvariantCultureIgnoreCase))
                        )
                        {
                            //save personal details of newly created offender
                            offenderPersonalDetailsService.SaveOffenderPersonalDetails(ProcessorConfig.CmiDbConnString, offenderDetails);
                        }

                        //check if address details processing enabled
                        if (
                            ProcessorConfig.OutboundProcessorConfig.ActivitySubTypesToProcess != null
                            && ProcessorConfig.OutboundProcessorConfig.ActivitySubTypesToProcess.Any(a => a.Equals(OutboundProcessorClientProfileActivitySubType.AddressDetails, StringComparison.InvariantCultureIgnoreCase))
                        )
                        {
                            //retrieve all address details (if any) for newly created client profile from Nexus
                            var allAddressDetails = addressService.GetAllAddressDetails(offenderDetails.Pin);

                            //check if we got any address details from API
                            if (allAddressDetails != null && allAddressDetails.Any())
                            {
                                //iterate through all address details & push it to Automon and then update integration id in Nexus accordingly
                                foreach (Address addressDetails in allAddressDetails)
                                {
                                    //save address details of newly created client profile in Automon & get automon specific id
                                    int automonAddressId = offenderAddressService.SaveOffenderAddressDetails(ProcessorConfig.CmiDbConnString, new OffenderAddress
                                    {
                                        Pin = addressDetails.ClientId,
                                        Id = 0,
                                        Line1 = addressDetails.FullAddress,
                                        AddressType = addressDetails.AddressType != null
                                            ? addressDetails.AddressType.Equals("Home", StringComparison.InvariantCultureIgnoreCase) || addressDetails.AddressType.Equals("Home Address", StringComparison.InvariantCultureIgnoreCase)
                                                ? "Residential"
                                                : "Mailing"
                                            : "Unknown",
                                        UpdatedBy = offenderDetails.UpdatedBy
                                    });

                                    //check if saving details to Automon was successsful
                                    if (automonAddressId == 0)
                                    {
                                        throw new CmiException("New offender - Address details could not be saved in Automon.");
                                    }

                                    //update integration id in Nexus
                                    //commonService.UpdateId(addressDetails.ClientId, new ReplaceIntegrationIdDetails { ElementType = DataElementType.Address, CurrentIntegrationId = addressDetails.AddressId, NewIntegrationId = string.Format("{0}-{1}", addressDetails.ClientId, automonAddressId.ToString()) });
                                }
                            }
                        }

                        //retrieve all contact details (if any) for newly created client profile from Nexus
                        var allContactDetails = contactService.GetAllContactDetails(offenderDetails.Pin);

                        //check if we got any contact details from API
                        if (allContactDetails != null && allContactDetails.Any())
                        {
                            //check if email details processing enabled
                            if (
                                ProcessorConfig.OutboundProcessorConfig.ActivitySubTypesToProcess != null
                                && ProcessorConfig.OutboundProcessorConfig.ActivitySubTypesToProcess.Any(a => a.Equals(OutboundProcessorClientProfileActivitySubType.EmailDetails, StringComparison.InvariantCultureIgnoreCase))
                            )
                            {
                                //iterate through all contact details & push it to Automon and then update integration id in Nexus accordingly
                                foreach (Contact contactDetails in allContactDetails.Where(x => x.ContactType.Equals("E-mail", StringComparison.InvariantCultureIgnoreCase)))
                                {
                                    //save email details of newly created client profile in Automon & get automon specific id
                                    int automonEmailId = offenderEmailService.SaveOffenderEmailDetails(ProcessorConfig.CmiDbConnString, new OffenderEmail
                                    {
                                        Pin = contactDetails.ClientId,
                                        Id = 0,
                                        EmailAddress = contactDetails.ContactValue,
                                        UpdatedBy = offenderDetails.UpdatedBy
                                    });

                                    //check if saving details to Automon was successsful
                                    if (automonEmailId == 0)
                                    {
                                        throw new CmiException("New offender - Email details could not be saved in Automon.");
                                    }

                                    //update integration id in Nexus
                                    //commonService.UpdateId(contactDetails.ClientId, new ReplaceIntegrationIdDetails { ElementType = DataElementType.Address, CurrentIntegrationId = contactDetails.ContactId, NewIntegrationId = string.Format("{0}-{1}", contactDetails.ClientId, automonEmailId.ToString()) });
                                }
                            }

                            //check if contact details processing enabled
                            if (
                                ProcessorConfig.OutboundProcessorConfig.ActivitySubTypesToProcess != null
                                && ProcessorConfig.OutboundProcessorConfig.ActivitySubTypesToProcess.Any(a => a.Equals(OutboundProcessorClientProfileActivitySubType.ContactDetails, StringComparison.InvariantCultureIgnoreCase))
                            )
                            {
                                //iterate through all contact details & push it to Automon and then update integration id in Nexus accordingly
                                foreach (Contact contactDetails in allContactDetails.Where(x => !x.ContactType.Equals("E-mail", StringComparison.InvariantCultureIgnoreCase)))
                                {
                                    //map contact type from Nexus to Automon phone number type
                                    string autmonPhoneNumberType = string.Empty;
                                    switch (contactDetails.ContactType)
                                    {
                                        case "Cell Phone":
                                        case "Emergency Phone":
                                            autmonPhoneNumberType = "Mobile";
                                            break;
                                        case "Home Phone":
                                            autmonPhoneNumberType = "Residential";
                                            break;
                                        case "Fax":
                                            autmonPhoneNumberType = "Fax";
                                            break;
                                        case "Business Phone":
                                        case "Work Phone":
                                            autmonPhoneNumberType = "Office";
                                            break;
                                        default:
                                            autmonPhoneNumberType = "Message";
                                            break;
                                    }

                                    //save phone details of newly created client profile in Automon & get automon specific id
                                    int automonPhoneNumberId = offenderPhoneService.SaveOffenderPhoneDetails(ProcessorConfig.CmiDbConnString, new OffenderPhone
                                    {
                                        Pin = contactDetails.ClientId,
                                        Id = 0,
                                        Phone =
                                            contactDetails.ContactValue != null
                                            ?
                                                contactDetails.ContactValue.Replace(" ", string.Empty)
                                            :
                                                string.Empty,
                                        PhoneNumberType = autmonPhoneNumberType,

                                        UpdatedBy = offenderDetails.UpdatedBy,
                                    });

                                    //check if saving details to Automon was successsful
                                    if (automonPhoneNumberId == 0)
                                    {
                                        throw new CmiException("New offender - Phone Number details could not be saved in Automon.");
                                    }

                                    //update integration id in Nexus
                                    //commonService.UpdateId(contactDetails.ClientId, new ReplaceIntegrationIdDetails { ElementType = DataElementType.Contact, CurrentIntegrationId = contactDetails.ContactId, NewIntegrationId = string.Format("{0}-{1}", contactDetails.ClientId, automonPhoneNumberId.ToString()) });
                                }
                            }
                        }
                        
                        //update counter
                        taskExecutionStatus.AutomonAddMessageCount++;
                        //mark message processing successful
                        message.IsSuccessful = true;

                        Logger.LogDebug(new LogRequest
                        {
                            OperationName = this.GetType().Name,
                            MethodName = "Execute",
                            Message = "New Offender - Details added successfully.",
                            AutomonData = JsonConvert.SerializeObject(offenderDetails),
                            NexusData = JsonConvert.SerializeObject(message)
                        });
                    }
                    catch (CmiException ce)
                    {
                        taskExecutionStatus.AutomonFailureMessageCount++;
                        message.IsSuccessful = false;
                        message.ErrorDetails = ce.ToString();

                        Logger.LogWarning(new LogRequest
                        {
                            OperationName = this.GetType().Name,
                            MethodName = "Execute",
                            Message = "Error occurred while processing a New Client Profile activity.",
                            Exception = ce,
                            AutomonData = JsonConvert.SerializeObject(offenderDetails),
                            NexusData = JsonConvert.SerializeObject(message)
                        });
                    }
                    catch (Exception ex)
                    {
                        taskExecutionStatus.AutomonFailureMessageCount++;
                        message.IsSuccessful = false;
                        message.ErrorDetails = ex.ToString();

                        Logger.LogError(new LogRequest
                        {
                            OperationName = this.GetType().Name,
                            MethodName = "Execute",
                            Message = "Critical error occurred while processing a New Client Profile activity.",
                            Exception = ex,
                            AutomonData = JsonConvert.SerializeObject(offenderDetails),
                            NexusData = JsonConvert.SerializeObject(message)
                        });
                    }
                }

                taskExecutionStatus.IsSuccessful = taskExecutionStatus.AutomonFailureMessageCount == 0;
            }
            catch (Exception ex)
            {
                taskExecutionStatus.IsSuccessful = false;
                messages.ToList().ForEach(m =>
                {
                    m.IsProcessed = true;
                    m.IsSuccessful = false;
                    m.ErrorDetails = ex.ToString();
                });

                Logger.LogError(new LogRequest
                {
                    OperationName = this.GetType().Name,
                    MethodName = "Execute",
                    Message = "Critical error occurred while processing New Client Profile activities.",
                    Exception = ex,
                    AutomonData = JsonConvert.SerializeObject(messages)
                });
            }

            //update message wise processing status
            if (messages != null && messages.Any())
            {
                ProcessorProvider.SaveOutboundMessagesToDatabase(messages);
            }

            Logger.LogInfo(new LogRequest
            {
                OperationName = this.GetType().Name,
                MethodName = "Execute",
                Message = "New Client Profile activity processing completed.",
                CustomParams = JsonConvert.SerializeObject(taskExecutionStatus)
            });

            return taskExecutionStatus;
        }
    }
}
