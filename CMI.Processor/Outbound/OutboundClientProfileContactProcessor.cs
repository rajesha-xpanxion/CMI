﻿using CMI.Automon.Interface;
using CMI.Automon.Model;
using CMI.Common.Logging;
using CMI.Common.Notification;
using CMI.MessageRetriever.Model;
using CMI.Nexus.Model;
using CMI.Processor.DAL;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CMI.Processor
{
    public class OutboundClientProfileContactProcessor : OutboundBaseProcessor
    {
        private readonly IOffenderEmailService offenderEmailService;
        private readonly IOffenderPhoneService offenderPhoneService;

        public OutboundClientProfileContactProcessor(
            IServiceProvider serviceProvider,
            IConfiguration configuration,
            IOffenderEmailService offenderEmailService,
            IOffenderPhoneService offenderPhoneService
        )
            : base(serviceProvider, configuration)
        {
            this.offenderEmailService = offenderEmailService;
            this.offenderPhoneService = offenderPhoneService;
        }

        public override TaskExecutionStatus Execute(IEnumerable<OutboundMessageDetails> messages, DateTime messagesReceivedOn)
        {
            Logger.LogInfo(new LogRequest
            {
                OperationName = this.GetType().Name,
                MethodName = "Execute",
                Message = "Client Profile - Contact Details activity processing initiated."
            });

            TaskExecutionStatus taskExecutionStatus = new TaskExecutionStatus
            {
                ProcessorType = Common.Notification.ProcessorType.Outbound,
                TaskName = "Client Profile - Contact Details",
                IsSuccessful = true,
                NexusReceivedMessageCount = messages.Count()
            };

            try
            {
                foreach (OutboundMessageDetails message in messages)
                {
                    Offender offenderContactDetails = null;
                    OffenderPhone offenderPhoneDetails = null;
                    OffenderEmail offenderEmailDetails = null;
                    message.IsProcessed = true;
                    try
                    {
                        offenderContactDetails = (Offender)ConvertResponseToObject<ClientProfileContactDetailsActivityResponse>(
                            message.ClientIntegrationId,
                            RetrieveActivityDetails<ClientProfileContactDetailsActivityResponse>(message.Details),
                            message.ActionUpdatedBy
                        );

                        if (offenderContactDetails.GetType() == typeof(OffenderEmail))
                        {
                            offenderEmailDetails = (OffenderEmail)offenderContactDetails;
                            offenderEmailService.SaveOffenderEmailDetails(ProcessorConfig.CmiDbConnString, offenderEmailDetails);
                        }
                        else
                        {
                            offenderPhoneDetails = (OffenderPhone)offenderContactDetails; 

                            offenderPhoneService.SaveOffenderPhoneDetails(ProcessorConfig.CmiDbConnString, offenderPhoneDetails);
                        }

                        taskExecutionStatus.AutomonAddMessageCount++;
                        message.IsSuccessful = true;

                        Logger.LogDebug(new LogRequest
                        {
                            OperationName = this.GetType().Name,
                            MethodName = "Execute",
                            Message = "New Offender - Contact Details added successfully.",
                            AutomonData = JsonConvert.SerializeObject(offenderContactDetails),
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
                            Message = "Error occurred while processing a Client Profile - Contact Details activity.",
                            Exception = ce,
                            AutomonData = JsonConvert.SerializeObject(offenderContactDetails),
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
                            Message = "Critical error occurred while processing a Client Profile - Contact Details activity.",
                            Exception = ex,
                            AutomonData = JsonConvert.SerializeObject(offenderContactDetails),
                            NexusData = JsonConvert.SerializeObject(message)
                        });
                    }
                }

                taskExecutionStatus.IsSuccessful = taskExecutionStatus.AutomonFailureMessageCount == 0;
            }
            catch (Exception ex)
            {
                taskExecutionStatus.IsSuccessful = false;
                messages.ToList().ForEach(m => {
                    m.IsProcessed = true;
                    m.IsSuccessful = false;
                    m.ErrorDetails = ex.ToString();
                });

                Logger.LogError(new LogRequest
                {
                    OperationName = this.GetType().Name,
                    MethodName = "Execute",
                    Message = "Critical error occurred while processing Client Profile - Contact Details activities.",
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
                Message = "Client Profile - Contact Details activity processing completed.",
                CustomParams = JsonConvert.SerializeObject(taskExecutionStatus)
            });

            return taskExecutionStatus;
        }
    }
}