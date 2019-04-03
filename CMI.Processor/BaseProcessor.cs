
using CMI.Common.Logging;
using CMI.Common.Notification;
using CMI.Processor.DAL;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace CMI.Processor
{
    public abstract class BaseProcessor
    {
        protected ILogger Logger { get; set; }
        protected IProcessorProvider ProcessorProvider { get; set; }
        protected IEmailNotificationProvider EmailNotificationProvider { get; set; }
        protected ProcessorConfig ProcessorConfig { get; set; }

        protected ExecutionStatus ProcessorExecutionStatus { get; set; }
        protected List<TaskExecutionStatus> TaskExecutionStatuses { get; set; }

        public BaseProcessor(
            IServiceProvider serviceProvider,
            IConfiguration configuration
        )
        {
            Logger = (ILogger)serviceProvider.GetService(typeof(ILogger));
            ProcessorProvider = (IProcessorProvider)serviceProvider.GetService(typeof(IProcessorProvider));
            EmailNotificationProvider = (IEmailNotificationProvider)serviceProvider.GetService(typeof(IEmailNotificationProvider));

            ProcessorConfig = configuration.GetSection(ConfigKeys.ProcessorConfig).Get<ProcessorConfig>();
        }

        public abstract IEnumerable<TaskExecutionStatus> Execute();

        protected void UpdateExecutionStatus(TaskExecutionStatus taskExecutionStatus)
        {
            ProcessorExecutionStatus.NumTaskProcessed++;
            if (taskExecutionStatus != null)
            {
                if (taskExecutionStatus.IsSuccessful)
                {
                    ProcessorExecutionStatus.NumTaskSucceeded++;
                }
                else
                {
                    ProcessorExecutionStatus.NumTaskFailed++;
                }

                ProcessorExecutionStatus.IsSuccessful = ProcessorExecutionStatus.IsSuccessful & taskExecutionStatus.IsSuccessful;
                TaskExecutionStatuses.Add(taskExecutionStatus);
            }
        }

        protected void SaveExecutionStatus(ExecutionStatus executionStatus)
        {
            try
            {
                ProcessorProvider.SaveExecutionStatus(executionStatus);
            }
            catch (Exception ex)
            {
                Logger.LogError(new LogRequest
                {
                    OperationName = this.GetType().Name,
                    MethodName = "SaveExecutionStatus",
                    Message = string.Format("Error occurred while saving {0} processor execution status.", executionStatus.ProcessorType.ToString()),
                    Exception = ex,
                    CustomParams = JsonConvert.SerializeObject(executionStatus)
                });
            }
        }
    }
}
