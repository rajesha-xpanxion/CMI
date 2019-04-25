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
            IEnumerable<MessageBodyResponse> allOutboundMessages = ((IMessageRetrieverService)serviceProvider.GetService(typeof(IMessageRetrieverService))).Execute().Result;

            //filter out valid outbound messages
            IEnumerable<MessageBodyResponse> validOutboundMessages = allOutboundMessages.Where(x => x.Client != null && x.Activity != null && x.Details != null && x.Action != null);

            //retrieve failed outbound messages from database to process it again
            IEnumerable<OutboundMessageDetails> failedOutboundMessages = ProcessorProvider.GetFailedOutboundMessages();

            DateTime messagesReceivedOn = DateTime.Now;

            //save details of received messages in database
            IEnumerable<OutboundMessageDetails> savedOutboundMessages = ProcessorProvider.SaveOutboundMessages(validOutboundMessages
                .Select(m => new OutboundMessageDetails
                {
                    Id = 0,
                    ActivityTypeName = m.Activity.Type,
                    ActivitySubTypeName = (
                        !string.IsNullOrEmpty(JsonConvert.SerializeObject(m.Details)) && JsonConvert.DeserializeObject<DetailsResponse>(JsonConvert.SerializeObject(m.Details)) != null
                        ? JsonConvert.DeserializeObject<DetailsResponse>(JsonConvert.SerializeObject(m.Details)).SubType
                        : string.Empty
                    ),
                    ActionReasonName = m.Action.Reason,
                    ClientIntegrationId = m.Client.IntegrationId,
                    ActivityIdentifier = m.Activity.Identifier,
                    ActionOccurredOn = m.Action.OccurredOn,
                    ActionUpdatedBy = m.Action.UpdatedBy,
                    Details = JsonConvert.SerializeObject(m.Details),
                    IsSuccessful = false,
                    RawData = JsonConvert.SerializeObject(m),
                    IsProcessed = false
                }),
                messagesReceivedOn
            );

            //combine newly saved outbound messages and failed outbound messages so that both list will be processed together
            IEnumerable<OutboundMessageDetails> toBeProcessedOutboundMessages = savedOutboundMessages.Concat(failedOutboundMessages);

            //process each type of message based on whether it is allowed or not

            //new client
            if (
                ProcessorConfig.OutboundProcessorConfig.ActivityTypesToProcess != null
                && ProcessorConfig.OutboundProcessorConfig.ActivityTypesToProcess.Any(a => a.Equals(OutboundProcessorActivityType.Client, StringComparison.InvariantCultureIgnoreCase))
            )
            {
                UpdateExecutionStatus(((OutboundNewClientProfileProcessor)serviceProvider.GetService(typeof(OutboundNewClientProfileProcessor))).Execute(
                    toBeProcessedOutboundMessages.Where(a => a.ActivityTypeName.Equals(OutboundProcessorActivityType.Client, StringComparison.InvariantCultureIgnoreCase)),
                    messagesReceivedOn
                    )
                );
            }

            //client profile
            if (
                ProcessorConfig.OutboundProcessorConfig.ActivityTypesToProcess != null
                && ProcessorConfig.OutboundProcessorConfig.ActivityTypesToProcess.Any(a => a.Equals(OutboundProcessorActivityType.ClientProfile, StringComparison.InvariantCultureIgnoreCase))
            )
            {
                //retrieve client profile messages
                var clientProfileMessages = toBeProcessedOutboundMessages
                .Where(
                    a =>
                        a.ActivityTypeName.Equals(OutboundProcessorActivityType.ClientProfile, StringComparison.InvariantCultureIgnoreCase)
                        && !string.IsNullOrEmpty(a.Details)
                        && JsonConvert.DeserializeObject<DetailsResponse>(a.Details) != null
                );

                //details
                //UpdateExecutionStatus(
                //    ((OutboundClientProfileProcessor)serviceProvider.GetService(typeof(OutboundClientProfileProcessor))).Execute(
                //        clientProfileMessages.Where(
                //            a => string.IsNullOrEmpty(a.ActivitySubTypeName)
                //        ),
                //        messagesReceivedOn
                //    )
                //);

                //personal details
                UpdateExecutionStatus(
                    ((OutboundClientProfilePersonalDetailsProcessor)serviceProvider.GetService(typeof(OutboundClientProfilePersonalDetailsProcessor))).Execute(
                        clientProfileMessages.Where(
                            a => a.ActivitySubTypeName.Equals(OutboundProcessorClientProfileActivitySubType.PersonalDetails, StringComparison.InvariantCultureIgnoreCase)
                        ),
                        messagesReceivedOn
                    )
                );

                //email
                UpdateExecutionStatus(
                    ((OutboundClientProfileEmailProcessor)serviceProvider.GetService(typeof(OutboundClientProfileEmailProcessor))).Execute(
                        clientProfileMessages.Where(
                            a => a.ActivitySubTypeName.Equals(OutboundProcessorClientProfileActivitySubType.EmailDetails, StringComparison.InvariantCultureIgnoreCase)
                        ),
                        messagesReceivedOn
                    )
                );

                //address
                UpdateExecutionStatus(
                    ((OutboundClientProfileAddressProcessor)serviceProvider.GetService(typeof(OutboundClientProfileAddressProcessor))).Execute(
                        clientProfileMessages.Where(
                            a => a.ActivitySubTypeName.Equals(OutboundProcessorClientProfileActivitySubType.AddressDetails, StringComparison.InvariantCultureIgnoreCase)
                        ),
                        messagesReceivedOn
                    )
                );

                //contact
                UpdateExecutionStatus(
                    ((OutboundClientProfileContactProcessor)serviceProvider.GetService(typeof(OutboundClientProfileContactProcessor))).Execute(
                        clientProfileMessages.Where(
                            a => a.ActivitySubTypeName.Equals(OutboundProcessorClientProfileActivitySubType.ContactDetails, StringComparison.InvariantCultureIgnoreCase)
                        ),
                        messagesReceivedOn
                    )
                );

                //vehicle
                UpdateExecutionStatus(
                    ((OutboundClientProfileVehicleProcessor)serviceProvider.GetService(typeof(OutboundClientProfileVehicleProcessor))).Execute(
                        clientProfileMessages.Where(
                            a => a.ActivitySubTypeName.Equals(OutboundProcessorClientProfileActivitySubType.VehicleDetails, StringComparison.InvariantCultureIgnoreCase)
                        ),
                        messagesReceivedOn
                    )
                );

                //employment
                UpdateExecutionStatus(
                    ((OutboundClientProfileEmploymentProcessor)serviceProvider.GetService(typeof(OutboundClientProfileEmploymentProcessor))).Execute(
                        clientProfileMessages.Where(
                            a => a.ActivitySubTypeName.Equals(OutboundProcessorClientProfileActivitySubType.EmploymentDetails, StringComparison.InvariantCultureIgnoreCase)
                        ),
                        messagesReceivedOn
                    )
                );
            }

            //general notes
            if (
                ProcessorConfig.OutboundProcessorConfig.ActivityTypesToProcess != null
                && ProcessorConfig.OutboundProcessorConfig.ActivityTypesToProcess.Any(a => a.Equals(OutboundProcessorActivityType.Note, StringComparison.InvariantCultureIgnoreCase))
            )
            {
                UpdateExecutionStatus(((OutboundClientProfileNoteProcessor)serviceProvider.GetService(typeof(OutboundClientProfileNoteProcessor))).Execute(
                    toBeProcessedOutboundMessages.Where(a => a.ActivityTypeName.Equals(OutboundProcessorActivityType.Note, StringComparison.InvariantCultureIgnoreCase)),
                    messagesReceivedOn
                    )
                );
            }

            //office visit
            if (
                ProcessorConfig.OutboundProcessorConfig.ActivityTypesToProcess != null
                && ProcessorConfig.OutboundProcessorConfig.ActivityTypesToProcess.Any(a => a.Equals(OutboundProcessorActivityType.OfficeVisit, StringComparison.InvariantCultureIgnoreCase))
            )
            {
                UpdateExecutionStatus(((OutboundClientProfileOfficeVisitProcessor)serviceProvider.GetService(typeof(OutboundClientProfileOfficeVisitProcessor))).Execute(
                    toBeProcessedOutboundMessages.Where(a => a.ActivityTypeName.Equals(OutboundProcessorActivityType.OfficeVisit, StringComparison.InvariantCultureIgnoreCase)),
                    messagesReceivedOn
                    )
                );
            }

            //drug test result
            if (
                ProcessorConfig.OutboundProcessorConfig.ActivityTypesToProcess != null
                && ProcessorConfig.OutboundProcessorConfig.ActivityTypesToProcess.Any(a => a.Equals(OutboundProcessorActivityType.DrugTestResult, StringComparison.InvariantCultureIgnoreCase))
            )
            {
                UpdateExecutionStatus(((OutboundClientProfileDrugTestResultProcessor)serviceProvider.GetService(typeof(OutboundClientProfileDrugTestResultProcessor))).Execute(
                    toBeProcessedOutboundMessages.Where(a => a.ActivityTypeName.Equals(OutboundProcessorActivityType.DrugTestResult, StringComparison.InvariantCultureIgnoreCase)),
                    messagesReceivedOn
                    )
                );
            }

            //field visit
            if (
                ProcessorConfig.OutboundProcessorConfig.ActivityTypesToProcess != null
                && ProcessorConfig.OutboundProcessorConfig.ActivityTypesToProcess.Any(a => a.Equals(OutboundProcessorActivityType.FieldVisit, StringComparison.InvariantCultureIgnoreCase))
            )
            {
                UpdateExecutionStatus(((OutboundClientProfileFieldVisitProcessor)serviceProvider.GetService(typeof(OutboundClientProfileFieldVisitProcessor))).Execute(
                    toBeProcessedOutboundMessages.Where(a => a.ActivityTypeName.Equals(OutboundProcessorActivityType.FieldVisit, StringComparison.InvariantCultureIgnoreCase)),
                    messagesReceivedOn
                    )
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
