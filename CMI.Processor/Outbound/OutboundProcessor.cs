using CMI.Common.Logging;
using CMI.Common.Notification;
using CMI.Processor.DAL;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CMI.Processor
{
    public class OutboundProcessor : BaseProcessor
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IConfiguration configuration;

        public OutboundProcessor(
            IServiceProvider serviceProvider,
            IConfiguration configuration
        )
            : base(serviceProvider, configuration)
        {
            this.serviceProvider = serviceProvider;
            this.configuration = configuration;

            ProcessorExecutionStatus = new ExecutionStatus { ProcessorType = ProcessorType.Outbound, ExecutedOn = DateTime.Now, IsSuccessful = true, NumTaskProcessed = 0, NumTaskSucceeded = 0, NumTaskFailed = 0 };
            TaskExecutionStatuses = new List<TaskExecutionStatus>();
        }

        public override void Execute()
        {
            //log info message for start of processing
            Logger.LogInfo(new LogRequest
            {
                OperationName = this.GetType().Name,
                MethodName = "Execute",
                Message = "Outbound Processor execution initiated."
            });

            //derive final processor execution status and save it to database
            ProcessorExecutionStatus.ExecutionStatusMessage = ProcessorExecutionStatus.IsSuccessful
                ? "Outbound Processor execution completed successfully."
                : "Outbound Processor execution failed. Please check logs for more details.";

            //save execution status details in history table
            SaveExecutionStatus(ProcessorExecutionStatus);

            //send execution status report email
            var executionStatusReportEmailRequest = new ExecutionStatusReportEmailRequest
            {
                ToEmailAddress = ProcessorConfig.ExecutionStatusReportReceiverEmailAddresses,
                Subject = ProcessorConfig.ExecutionStatusReportEmailSubject,
                TaskExecutionStatuses = TaskExecutionStatuses
            };

            var response = EmailNotificationProvider.SendExecutionStatusReportEmail(executionStatusReportEmailRequest);

            if (response.IsSuccessful)
            {
                Logger.LogInfo(new LogRequest
                {
                    OperationName = this.GetType().Name,
                    MethodName = "Execute",
                    Message = "Execution status report email sent successfully.",
                    CustomParams = JsonConvert.SerializeObject(executionStatusReportEmailRequest)
                });
            }
            else
            {
                Logger.LogWarning(new LogRequest
                {
                    OperationName = this.GetType().Name,
                    MethodName = "Execute",
                    Message = "Error occurred while sending execution status report email.",
                    Exception = response.Exception,
                    CustomParams = JsonConvert.SerializeObject(executionStatusReportEmailRequest)
                });
            }

            //log info message for end of processing
            Logger.LogInfo(new LogRequest
            {
                OperationName = this.GetType().Name,
                MethodName = "Execute",
                Message = "Outbound Processor execution completed.",
                CustomParams = JsonConvert.SerializeObject(ProcessorExecutionStatus)
            });
        }
    }
}
