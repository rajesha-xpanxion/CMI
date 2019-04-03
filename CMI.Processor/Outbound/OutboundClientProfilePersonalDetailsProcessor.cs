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
    public class OutboundClientProfilePersonalDetailsProcessor : OutboundBaseProcessor
    {
        private readonly IOffenderService offenderService;

        public OutboundClientProfilePersonalDetailsProcessor(
            IServiceProvider serviceProvider,
            IConfiguration configuration,
            IOffenderService offenderService
        )
            : base(serviceProvider, configuration)
        {
            this.offenderService = offenderService;
        }

        public override TaskExecutionStatus Execute(IEnumerable<MessageBodyResponse> messages)
        {
            Logger.LogInfo(new LogRequest
            {
                OperationName = this.GetType().Name,
                MethodName = "Execute",
                Message = "Client Profile Personal Details activity processing initiated."
            });

            TaskExecutionStatus taskExecutionStatus = new TaskExecutionStatus { ProcessorType = ProcessorType.Outbound, TaskName = "Process Client Profile - Personal Details Activity", IsSuccessful = true, NexusReceivedMessageCount = messages.Count() };

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
                Message = "Client Profile Personal Details activity processing completed.",
                CustomParams = JsonConvert.SerializeObject(taskExecutionStatus)
            });

            return taskExecutionStatus;
        }
    }
}
