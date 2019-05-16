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
        private readonly IEmailNotificationProvider emailNotificationProvider;

        public OutboundProcessor(
            IServiceProvider serviceProvider,
            IConfiguration configuration,
            IEmailNotificationProvider emailNotificationProvider
        )
            : base(serviceProvider, configuration)
        {
            this.serviceProvider = serviceProvider;
            this.configuration = configuration;
            this.emailNotificationProvider = emailNotificationProvider;

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
            IEnumerable<MessageBodyResponse> newlyReceivedOutboundMessages = ((IMessageRetrieverService)serviceProvider.GetService(typeof(IMessageRetrieverService))).Execute().Result;

            DateTime messagesReceivedOn = DateTime.Now;

            //filter out valid outbound messages
            IEnumerable<MessageBodyResponse> newlyReceivedValidOutboundMessages = newlyReceivedOutboundMessages.Where(x => x.Client != null && x.Activity != null && x.Details != null && x.Action != null);

            //convert messages in required format
            IEnumerable<OutboundMessageDetails> convertedOutboundMessages = newlyReceivedValidOutboundMessages
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
                        IsProcessed = false,
                        ReceivedOn = messagesReceivedOn
                    });

            //check if any new outbound message received and log as debug log for reference
            if (convertedOutboundMessages != null && convertedOutboundMessages.Any())
            {
                Logger.LogDebug(new LogRequest
                {
                    OperationName = this.GetType().Name,
                    MethodName = "Execute",
                    Message = "New Outbound messages received from Nexus.",
                    CustomParams = JsonConvert.SerializeObject(convertedOutboundMessages)
                });
            }

            //try to read failed messages (if any) from secondary storage (disk)
            IEnumerable<OutboundMessageDetails> outboundMessagesFromDisk = ProcessorProvider.GetOutboundMessagesFromDisk();

            //check if any failed outbound messages received from secondary storage (disk) and log as debug log for reference
            //also append in main list
            if (outboundMessagesFromDisk != null && outboundMessagesFromDisk.Any())
            {
                Logger.LogDebug(new LogRequest
                {
                    OperationName = this.GetType().Name,
                    MethodName = "Execute",
                    Message = "Failed Outbound messages read from secondary storage (disk).",
                    CustomParams = JsonConvert.SerializeObject(outboundMessagesFromDisk)
                });

                //append messages to mail list
                convertedOutboundMessages = convertedOutboundMessages.Concat(outboundMessagesFromDisk);
            }

            //continue with regular processing
            IEnumerable<OutboundMessageDetails> toBeProcessedOutboundMessages = null;

            //try to save received outbound messages in database
            try
            {
                toBeProcessedOutboundMessages = ProcessorProvider.SaveOutboundMessagesToDatabase(convertedOutboundMessages);
            }
            catch(Exception)
            {
                //save messages to disk in case of failure in saving it to database
                ProcessorProvider.SaveOutboundMessagesToDisk(convertedOutboundMessages);

                //send critical email to support persons configured
                emailNotificationProvider.SendCriticalErrorEmail(new BaseEmailRequest
                {
                    Subject = ProcessorConfig.CriticalErrorEmailSubject,
                    ToEmailAddress = ProcessorConfig.ExecutionStatusReportReceiverEmailAddresses
                });

                //application will not proceed further as there is no use of it. Hence just return from here and end execution of this processor.
                return TaskExecutionStatuses;
            }

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

                //personal details
                if (
                    ProcessorConfig.OutboundProcessorConfig.ActivitySubTypesToProcess != null
                    && ProcessorConfig.OutboundProcessorConfig.ActivitySubTypesToProcess.Any(a => a.Equals(OutboundProcessorClientProfileActivitySubType.PersonalDetails, StringComparison.InvariantCultureIgnoreCase))
                )
                {
                    UpdateExecutionStatus(
                        ((OutboundClientProfilePersonalDetailsProcessor)serviceProvider.GetService(typeof(OutboundClientProfilePersonalDetailsProcessor))).Execute(
                            clientProfileMessages.Where(
                                a => a.ActivitySubTypeName.Equals(OutboundProcessorClientProfileActivitySubType.PersonalDetails, StringComparison.InvariantCultureIgnoreCase)
                            ),
                            messagesReceivedOn
                        )
                    );
                }

                //email
                if (
                    ProcessorConfig.OutboundProcessorConfig.ActivitySubTypesToProcess != null
                    && ProcessorConfig.OutboundProcessorConfig.ActivitySubTypesToProcess.Any(a => a.Equals(OutboundProcessorClientProfileActivitySubType.EmailDetails, StringComparison.InvariantCultureIgnoreCase))
                )
                {
                    UpdateExecutionStatus(
                        ((OutboundClientProfileEmailProcessor)serviceProvider.GetService(typeof(OutboundClientProfileEmailProcessor))).Execute(
                            clientProfileMessages.Where(
                                a => a.ActivitySubTypeName.Equals(OutboundProcessorClientProfileActivitySubType.EmailDetails, StringComparison.InvariantCultureIgnoreCase)
                            ),
                            messagesReceivedOn
                        )
                    );
                }

                //address
                if (
                    ProcessorConfig.OutboundProcessorConfig.ActivitySubTypesToProcess != null
                    && ProcessorConfig.OutboundProcessorConfig.ActivitySubTypesToProcess.Any(a => a.Equals(OutboundProcessorClientProfileActivitySubType.AddressDetails, StringComparison.InvariantCultureIgnoreCase))
                )
                {
                    UpdateExecutionStatus(
                        ((OutboundClientProfileAddressProcessor)serviceProvider.GetService(typeof(OutboundClientProfileAddressProcessor))).Execute(
                            clientProfileMessages.Where(
                                a => a.ActivitySubTypeName.Equals(OutboundProcessorClientProfileActivitySubType.AddressDetails, StringComparison.InvariantCultureIgnoreCase)
                            ),
                            messagesReceivedOn
                        )
                    );
                }

                //contact
                if (
                    ProcessorConfig.OutboundProcessorConfig.ActivitySubTypesToProcess != null
                    && ProcessorConfig.OutboundProcessorConfig.ActivitySubTypesToProcess.Any(a => a.Equals(OutboundProcessorClientProfileActivitySubType.ContactDetails, StringComparison.InvariantCultureIgnoreCase))
                )
                {
                    UpdateExecutionStatus(
                        ((OutboundClientProfileContactProcessor)serviceProvider.GetService(typeof(OutboundClientProfileContactProcessor))).Execute(
                            clientProfileMessages.Where(
                                a => a.ActivitySubTypeName.Equals(OutboundProcessorClientProfileActivitySubType.ContactDetails, StringComparison.InvariantCultureIgnoreCase)
                            ),
                            messagesReceivedOn
                        )
                    );
                }

                //vehicle
                if (
                    ProcessorConfig.OutboundProcessorConfig.ActivitySubTypesToProcess != null
                    && ProcessorConfig.OutboundProcessorConfig.ActivitySubTypesToProcess.Any(a => a.Equals(OutboundProcessorClientProfileActivitySubType.VehicleDetails, StringComparison.InvariantCultureIgnoreCase))
                )
                {
                    UpdateExecutionStatus(
                        ((OutboundClientProfileVehicleProcessor)serviceProvider.GetService(typeof(OutboundClientProfileVehicleProcessor))).Execute(
                            clientProfileMessages.Where(
                                a => a.ActivitySubTypeName.Equals(OutboundProcessorClientProfileActivitySubType.VehicleDetails, StringComparison.InvariantCultureIgnoreCase)
                            ),
                            messagesReceivedOn
                        )
                    );
                }

                //employment
                if (
                    ProcessorConfig.OutboundProcessorConfig.ActivitySubTypesToProcess != null
                    && ProcessorConfig.OutboundProcessorConfig.ActivitySubTypesToProcess.Any(a => a.Equals(OutboundProcessorClientProfileActivitySubType.EmploymentDetails, StringComparison.InvariantCultureIgnoreCase))
                )
                {
                    UpdateExecutionStatus(
                        ((OutboundClientProfileEmploymentProcessor)serviceProvider.GetService(typeof(OutboundClientProfileEmploymentProcessor))).Execute(
                            clientProfileMessages.Where(
                                a => a.ActivitySubTypeName.Equals(OutboundProcessorClientProfileActivitySubType.EmploymentDetails, StringComparison.InvariantCultureIgnoreCase)
                            ),
                            messagesReceivedOn
                        )
                    );
                }
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

            //drug test appointment
            if (
                ProcessorConfig.OutboundProcessorConfig.ActivityTypesToProcess != null
                && ProcessorConfig.OutboundProcessorConfig.ActivityTypesToProcess.Any(a => a.Equals(OutboundProcessorActivityType.DrugTestAppointment, StringComparison.InvariantCultureIgnoreCase))
            )
            {
                UpdateExecutionStatus(((OutboundClientProfileDrugTestAppointmentProcessor)serviceProvider.GetService(typeof(OutboundClientProfileDrugTestAppointmentProcessor))).Execute(
                    toBeProcessedOutboundMessages.Where(a => a.ActivityTypeName.Equals(OutboundProcessorActivityType.DrugTestAppointment, StringComparison.InvariantCultureIgnoreCase)),
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

            //treatment appointment
            if (
                ProcessorConfig.OutboundProcessorConfig.ActivityTypesToProcess != null
                && ProcessorConfig.OutboundProcessorConfig.ActivityTypesToProcess.Any(a => a.Equals(OutboundProcessorActivityType.TreatmentAppointment, StringComparison.InvariantCultureIgnoreCase))
            )
            {
                UpdateExecutionStatus(((OutboundClientProfileTreatmentAppointmentProcessor)serviceProvider.GetService(typeof(OutboundClientProfileTreatmentAppointmentProcessor))).Execute(
                    toBeProcessedOutboundMessages.Where(a => a.ActivityTypeName.Equals(OutboundProcessorActivityType.TreatmentAppointment, StringComparison.InvariantCultureIgnoreCase)),
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
