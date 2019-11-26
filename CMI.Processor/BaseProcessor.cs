
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

        protected IEnumerable<OutboundMessageDetails> UpdateIdentifiers(IEnumerable<OutboundMessageDetails> outboundMessages, bool isUpdateClientIntegrationId = false)
        {
            List<OutboundMessageDetails> updatedOutboundMessages = new List<OutboundMessageDetails>();

            outboundMessages.ToList().ForEach(m =>
            {
                updatedOutboundMessages.Add(new OutboundMessageDetails
                {
                    Id = m.Id,
                    ActivityTypeName = m.ActivityTypeName,
                    ActivitySubTypeName = m.ActivitySubTypeName,
                    ActionReasonName = m.ActionReasonName,
                    ClientIntegrationId = (
                        isUpdateClientIntegrationId
                        && outboundMessages.Any(x => 
                            x.ActivityTypeName.Equals("Client", StringComparison.InvariantCultureIgnoreCase)
                            && !string.IsNullOrEmpty(x.AutomonIdentifier) 
                            && x.ClientIntegrationId.Equals(m.ClientIntegrationId, StringComparison.InvariantCultureIgnoreCase)
                        )
                    )
                    ? outboundMessages
                            .Where(p => 
                                p.ActivityTypeName.Equals("Client", StringComparison.InvariantCultureIgnoreCase)
                                && !string.IsNullOrEmpty(p.AutomonIdentifier) && p.ClientIntegrationId.Equals(m.ClientIntegrationId, StringComparison.InvariantCultureIgnoreCase)
                            )
                            .OrderByDescending(q => q.ReceivedOn)
                            .FirstOrDefault()
                            .AutomonIdentifier
                    : m.ClientIntegrationId,
                    ActivityIdentifier = m.ActivityIdentifier,
                    ActionOccurredOn = m.ActionOccurredOn,
                    ActionUpdatedBy = m.ActionUpdatedBy,
                    Details = m.Details,
                    IsSuccessful = m.IsSuccessful,
                    ErrorDetails = m.ErrorDetails,
                    RawData = m.RawData,
                    IsProcessed = m.IsProcessed,
                    ReceivedOn = m.ReceivedOn,
                    AutomonIdentifier = (
                        outboundMessages.Any(x => 
                            !string.IsNullOrEmpty(x.AutomonIdentifier) 
                            && x.ActivityTypeName.Equals(m.ActivityTypeName, StringComparison.InvariantCultureIgnoreCase)
                            && x.ActivityIdentifier.Equals(m.ActivityIdentifier, StringComparison.InvariantCultureIgnoreCase)
                        )
                    )
                    ? outboundMessages
                            .Where(x => 
                                !string.IsNullOrEmpty(x.AutomonIdentifier)
                                && x.ActivityTypeName.Equals(m.ActivityTypeName, StringComparison.InvariantCultureIgnoreCase)
                                && x.ActivityIdentifier.Equals(m.ActivityIdentifier, StringComparison.InvariantCultureIgnoreCase)
                            )
                            .OrderByDescending(y => y.ReceivedOn)
                            .FirstOrDefault()
                            .AutomonIdentifier
                    : m.AutomonIdentifier
                });
            });

            return updatedOutboundMessages;
        }
    }
}
