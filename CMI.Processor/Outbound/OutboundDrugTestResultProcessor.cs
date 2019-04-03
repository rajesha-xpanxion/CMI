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
    public class OutboundDrugTestResultProcessor : OutboundBaseProcessor
    {
        private readonly IOffenderDrugTestService offenderDrugTestService;

        public OutboundDrugTestResultProcessor(
            IServiceProvider serviceProvider,
            IConfiguration configuration,
            IOffenderDrugTestService offenderDrugTestService
        )
            : base(serviceProvider, configuration)
        {
            this.offenderDrugTestService = offenderDrugTestService;
        }

        public override TaskExecutionStatus Execute(IEnumerable<MessageBodyResponse> messages)
        {
            Logger.LogInfo(new LogRequest
            {
                OperationName = this.GetType().Name,
                MethodName = "Execute",
                Message = "Drug Test Result activity processing initiated."
            });

            TaskExecutionStatus taskExecutionStatus = new TaskExecutionStatus { ProcessorType = ProcessorType.Outbound, TaskName = "Process Drug Test Result Activity", IsSuccessful = true, NexusReceivedMessageCount = messages.Count() };

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
                Message = "Drug Test Result activity processing completed.",
                CustomParams = JsonConvert.SerializeObject(taskExecutionStatus)
            });

            return taskExecutionStatus;
        }
    }
}
