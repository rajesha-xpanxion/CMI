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
    public class OutboundOfficeVisitProcessor : OutboundBaseProcessor
    {
        private readonly IOffenderOfficeVisitService offenderOfficeVisitService;

        public OutboundOfficeVisitProcessor(
            IServiceProvider serviceProvider,
            IConfiguration configuration,
            IOffenderOfficeVisitService offenderOfficeVisitService
        )
            : base(serviceProvider, configuration)
        {
            this.offenderOfficeVisitService = offenderOfficeVisitService;
        }

        public override TaskExecutionStatus Execute(IEnumerable<MessageBodyResponse> messages)
        {
            Logger.LogInfo(new LogRequest
            {
                OperationName = this.GetType().Name,
                MethodName = "Execute",
                Message = "Office Visit activity processing initiated."
            });

            TaskExecutionStatus taskExecutionStatus = new TaskExecutionStatus
            {
                ProcessorType = ProcessorType.Outbound,
                TaskName = "Process Office Visit Activity",
                IsSuccessful = true,
                NexusReceivedMessageCount = messages.Count()
            };

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
                Message = "Office Visit activity processing completed.",
                CustomParams = JsonConvert.SerializeObject(taskExecutionStatus)
            });

            return taskExecutionStatus;
        }
    }
}
