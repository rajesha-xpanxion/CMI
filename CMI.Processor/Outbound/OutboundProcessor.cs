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

            ProcessorExecutionStatus = new ExecutionStatus { ProcessorType = DAL.ProcessorType.Outbound, ExecutedOn = DateTime.Now, IsSuccessful = true, NumTaskProcessed = 0, NumTaskSucceeded = 0, NumTaskFailed = 0 };
            TaskExecutionStatuses = new List<TaskExecutionStatus>();
        }

        public override IEnumerable<TaskExecutionStatus> Execute()
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

            //save details of received messages in database
            ProcessorProvider.SaveOutboundMessages(messages.Select(m => new OutboundMessageDetails
            {
                Id = 0,
                ActivityTypeName = m.Activity.Type,
                ActionReasonName = m.Action.Reason,
                ClientIntegrationId = m.Client.IntegrationId,
                ActivityIdentifier = m.Activity.Identifier,
                ActionOccurredOn = m.Action.OccurredOn,
                ActionUpdatedBy = m.Action.UpdatedBy,
                Details = JsonConvert.SerializeObject(m.Details)
            }));

            //process each type of message based on whether it is allowed or not
            //client profile - personal details
            if (
                ProcessorConfig.OutboundProcessorConfig.ActivityTypesToProcess != null 
                && ProcessorConfig.OutboundProcessorConfig.ActivityTypesToProcess.Any(a => a.Equals(OutboundProcessorActivityType.ClientProfile, StringComparison.InvariantCultureIgnoreCase))
            )
            {
                UpdateExecutionStatus(
                    ((OutboundClientProfilePersonalDetailsProcessor)serviceProvider.GetService(typeof(OutboundClientProfilePersonalDetailsProcessor))).Execute(
                        messages.Where(
                            a => 
                            a.Activity != null 
                            && a.Activity.Type.Equals(OutboundProcessorActivityType.ClientProfile, StringComparison.InvariantCultureIgnoreCase)
                            && a.Action.Reason.StartsWith(OutboundProcessorClientProfileActivitySubType.PersonalDetails, StringComparison.InvariantCultureIgnoreCase)
                        )
                    )
                );
            }

            //client profile - email
            if (
                ProcessorConfig.OutboundProcessorConfig.ActivityTypesToProcess != null
                && ProcessorConfig.OutboundProcessorConfig.ActivityTypesToProcess.Any(a => a.Equals(OutboundProcessorActivityType.ClientProfile, StringComparison.InvariantCultureIgnoreCase))
            )
            {
                UpdateExecutionStatus(
                    ((OutboundClientProfileEmailProcessor)serviceProvider.GetService(typeof(OutboundClientProfileEmailProcessor))).Execute(
                        messages.Where(
                            a =>
                            a.Activity != null
                            && a.Activity.Type.Equals(OutboundProcessorActivityType.ClientProfile, StringComparison.InvariantCultureIgnoreCase)
                            && a.Action.Reason.StartsWith(OutboundProcessorClientProfileActivitySubType.Email, StringComparison.InvariantCultureIgnoreCase)
                        )
                    )
                );
            }

            //client profile - address
            if (
                ProcessorConfig.OutboundProcessorConfig.ActivityTypesToProcess != null
                && ProcessorConfig.OutboundProcessorConfig.ActivityTypesToProcess.Any(a => a.Equals(OutboundProcessorActivityType.ClientProfile, StringComparison.InvariantCultureIgnoreCase))
            )
            {
                UpdateExecutionStatus(
                    ((OutboundClientProfileAddressProcessor)serviceProvider.GetService(typeof(OutboundClientProfileAddressProcessor))).Execute(
                        messages.Where(
                            a =>
                            a.Activity != null
                            && a.Activity.Type.Equals(OutboundProcessorActivityType.ClientProfile, StringComparison.InvariantCultureIgnoreCase)
                            && a.Action.Reason.StartsWith(OutboundProcessorClientProfileActivitySubType.Address, StringComparison.InvariantCultureIgnoreCase)
                        )
                    )
                );
            }

            //client profile - contact
            if (
                ProcessorConfig.OutboundProcessorConfig.ActivityTypesToProcess != null
                && ProcessorConfig.OutboundProcessorConfig.ActivityTypesToProcess.Any(a => a.Equals(OutboundProcessorActivityType.ClientProfile, StringComparison.InvariantCultureIgnoreCase))
            )
            {
                UpdateExecutionStatus(
                    ((OutboundClientProfileContactProcessor)serviceProvider.GetService(typeof(OutboundClientProfileContactProcessor))).Execute(
                        messages.Where(
                            a =>
                            a.Activity != null
                            && a.Activity.Type.Equals(OutboundProcessorActivityType.ClientProfile, StringComparison.InvariantCultureIgnoreCase)
                            && a.Action.Reason.StartsWith(OutboundProcessorClientProfileActivitySubType.Contact, StringComparison.InvariantCultureIgnoreCase)
                        )
                    )
                );
            }

            //client profile - vehicle
            if (
                ProcessorConfig.OutboundProcessorConfig.ActivityTypesToProcess != null
                && ProcessorConfig.OutboundProcessorConfig.ActivityTypesToProcess.Any(a => a.Equals(OutboundProcessorActivityType.ClientProfile, StringComparison.InvariantCultureIgnoreCase))
            )
            {
                UpdateExecutionStatus(
                    ((OutboundClientProfileVehicleProcessor)serviceProvider.GetService(typeof(OutboundClientProfileVehicleProcessor))).Execute(
                        messages.Where(
                            a =>
                            a.Activity != null
                            && a.Activity.Type.Equals(OutboundProcessorActivityType.ClientProfile, StringComparison.InvariantCultureIgnoreCase)
                            && a.Action.Reason.StartsWith(OutboundProcessorClientProfileActivitySubType.Vehicle, StringComparison.InvariantCultureIgnoreCase)
                        )
                    )
                );
            }

            //client profile - employment
            if (
                ProcessorConfig.OutboundProcessorConfig.ActivityTypesToProcess != null
                && ProcessorConfig.OutboundProcessorConfig.ActivityTypesToProcess.Any(a => a.Equals(OutboundProcessorActivityType.ClientProfile, StringComparison.InvariantCultureIgnoreCase))
            )
            {
                UpdateExecutionStatus(
                    ((OutboundClientProfileEmploymentProcessor)serviceProvider.GetService(typeof(OutboundClientProfileEmploymentProcessor))).Execute(
                        messages.Where(
                            a =>
                            a.Activity != null
                            && a.Activity.Type.Equals(OutboundProcessorActivityType.ClientProfile, StringComparison.InvariantCultureIgnoreCase)
                            && a.Action.Reason.StartsWith(OutboundProcessorClientProfileActivitySubType.Employment, StringComparison.InvariantCultureIgnoreCase)
                        )
                    )
                );
            }

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

            //log info message for end of processing
            Logger.LogInfo(new LogRequest
            {
                OperationName = this.GetType().Name,
                MethodName = "Execute",
                Message = "Outbound Processor execution completed.",
                CustomParams = JsonConvert.SerializeObject(ProcessorExecutionStatus)
            });

            return TaskExecutionStatuses;
        }
    }
}
