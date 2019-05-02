using CMI.Automon.Interface;
using CMI.Automon.Model;
using CMI.Common.Logging;
using CMI.Common.Notification;
using CMI.MessageRetriever.Model;
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
    public class OutboundNewClientProfileProcessor : OutboundBaseProcessor
    {
        private readonly IOffenderService offenderService;
        private readonly IOffenderPersonalDetailsService offenderPersonalDetailsService;
        private readonly IOffenderEmailService offenderEmailService;
        private readonly IOffenderAddressService offenderAddressService;
        private readonly IOffenderPhoneService offenderPhoneService;
        private readonly IClientService clientService;

        public OutboundNewClientProfileProcessor(
            IServiceProvider serviceProvider,
            IConfiguration configuration,
            IOffenderService offenderService,
            IOffenderPersonalDetailsService offenderPersonalDetailsService,
            IOffenderEmailService offenderEmailService,
            IOffenderAddressService offenderAddressService,
            IOffenderPhoneService offenderPhoneService,
            IClientService clientService
        )
            : base(serviceProvider, configuration)
        {
            this.offenderService = offenderService;
            this.offenderPersonalDetailsService = offenderPersonalDetailsService;
            this.offenderEmailService = offenderEmailService;
            this.offenderAddressService = offenderAddressService;
            this.offenderPhoneService = offenderPhoneService;

            this.clientService = clientService;
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
                            RetrieveActivityDetails<NewClientProfileActivityResponse>(message.Details),
                            message.ActionUpdatedBy
                        );

                        //add new offender with basic details
                        offenderDetails.Pin = offenderService.SaveOffenderDetails(ProcessorConfig.CmiDbConnString, offenderDetails);

                        //update back client id in Nexus
                        clientService.UpdateClientId(message.ClientIntegrationId, offenderDetails.Pin);

                        //save personal details of newly created offender
                        offenderPersonalDetailsService.SaveOffenderPersonalDetails(ProcessorConfig.CmiDbConnString, offenderDetails);

                        //save email details of newly created offender
                        offenderEmailService.SaveOffenderEmailDetails(ProcessorConfig.CmiDbConnString, new OffenderEmail
                        {
                            Pin = offenderDetails.Pin,
                            UpdatedBy = offenderDetails.UpdatedBy,
                            EmailAddress = offenderDetails.EmailAddress
                        });

                        //save address details of newly created offender
                        offenderAddressService.SaveOffenderAddressDetails(ProcessorConfig.CmiDbConnString, new OffenderAddress
                        {
                            Pin = offenderDetails.Pin,
                            UpdatedBy = offenderDetails.UpdatedBy,
                            Line1 = offenderDetails.Line1,
                            Line2 = offenderDetails.Line2,
                            AddressType = offenderDetails.AddressType
                        });

                        //save phone details of newly created offender
                        offenderPhoneService.SaveOffenderPhoneDetails(ProcessorConfig.CmiDbConnString, new OffenderPhone
                        {
                            Pin = offenderDetails.Pin,
                            UpdatedBy = offenderDetails.UpdatedBy,
                            Phone = offenderDetails.Phone,
                            PhoneNumberType = offenderDetails.PhoneNumberType
                        });

                        taskExecutionStatus.AutomonAddMessageCount++;
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
                ProcessorProvider.SaveOutboundMessages(messages, messagesReceivedOn);
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
