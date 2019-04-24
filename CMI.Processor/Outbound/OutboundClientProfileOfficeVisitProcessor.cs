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
    public class OutboundClientProfileOfficeVisitProcessor : OutboundBaseProcessor
    {
        private readonly IOffenderOfficeVisitService offenderOfficeVisitService;

        public OutboundClientProfileOfficeVisitProcessor(
            IServiceProvider serviceProvider,
            IConfiguration configuration,
            IOffenderOfficeVisitService offenderOfficeVisitService
        )
            : base(serviceProvider, configuration)
        {
            this.offenderOfficeVisitService = offenderOfficeVisitService;
        }

        public override TaskExecutionStatus Execute(IEnumerable<OutboundMessageDetails> messages, DateTime messagesReceivedOn)
        {
            Logger.LogInfo(new LogRequest
            {
                OperationName = this.GetType().Name,
                MethodName = "Execute",
                Message = "Office Visit activity processing initiated."
            });

            TaskExecutionStatus taskExecutionStatus = new TaskExecutionStatus
            {
                ProcessorType = Common.Notification.ProcessorType.Outbound,
                TaskName = "Office Visit",
                IsSuccessful = true,
                NexusReceivedMessageCount = messages.Count()
            };

            try
            {
                foreach (OutboundMessageDetails message in messages)
                {
                    OffenderOfficeVisit offenderOfficeVisitDetails = null;
                    message.IsProcessed = true;
                    try
                    {
                        offenderOfficeVisitDetails = (OffenderOfficeVisit)ConvertResponseToObject<ClientProfileOfficeVisitDetailsActivityResponse>(
                            message.ClientIntegrationId,
                            RetrieveActivityDetails<ClientProfileOfficeVisitDetailsActivityResponse>(message.Details),
                            message.ActionUpdatedBy
                        );

                        offenderOfficeVisitService.SaveOffenderOfficeVisitDetails(ProcessorConfig.CmiDbConnString, offenderOfficeVisitDetails);

                        taskExecutionStatus.AutomonAddMessageCount++;
                        message.IsSuccessful = true;

                        Logger.LogDebug(new LogRequest
                        {
                            OperationName = this.GetType().Name,
                            MethodName = "Execute",
                            Message = "New Offender - Office Visit Details added successfully.",
                            AutomonData = JsonConvert.SerializeObject(offenderOfficeVisitDetails),
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
                            Message = "Error occurred while processing a Client Profile - Office Visit Details activity.",
                            Exception = ce,
                            AutomonData = JsonConvert.SerializeObject(offenderOfficeVisitDetails),
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
                            Message = "Critical error occurred while processing a Client Profile - Office Visit Details activity.",
                            Exception = ex,
                            AutomonData = JsonConvert.SerializeObject(offenderOfficeVisitDetails),
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
                    Message = "Critical error occurred while processing Client Profile - Office Visit Details activities.",
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
                Message = "Office Visit activity processing completed.",
                CustomParams = JsonConvert.SerializeObject(taskExecutionStatus)
            });

            return taskExecutionStatus;
        }
    }
}
