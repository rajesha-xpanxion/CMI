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
        private LastExecutionStatus lastExecutionStatus;

        public InboundProcessor(
            IServiceProvider serviceProvider,
            IConfiguration configuration
        )
            : base(serviceProvider, configuration)
        {
            this.serviceProvider = serviceProvider;
            this.configuration = configuration;

            processorProvider = (ProcessorProvider)serviceProvider.GetService(typeof(IProcessorProvider));

            ProcessorExecutionStatus = new ExecutionStatus { ProcessorType = DAL.ProcessorType.Inbound, ExecutedOn = DateTime.Now, IsExecutedInIncrementalMode = true, IsSuccessful = true, NumTaskProcessed = 0, NumTaskSucceeded = 0, NumTaskFailed = 0 };
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

            DateTime? lastExecutionDateTime = lastExecutionStatus.LastIncrementalModeExecutionDateTime;
            //derive whether current execution should be in incremental or non-incremental mode
            if (
                !string.IsNullOrEmpty(ProcessorConfig.TimespanForNonIncrementalModeExecution)
                && int.TryParse(ProcessorConfig.TimespanForNonIncrementalModeExecution.Split(":")[0], out int timeSpanHours)
                && int.TryParse(ProcessorConfig.TimespanForNonIncrementalModeExecution.Split(":")[1], out int timeSpanMinutes)
            )
            {
                if (
                    lastExecutionStatus.LastNonIncrementalModeExecutionDateTime.HasValue
                    && lastExecutionStatus.LastNonIncrementalModeExecutionDateTime.Value.AddHours(timeSpanHours).AddMinutes(timeSpanMinutes) >= DateTime.Now
                )
                {
                    //set processor in Incremental mode
                    lastExecutionDateTime = lastExecutionStatus.LastIncrementalModeExecutionDateTime;
                    ProcessorExecutionStatus.IsExecutedInIncrementalMode = true;
                }
                else
                {
                    //set processor in Non-Incremental mode
                    lastExecutionDateTime = null;
                    ProcessorExecutionStatus.IsExecutedInIncrementalMode = false;

                    //log list of officers configured
                    Logger.LogDebug(new LogRequest
                    {
                        OperationName = this.GetType().Name,
                        MethodName = "Execute",
                        Message = "List of officers configured for integration.",
                        CustomParams = JsonConvert.SerializeObject(ProcessorConfig.InboundProcessorConfig.OfficerLogonsToFilter)
                    });
                }
            }
            else
            {
                lastExecutionDateTime = lastExecutionStatus.LastIncrementalModeExecutionDateTime;
                ProcessorExecutionStatus.IsExecutedInIncrementalMode = true;
            }

            //log execution mode details
            Logger.LogDebug(new LogRequest
            {
                OperationName = this.GetType().Name,
                MethodName = "Execute",
                Message = string.Format("Inbound Processor is currently being executed in {0} mode.", ProcessorExecutionStatus.IsExecutedInIncrementalMode ? ProcessorExecutionMode.Incremental : ProcessorExecutionMode.NonIncremental)
            });

            //process client profiles
            if (ProcessorConfig.InboundProcessorConfig.StagesToProcess != null && ProcessorConfig.InboundProcessorConfig.StagesToProcess.Any(a => a.Equals(InboundProcessorStage.ClientProfiles, StringComparison.InvariantCultureIgnoreCase)))
            {
                UpdateExecutionStatus(((InboundClientProfileProcessor)serviceProvider.GetService(typeof(InboundClientProfileProcessor))).Execute(lastExecutionDateTime, ProcessorConfig.InboundProcessorConfig.OfficerLogonsToFilter));
            }

            //process client addresses
            if (ProcessorConfig.InboundProcessorConfig.StagesToProcess != null && ProcessorConfig.InboundProcessorConfig.StagesToProcess.Any(a => a.Equals(InboundProcessorStage.Addresses, StringComparison.InvariantCultureIgnoreCase)))
            {
                UpdateExecutionStatus(((InboundAddressProcessor)serviceProvider.GetService(typeof(InboundAddressProcessor))).Execute(lastExecutionDateTime, ProcessorConfig.InboundProcessorConfig.OfficerLogonsToFilter));
            }

            //process client phone contacts
            if (ProcessorConfig.InboundProcessorConfig.StagesToProcess != null && ProcessorConfig.InboundProcessorConfig.StagesToProcess.Any(a => a.Equals(InboundProcessorStage.PhoneContacts, StringComparison.InvariantCultureIgnoreCase)))
            {
                UpdateExecutionStatus(((InboundPhoneContactProcessor)serviceProvider.GetService(typeof(InboundPhoneContactProcessor))).Execute(lastExecutionDateTime, ProcessorConfig.InboundProcessorConfig.OfficerLogonsToFilter));
            }

            //process client email contacts
            if (ProcessorConfig.InboundProcessorConfig.StagesToProcess != null && ProcessorConfig.InboundProcessorConfig.StagesToProcess.Any(a => a.Equals(InboundProcessorStage.EmailContacts, StringComparison.InvariantCultureIgnoreCase)))
            {
                UpdateExecutionStatus(((InboundEmailContactProcessor)serviceProvider.GetService(typeof(InboundEmailContactProcessor))).Execute(lastExecutionDateTime, ProcessorConfig.InboundProcessorConfig.OfficerLogonsToFilter));
            }

            //process client cases
            if (ProcessorConfig.InboundProcessorConfig.StagesToProcess != null && ProcessorConfig.InboundProcessorConfig.StagesToProcess.Any(a => a.Equals(InboundProcessorStage.Cases, StringComparison.InvariantCultureIgnoreCase)))
            {
                UpdateExecutionStatus(((InboundCaseProcessor)serviceProvider.GetService(typeof(InboundCaseProcessor))).Execute(lastExecutionDateTime, ProcessorConfig.InboundProcessorConfig.OfficerLogonsToFilter));
            }

            //process client notes
            if (ProcessorConfig.InboundProcessorConfig.StagesToProcess != null && ProcessorConfig.InboundProcessorConfig.StagesToProcess.Any(a => a.Equals(InboundProcessorStage.Notes, StringComparison.InvariantCultureIgnoreCase)))
            {
                UpdateExecutionStatus(((InboundNoteProcessor)serviceProvider.GetService(typeof(InboundNoteProcessor))).Execute(lastExecutionDateTime, ProcessorConfig.InboundProcessorConfig.OfficerLogonsToFilter));
            }

            //process client vehicles
            if (ProcessorConfig.InboundProcessorConfig.StagesToProcess != null && ProcessorConfig.InboundProcessorConfig.StagesToProcess.Any(a => a.Equals(InboundProcessorStage.Vehicles, StringComparison.InvariantCultureIgnoreCase)))
            {
                UpdateExecutionStatus(((InboundVehicleProcessor)serviceProvider.GetService(typeof(InboundVehicleProcessor))).Execute(lastExecutionDateTime, ProcessorConfig.InboundProcessorConfig.OfficerLogonsToFilter));
            }

            //process client employments
            if (ProcessorConfig.InboundProcessorConfig.StagesToProcess != null && ProcessorConfig.InboundProcessorConfig.StagesToProcess.Any(a => a.Equals(InboundProcessorStage.Employments, StringComparison.InvariantCultureIgnoreCase)))
            {
                UpdateExecutionStatus(((InboundEmploymentProcessor)serviceProvider.GetService(typeof(InboundEmploymentProcessor))).Execute(lastExecutionDateTime, ProcessorConfig.InboundProcessorConfig.OfficerLogonsToFilter));
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
                lastExecutionStatus = processorProvider.GetLastExecutionDateTime(DAL.ProcessorType.Inbound);

                Logger.LogInfo(new LogRequest
                {
                    OperationName = this.GetType().Name,
                    MethodName = "RetrieveLastExecutionDateTime",
                    Message = "Successfully retrieved Last Execution Date Time.",
                    CustomParams = JsonConvert.SerializeObject(lastExecutionStatus)
                });
            }
            catch (Exception ex)
            {
                Logger.LogError(new LogRequest
                {
                    OperationName = this.GetType().Name,
                    MethodName = "RetrieveLastExecutionDateTime",
                    Message = "Error occurred while retriving processor Last Execution Date Time.",
                    Exception = ex
                });
            }
        }
    }
}
