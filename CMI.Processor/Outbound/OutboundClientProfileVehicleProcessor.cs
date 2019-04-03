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

        public override TaskExecutionStatus Execute(IEnumerable<MessageBodyResponse> messages)
        {
            Logger.LogInfo(new LogRequest
            {
                OperationName = this.GetType().Name,
                MethodName = "Execute",
                Message = "Client Profile Vehicle activity processing initiated."
            });

            TaskExecutionStatus taskExecutionStatus = new TaskExecutionStatus { ProcessorType = ProcessorType.Outbound, TaskName = "Process Client Profile - Vehicle Activity", IsSuccessful = true, NexusReceivedMessageCount = messages.Count() };

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
                Message = "Client Profile Vehicle activity processing completed.",
                CustomParams = JsonConvert.SerializeObject(taskExecutionStatus)
            });

            return taskExecutionStatus;
        }
    }
}
