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

            ProcessorExecutionStatus = new ExecutionStatus { ProcessorType = DAL.ProcessorType.Inbound, ExecutedOn = DateTime.Now, IsSuccessful = true, NumTaskProcessed = 0, NumTaskSucceeded = 0, NumTaskFailed = 0 };
            TaskExecutionStatuses = new List<TaskExecutionStatus>();
        }

        public override IEnumerable<TaskExecutionStatus> Execute()
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
            if (ProcessorConfig.InboundProcessorConfig.StagesToProcess != null && ProcessorConfig.InboundProcessorConfig.StagesToProcess.Any(a => a.Equals(InboundProcessorStage.ClientProfiles, StringComparison.InvariantCultureIgnoreCase)))
            {
                UpdateExecutionStatus(((InboundClientProfileProcessor)serviceProvider.GetService(typeof(InboundClientProfileProcessor))).Execute(lastExecutionDateTime));
            }

            //process client addresses
            if (ProcessorConfig.InboundProcessorConfig.StagesToProcess != null && ProcessorConfig.InboundProcessorConfig.StagesToProcess.Any(a => a.Equals(InboundProcessorStage.Addresses, StringComparison.InvariantCultureIgnoreCase)))
            {
                UpdateExecutionStatus(((InboundAddressProcessor)serviceProvider.GetService(typeof(InboundAddressProcessor))).Execute(lastExecutionDateTime));
            }

            //process client phone contacts
            if (ProcessorConfig.InboundProcessorConfig.StagesToProcess != null && ProcessorConfig.InboundProcessorConfig.StagesToProcess.Any(a => a.Equals(InboundProcessorStage.PhoneContacts, StringComparison.InvariantCultureIgnoreCase)))
            {
                UpdateExecutionStatus(((InboundPhoneContactProcessor)serviceProvider.GetService(typeof(InboundPhoneContactProcessor))).Execute(lastExecutionDateTime));
            }

            //process client email contacts
            if (ProcessorConfig.InboundProcessorConfig.StagesToProcess != null && ProcessorConfig.InboundProcessorConfig.StagesToProcess.Any(a => a.Equals(InboundProcessorStage.EmailContacts, StringComparison.InvariantCultureIgnoreCase)))
            {
                UpdateExecutionStatus(((InboundEmailContactProcessor)serviceProvider.GetService(typeof(InboundEmailContactProcessor))).Execute(lastExecutionDateTime));
            }

            //process client cases
            if (ProcessorConfig.InboundProcessorConfig.StagesToProcess != null && ProcessorConfig.InboundProcessorConfig.StagesToProcess.Any(a => a.Equals(InboundProcessorStage.Cases, StringComparison.InvariantCultureIgnoreCase)))
            {
                UpdateExecutionStatus(((InboundCaseProcessor)serviceProvider.GetService(typeof(InboundCaseProcessor))).Execute(lastExecutionDateTime));
            }

            //process client notes
            if (ProcessorConfig.InboundProcessorConfig.StagesToProcess != null && ProcessorConfig.InboundProcessorConfig.StagesToProcess.Any(a => a.Equals(InboundProcessorStage.Notes, StringComparison.InvariantCultureIgnoreCase)))
            {
                UpdateExecutionStatus(((InboundNoteProcessor)serviceProvider.GetService(typeof(InboundNoteProcessor))).Execute(lastExecutionDateTime));
            }

            //derive final processor execution status and save it to database
            ProcessorExecutionStatus.ExecutionStatusMessage = ProcessorExecutionStatus.IsSuccessful
                ? "Inbound Processor execution completed successfully."
                : "Inbound Processor execution failed. Please check logs for more details.";

            //save execution status details in history table
            SaveExecutionStatus(ProcessorExecutionStatus);

            //log info message for end of processing
            Logger.LogInfo(new LogRequest
            {
                OperationName = this.GetType().Name,
                MethodName = "Execute",
                Message = "Inbound Processor execution completed.",
                CustomParams = JsonConvert.SerializeObject(ProcessorExecutionStatus)
            });

            return TaskExecutionStatuses;
        }

        private void RetrieveLastExecutionDateTime()
        {
            try
            {
                lastExecutionDateTime = processorProvider.GetLastExecutionDateTime(DAL.ProcessorType.Inbound);

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
