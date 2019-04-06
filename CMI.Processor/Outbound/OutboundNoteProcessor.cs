using CMI.Automon.Interface;
using CMI.Automon.Model;
using CMI.Common.Logging;
using CMI.Common.Notification;
using CMI.MessageRetriever.Model;
using CMI.Nexus.Model;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CMI.Processor
{
    public class OutboundNoteProcessor: OutboundBaseProcessor
    {
        private readonly IOffenderNoteService offenderNoteService;

        public OutboundNoteProcessor(
            IServiceProvider serviceProvider,
            IConfiguration configuration,
            IOffenderNoteService offenderNoteService
        )
            : base(serviceProvider, configuration)
        {
            this.offenderNoteService = offenderNoteService;
        }

        public override TaskExecutionStatus Execute(IEnumerable<MessageBodyResponse> messages)
        {
            Logger.LogInfo(new LogRequest
            {
                OperationName = this.GetType().Name,
                MethodName = "Execute",
                Message = "Note activity processing initiated."
            });

            TaskExecutionStatus taskExecutionStatus = new TaskExecutionStatus
            {
                ProcessorType = ProcessorType.Outbound,
                TaskName = "Process Note Activity",
                IsSuccessful = true,
                NexusReceivedMessageCount = messages.Count()
            };

            try
            {
                foreach(MessageBodyResponse message in messages)
                {
                    OffenderNote offenderNoteDetails = null;
                    try
                    {
                        offenderNoteDetails = ConvertResponseToObject<NoteActivityDetailsResponse>(
                            message.Client,
                            RetrieveActivityDetails<NoteActivityDetailsResponse>(message.Details)
                        );

                        offenderNoteService.SaveOffenderNoteDetails(ProcessorConfig.CmiDbConnString, offenderNoteDetails);

                        taskExecutionStatus.AutomonAddMessageCount++;

                        Logger.LogDebug(new LogRequest
                        {
                            OperationName = this.GetType().Name,
                            MethodName = "Execute",
                            Message = "New Offender Note details added successfully.",
                            AutomonData = JsonConvert.SerializeObject(offenderNoteDetails),
                            NexusData = JsonConvert.SerializeObject(message)
                        });
                    }
                    catch (CmiException ce)
                    {
                        taskExecutionStatus.AutomonFailureMessageCount++;

                        Logger.LogWarning(new LogRequest
                        {
                            OperationName = this.GetType().Name,
                            MethodName = "Execute",
                            Message = "Error occurred while processing a Note activity.",
                            Exception = ce,
                            AutomonData = JsonConvert.SerializeObject(offenderNoteDetails),
                            NexusData = JsonConvert.SerializeObject(message)
                        });
                    }
                    catch (Exception ex)
                    {
                        taskExecutionStatus.AutomonFailureMessageCount++;

                        Logger.LogError(new LogRequest
                        {
                            OperationName = this.GetType().Name,
                            MethodName = "Execute",
                            Message = "Error occurred while processing a Note activity.",
                            Exception = ex,
                            AutomonData = JsonConvert.SerializeObject(offenderNoteDetails),
                            NexusData = JsonConvert.SerializeObject(message)
                        });
                    }
                }

                taskExecutionStatus.IsSuccessful = taskExecutionStatus.AutomonFailureMessageCount == 0;
            }
            catch (Exception ex)
            {
                taskExecutionStatus.IsSuccessful = false;

                Logger.LogError(new LogRequest
                {
                    OperationName = this.GetType().Name,
                    MethodName = "Execute",
                    Message = "Error occurred while processing Note activity.",
                    Exception = ex,
                    AutomonData = JsonConvert.SerializeObject(messages)
                });
            }


            Logger.LogInfo(new LogRequest
            {
                OperationName = this.GetType().Name,
                MethodName = "Execute",
                Message = "Note activity processing completed.",
                CustomParams = JsonConvert.SerializeObject(taskExecutionStatus)
            });

            return taskExecutionStatus;
        }
    }
}
