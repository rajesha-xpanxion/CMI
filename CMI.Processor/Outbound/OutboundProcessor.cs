using CMI.Common.Logging;
using CMI.Common.Notification;
using CMI.MessageRetriever.Interface;
using CMI.MessageRetriever.Model;
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

            //retrieve all messages from queue
            IEnumerable<MessageBodyResponse> messages = ((IMessageRetrieverService)serviceProvider.GetService(typeof(IMessageRetrieverService))).Execute().Result;

            //process each type of message based on whether it is allowed or not
            //general notes
            if (
                ProcessorConfig.OutboundProcessorConfig.ActivityTypesToProcess != null 
                && ProcessorConfig.OutboundProcessorConfig.ActivityTypesToProcess.Any(a => a.Equals(OutboundProcessorActivityType.Note, StringComparison.InvariantCultureIgnoreCase))
            )
            {
                UpdateExecutionStatus(((OutboundNoteProcessor)serviceProvider.GetService(typeof(OutboundNoteProcessor))).Execute(
                    messages.Where(a => a.Activity != null && a.Activity.Type.Equals(OutboundProcessorActivityType.Note, StringComparison.InvariantCultureIgnoreCase)))
                );
            }

            //office visit
            if (
                ProcessorConfig.OutboundProcessorConfig.ActivityTypesToProcess != null
                && ProcessorConfig.OutboundProcessorConfig.ActivityTypesToProcess.Any(a => a.Equals(OutboundProcessorActivityType.OfficeVisit, StringComparison.InvariantCultureIgnoreCase))
            )
            {
                UpdateExecutionStatus(((OutboundOfficeVisitProcessor)serviceProvider.GetService(typeof(OutboundOfficeVisitProcessor))).Execute(
                    messages.Where(a => a.Activity != null && a.Activity.Type.Equals(OutboundProcessorActivityType.OfficeVisit, StringComparison.InvariantCultureIgnoreCase)))
                );
            }

            //drug test appointment
            if (
                ProcessorConfig.OutboundProcessorConfig.ActivityTypesToProcess != null
                && ProcessorConfig.OutboundProcessorConfig.ActivityTypesToProcess.Any(a => a.Equals(OutboundProcessorActivityType.DrugTestAppointment, StringComparison.InvariantCultureIgnoreCase))
            )
            {
                UpdateExecutionStatus(((OutboundDrugTestAppointmentProcessor)serviceProvider.GetService(typeof(OutboundDrugTestAppointmentProcessor))).Execute(
                    messages.Where(a => a.Activity != null && a.Activity.Type.Equals(OutboundProcessorActivityType.DrugTestAppointment, StringComparison.InvariantCultureIgnoreCase)))
                );
            }

            //drug test result
            if (
                ProcessorConfig.OutboundProcessorConfig.ActivityTypesToProcess != null
                && ProcessorConfig.OutboundProcessorConfig.ActivityTypesToProcess.Any(a => a.Equals(OutboundProcessorActivityType.DrugTestResult, StringComparison.InvariantCultureIgnoreCase))
            )
            {
                UpdateExecutionStatus(((OutboundDrugTestResultProcessor)serviceProvider.GetService(typeof(OutboundDrugTestResultProcessor))).Execute(
                    messages.Where(a => a.Activity != null && a.Activity.Type.Equals(OutboundProcessorActivityType.DrugTestResult, StringComparison.InvariantCultureIgnoreCase)))
                );
            }

            //field visit
            if (
                ProcessorConfig.OutboundProcessorConfig.ActivityTypesToProcess != null
                && ProcessorConfig.OutboundProcessorConfig.ActivityTypesToProcess.Any(a => a.Equals(OutboundProcessorActivityType.FieldVisit, StringComparison.InvariantCultureIgnoreCase))
            )
            {
                UpdateExecutionStatus(((OutboundFieldVisitProcessor)serviceProvider.GetService(typeof(OutboundFieldVisitProcessor))).Execute(
                    messages.Where(a => a.Activity != null && a.Activity.Type.Equals(OutboundProcessorActivityType.FieldVisit, StringComparison.InvariantCultureIgnoreCase)))
                );
            }

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
