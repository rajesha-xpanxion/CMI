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
    public class OutboundClientProfileOnDemandSanctionProcessor : OutboundBaseProcessor
    {
        private readonly IOffenderOnDemandSanctionService offenderOnDemandSanctionService;
        private readonly ICommonService commonService;

        public OutboundClientProfileOnDemandSanctionProcessor(
            IServiceProvider serviceProvider,
            IConfiguration configuration,
            IOffenderOnDemandSanctionService offenderOnDemandSanctionService,
            ICommonService commonService
        )
            : base(serviceProvider, configuration)
        {
            this.offenderOnDemandSanctionService = offenderOnDemandSanctionService;
            this.commonService = commonService;
        }

        public override TaskExecutionStatus Execute(IEnumerable<OutboundMessageDetails> messages, DateTime messagesReceivedOn)
        {
            Logger.LogInfo(new LogRequest
            {
                OperationName = this.GetType().Name,
                MethodName = "Execute",
                Message = "OnDemand Sanction activity processing initiated."
            });

            TaskExecutionStatus taskExecutionStatus = new TaskExecutionStatus
            {
                ProcessorType = Common.Notification.ProcessorType.Outbound,
                TaskName = "OnDemand Sanction",
                IsSuccessful = true,
                NexusReceivedMessageCount = messages.Count()
            };

            try
            {
                if (messages.Any())
                {
                    Logger.LogDebug(new LogRequest
                    {
                        OperationName = this.GetType().Name,
                        MethodName = "Execute",
                        Message = string.Format("{0} OnDemand Sanction messages received for processing.", messages.Count()),
                        CustomParams = JsonConvert.SerializeObject(messages)
                    });
                }

                foreach (OutboundMessageDetails message in messages)
                {
                    OffenderOnDemandSanction offenderOnDemandSanctionDetails = null;
                    message.IsProcessed = true;
                    try
                    {
                        //transform message details into required Automon object
                        offenderOnDemandSanctionDetails = (OffenderOnDemandSanction)ConvertResponseToObject<ClientProfileOnDemandSanctionDetailsActivityResponse>(
                            message.ClientIntegrationId,
                            message.ActivityIdentifier,
                            message.AutomonIdentifier,
                            RetrieveActivityDetails<ClientProfileOnDemandSanctionDetailsActivityResponse>(message.Details),
                            message.ActionUpdatedBy
                        );

                        //save details to Automon
                        offenderOnDemandSanctionService.SaveOffenderOnDemandSanctionDetails(ProcessorConfig.CmiDbConnString, offenderOnDemandSanctionDetails);

                        //mark this message as successful
                        message.IsSuccessful = true;

                        taskExecutionStatus.AutomonAddMessageCount++;
                        Logger.LogDebug(new LogRequest
                        {
                            OperationName = this.GetType().Name,
                            MethodName = "Execute",
                            Message = "Offender - OnDemand Sanction details saved successfully.",
                            AutomonData = JsonConvert.SerializeObject(offenderOnDemandSanctionDetails),
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
                            Message = "Error occurred while processing a Client Profile - OnDemand Sanction Details activity.",
                            Exception = ce,
                            AutomonData = JsonConvert.SerializeObject(offenderOnDemandSanctionDetails),
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
                            Message = "Critical error occurred while processing a Client Profile - OnDemand Sanction Details activity.",
                            Exception = ex,
                            AutomonData = JsonConvert.SerializeObject(offenderOnDemandSanctionDetails),
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
                    Message = "Critical error occurred while processing Client Profile - OnDemand Sanction Details activities.",
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
                Message = "OnDemand Sanction activity processing completed.",
                CustomParams = JsonConvert.SerializeObject(taskExecutionStatus)
            });

            return taskExecutionStatus;
        }
    }
}
