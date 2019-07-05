using CMI.Automon.Interface;
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
    public class OutboundClientProfileEmploymentProcessor : OutboundBaseProcessor
    {
        private readonly IOffenderEmploymentService offenderEmploymentService;

        public OutboundClientProfileEmploymentProcessor(
            IServiceProvider serviceProvider,
            IConfiguration configuration,
            IOffenderEmploymentService offenderEmploymentService
        )
            : base(serviceProvider, configuration)
        {
            this.offenderEmploymentService = offenderEmploymentService;
        }

        public override TaskExecutionStatus Execute(IEnumerable<OutboundMessageDetails> messages, DateTime messagesReceivedOn)
        {
            Logger.LogInfo(new LogRequest
            {
                OperationName = this.GetType().Name,
                MethodName = "Execute",
                Message = "Client Profile - Employment Details activity processing initiated."
            });

            TaskExecutionStatus taskExecutionStatus = new TaskExecutionStatus
            {
                ProcessorType = Common.Notification.ProcessorType.Outbound,
                TaskName = "Client Profile - Employment Details",
                IsSuccessful = true,
                NexusReceivedMessageCount = messages.Count()
            };

            try
            {
                foreach (OutboundMessageDetails message in messages)
                {
                    OffenderEmployment offenderEmploymentDetails = null;
                    message.IsProcessed = true;
                    try
                    {
                        offenderEmploymentDetails = (OffenderEmployment)ConvertResponseToObject<ClientProfileEmploymentDetailsActivityResponse>(
                            message.ClientIntegrationId,
                            message.ActivityIdentifier,
                            RetrieveActivityDetails<ClientProfileEmploymentDetailsActivityResponse>(message.Details),
                            message.ActionUpdatedBy
                        );

                        if (
                            message.ActionReasonName.Equals(OutboundProcessorActionReason.Created, StringComparison.InvariantCultureIgnoreCase)
                            || message.ActionReasonName.Equals(OutboundProcessorActionReason.Updated, StringComparison.InvariantCultureIgnoreCase)
                        )
                        {

                            offenderEmploymentService.SaveOffenderEmploymentDetails(ProcessorConfig.CmiDbConnString, offenderEmploymentDetails);
                            taskExecutionStatus.AutomonAddMessageCount++;
                            Logger.LogDebug(new LogRequest
                            {
                                OperationName = this.GetType().Name,
                                MethodName = "Execute",
                                Message = "New Offender - Employment Details added successfully.",
                                AutomonData = JsonConvert.SerializeObject(offenderEmploymentDetails),
                                NexusData = JsonConvert.SerializeObject(message)
                            });
                        }
                        else if (message.ActionReasonName.Equals(OutboundProcessorActionReason.Removed, StringComparison.InvariantCultureIgnoreCase))
                        {
                            offenderEmploymentService.DeleteOffenderEmploymentDetails(ProcessorConfig.CmiDbConnString, offenderEmploymentDetails);
                            taskExecutionStatus.AutomonDeleteMessageCount++;
                            Logger.LogDebug(new LogRequest
                            {
                                OperationName = this.GetType().Name,
                                MethodName = "Execute",
                                Message = "Offender - Employment Details removed successfully.",
                                AutomonData = JsonConvert.SerializeObject(offenderEmploymentDetails),
                                NexusData = JsonConvert.SerializeObject(message)
                            });
                        }
                        message.IsSuccessful = true;
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
                            Message = "Error occurred while processing a Client Profile - Employment Details activity.",
                            Exception = ce,
                            AutomonData = JsonConvert.SerializeObject(offenderEmploymentDetails),
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
                            Message = "Critical error occurred while processing a Client Profile - Employment Details activity.",
                            Exception = ex,
                            AutomonData = JsonConvert.SerializeObject(offenderEmploymentDetails),
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
                    Message = "Critical error occurred while processing Client Profile - Employment Details activities.",
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
                Message = "Client Profile - Employment Details activity processing completed.",
                CustomParams = JsonConvert.SerializeObject(taskExecutionStatus)
            });

            return taskExecutionStatus;
        }
    }
}
