using CMI.Automon.Interface;
using CMI.Common.Logging;
using CMI.Common.Notification;
using CMI.MessageRetriever.Model;
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

            TaskExecutionStatus taskExecutionStatus = new TaskExecutionStatus { ProcessorType = ProcessorType.Outbound, TaskName = "Process Note Activity", IsSuccessful = true, NexusReceivedMessageCount = messages.Count() };

            ////////////////////////////////////
            ////////////////////////////////////
            ////////////////////////////////////
            ////////////////////////////////////
            ////////////////////////////////////
            ////////////////////////////////////
            ////////////////////////////////////
            ////////////////////////////////////
            ////////////////////////////////////
            ////////////////////////////////////
            ////////////////////////////////////
            ////////////////////////////////////


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
