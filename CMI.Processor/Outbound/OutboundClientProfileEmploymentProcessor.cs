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

        public override TaskExecutionStatus Execute(IEnumerable<MessageBodyResponse> messages)
        {
            Logger.LogInfo(new LogRequest
            {
                OperationName = this.GetType().Name,
                MethodName = "Execute",
                Message = "Client Profile Employment activity processing initiated."
            });

            TaskExecutionStatus taskExecutionStatus = new TaskExecutionStatus { ProcessorType = ProcessorType.Outbound, TaskName = "Process Client Profile - Employment Activity", IsSuccessful = true, NexusReceivedMessageCount = messages.Count() };

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
                Message = "Client Profile Employment activity processing completed.",
                CustomParams = JsonConvert.SerializeObject(taskExecutionStatus)
            });

            return taskExecutionStatus;
        }
    }
}
