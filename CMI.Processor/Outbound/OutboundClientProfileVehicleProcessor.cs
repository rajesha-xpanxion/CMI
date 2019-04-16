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
    public class OutboundClientProfileVehicleProcessor : OutboundBaseProcessor
    {
        private readonly IOffenderVehicleService offenderVehicleService;

        public OutboundClientProfileVehicleProcessor(
            IServiceProvider serviceProvider,
            IConfiguration configuration,
            IOffenderVehicleService offenderVehicleService
        )
            : base(serviceProvider, configuration)
        {
            this.offenderVehicleService = offenderVehicleService;
        }

        public override TaskExecutionStatus Execute(IEnumerable<OutboundMessageDetails> messages, DateTime messagesReceivedOn)
        {
            Logger.LogInfo(new LogRequest
            {
                OperationName = this.GetType().Name,
                MethodName = "Execute",
                Message = "Client Profile - Vehicle Details activity processing initiated."
            });

            TaskExecutionStatus taskExecutionStatus = new TaskExecutionStatus
            {
                ProcessorType = Common.Notification.ProcessorType.Outbound,
                TaskName = "Client Profile - Vehicle Details",
                IsSuccessful = true,
                NexusReceivedMessageCount = messages.Count()
            };

            try
            {
                foreach (OutboundMessageDetails message in messages)
                {
                    OffenderVehicle offenderVehicleDetails = null;
                    try
                    {
                        offenderVehicleDetails = (OffenderVehicle)ConvertResponseToObject<ClientProfileVehicleDetailsActivityResponse>(
                            message.ClientIntegrationId,
                            RetrieveActivityDetails<ClientProfileVehicleDetailsActivityResponse>(message.Details),
                            message.ActionUpdatedBy
                        );

                        if (
                            message.ActionReasonName.Equals(OutboundProcessorActionReason.Created, StringComparison.InvariantCultureIgnoreCase)
                            || message.ActionReasonName.Equals(OutboundProcessorActionReason.Updated, StringComparison.InvariantCultureIgnoreCase)
                        )
                        {
                            offenderVehicleService.SaveOffenderVehicleDetails(ProcessorConfig.CmiDbConnString, offenderVehicleDetails);
                            taskExecutionStatus.AutomonAddMessageCount++;

                            Logger.LogDebug(new LogRequest
                            {
                                OperationName = this.GetType().Name,
                                MethodName = "Execute",
                                Message = "New Offender - Vehicle Details added successfully.",
                                AutomonData = JsonConvert.SerializeObject(offenderVehicleDetails),
                                NexusData = JsonConvert.SerializeObject(message)
                            });
                        }
                        else if(message.ActionReasonName.Equals(OutboundProcessorActionReason.Removed, StringComparison.InvariantCultureIgnoreCase))
                        {
                            offenderVehicleService.DeleteOffenderVehicleDetails(ProcessorConfig.CmiDbConnString, offenderVehicleDetails);
                            taskExecutionStatus.AutomonDeleteMessageCount++;

                            Logger.LogDebug(new LogRequest
                            {
                                OperationName = this.GetType().Name,
                                MethodName = "Execute",
                                Message = "Offender - Vehicle Details removed successfully.",
                                AutomonData = JsonConvert.SerializeObject(offenderVehicleDetails),
                                NexusData = JsonConvert.SerializeObject(message)
                            });
                        }
                        else
                        {
                            throw new CmiException("Invalid action reason found.");
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
                            Message = "Error occurred while processing a Client Profile - Vehicle Details activity.",
                            Exception = ce,
                            AutomonData = JsonConvert.SerializeObject(offenderVehicleDetails),
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
                            Message = "Critical error occurred while processing a Client Profile - Vehicle Details activity.",
                            Exception = ex,
                            AutomonData = JsonConvert.SerializeObject(offenderVehicleDetails),
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
                    m.IsSuccessful = false;
                    m.ErrorDetails = ex.ToString();
                });

                Logger.LogError(new LogRequest
                {
                    OperationName = this.GetType().Name,
                    MethodName = "Execute",
                    Message = "Critical error occurred while processing Client Profile - Vehicle Details activities.",
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
                Message = "Client Profile - Vehicle Details activity processing completed.",
                CustomParams = JsonConvert.SerializeObject(taskExecutionStatus)
            });

            return taskExecutionStatus;
        }
    }
}
