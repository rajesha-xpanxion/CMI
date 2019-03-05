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
    public class InboundProcessor : BaseProcessor
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IConfiguration configuration;
        private readonly IProcessorProvider processorProvider;
        private DateTime? lastExecutionDateTime;

        public InboundProcessor(
            IServiceProvider serviceProvider,
            IConfiguration configuration
        )
            : base(serviceProvider, configuration)
        {
            this.serviceProvider = serviceProvider;
            this.configuration = configuration;

            processorProvider = (ProcessorProvider)serviceProvider.GetService(typeof(IProcessorProvider));

            ProcessorExecutionStatus = new ExecutionStatus { ProcessorType = ProcessorType.Inbound, ExecutedOn = DateTime.Now, IsSuccessful = true, NumTaskProcessed = 0, NumTaskSucceeded = 0, NumTaskFailed = 0 };
            TaskExecutionStatuses = new List<TaskExecutionStatus>();
        }

        public override void Execute()
        {
            //log info message for start of processing
            Logger.LogInfo(new LogRequest
            {
                OperationName = this.GetType().Name,
                MethodName = "Execute",
                Message = "Inbound Processor execution initiated."
            });

            //retrieve last execution date time
            RetrieveLastExecutionDateTime();

            //process client profiles
            if (ProcessorConfig.InboundProcessorStagesToProcess != null && ProcessorConfig.InboundProcessorStagesToProcess.Any(a => a.Equals(InboundProcessorStage.ClientProfiles, StringComparison.InvariantCultureIgnoreCase)))
            {
                UpdateExecutionStatus(((InboundClientProfileProcessor)serviceProvider.GetService(typeof(InboundClientProfileProcessor))).Execute(lastExecutionDateTime));
            }

            //process client addresses
            if (ProcessorConfig.InboundProcessorStagesToProcess != null && ProcessorConfig.InboundProcessorStagesToProcess.Any(a => a.Equals(InboundProcessorStage.Addresses, StringComparison.InvariantCultureIgnoreCase)))
            {
                UpdateExecutionStatus(((InboundAddressProcessor)serviceProvider.GetService(typeof(InboundAddressProcessor))).Execute(lastExecutionDateTime));
            }

            //process client phone contacts
            if (ProcessorConfig.InboundProcessorStagesToProcess != null && ProcessorConfig.InboundProcessorStagesToProcess.Any(a => a.Equals(InboundProcessorStage.PhoneContacts, StringComparison.InvariantCultureIgnoreCase)))
            {
                UpdateExecutionStatus(((InboundPhoneContactProcessor)serviceProvider.GetService(typeof(InboundPhoneContactProcessor))).Execute(lastExecutionDateTime));
            }

            //process client email contacts
            if (ProcessorConfig.InboundProcessorStagesToProcess != null && ProcessorConfig.InboundProcessorStagesToProcess.Any(a => a.Equals(InboundProcessorStage.EmailContacts, StringComparison.InvariantCultureIgnoreCase)))
            {
                UpdateExecutionStatus(((InboundEmailContactProcessor)serviceProvider.GetService(typeof(InboundEmailContactProcessor))).Execute(lastExecutionDateTime));
            }

            //process client cases
            if (ProcessorConfig.InboundProcessorStagesToProcess != null && ProcessorConfig.InboundProcessorStagesToProcess.Any(a => a.Equals(InboundProcessorStage.Cases, StringComparison.InvariantCultureIgnoreCase)))
            {
                UpdateExecutionStatus(((InboundCaseProcessor)serviceProvider.GetService(typeof(InboundCaseProcessor))).Execute(lastExecutionDateTime));
            }

            //process client notes
            if (ProcessorConfig.InboundProcessorStagesToProcess != null && ProcessorConfig.InboundProcessorStagesToProcess.Any(a => a.Equals(InboundProcessorStage.Notes, StringComparison.InvariantCultureIgnoreCase)))
            {
                UpdateExecutionStatus(((InboundNoteProcessor)serviceProvider.GetService(typeof(InboundNoteProcessor))).Execute(lastExecutionDateTime));
            }

            //derive final processor execution status and save it to database
            ProcessorExecutionStatus.ExecutionStatusMessage = ProcessorExecutionStatus.IsSuccessful
                ? "Inbound Processor execution completed successfully."
                : "Inbound Processor execution failed. Please check logs for more details.";

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
                Message = "Inbound Processor execution completed.",
                CustomParams = JsonConvert.SerializeObject(ProcessorExecutionStatus)
            });
        }

        private void RetrieveLastExecutionDateTime()
        {
            try
            {
                lastExecutionDateTime = processorProvider.GetLastExecutionDateTime(ProcessorType.Inbound);

                Logger.LogInfo(new LogRequest
                {
                    OperationName = this.GetType().Name,
                    MethodName = "RetrieveLastExecutionDateTime",
                    Message = "Successfully retrieved Last Execution Date Time",
                    CustomParams = JsonConvert.SerializeObject(lastExecutionDateTime)
                });
            }
            catch (Exception ex)
            {
                Logger.LogError(new LogRequest
                {
                    OperationName = this.GetType().Name,
                    MethodName = "RetrieveLastExecutionDateTime",
                    Message = "Error occurred while retriving processor last execution status.",
                    Exception = ex
                });
            }
        }
    }
}
