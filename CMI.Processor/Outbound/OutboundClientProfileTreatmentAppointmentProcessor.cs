using CMI.Automon.Interface;
using CMI.Automon.Model;
using CMI.Common.Logging;
using CMI.Common.Notification;
using CMI.MessageRetriever.Model;
using CMI.Nexus.Model;
using CMI.Processor.DAL;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CMI.Processor
{
    public class OutboundClientProfileTreatmentAppointmentProcessor : OutboundBaseProcessor
    {
        private readonly IOffenderTreatmentAppointmentService offenderTreatmentAppointmentService;

        public OutboundClientProfileTreatmentAppointmentProcessor(
            IServiceProvider serviceProvider,
            IConfiguration configuration,
            IOffenderTreatmentAppointmentService offenderTreatmentAppointmentService
        )
            : base(serviceProvider, configuration)
        {
            this.offenderTreatmentAppointmentService = offenderTreatmentAppointmentService;
        }

        public override TaskExecutionStatus Execute(IEnumerable<OutboundMessageDetails> messages, DateTime messagesReceivedOn)
        {
            Logger.LogInfo(new LogRequest
            {
                OperationName = this.GetType().Name,
                MethodName = "Execute",
                Message = "Treatment Appointment activity processing initiated."
            });

            TaskExecutionStatus taskExecutionStatus = new TaskExecutionStatus
            {
                ProcessorType = Common.Notification.ProcessorType.Outbound,
                TaskName = "Treatment Appointment",
                IsSuccessful = true,
                NexusReceivedMessageCount = messages.Count()
            };

            try
            {
                foreach (OutboundMessageDetails message in messages)
                {
                    OffenderTreatmentAppointment offenderTreatmentAppointmentDetails = null;
                    message.IsProcessed = true;
                    try
                    {
                        offenderTreatmentAppointmentDetails = (OffenderTreatmentAppointment)ConvertResponseToObject<ClientProfileTreatmentAppointmentDetailsActivityResponse>(
                            message.ClientIntegrationId,
                            RetrieveActivityDetails<ClientProfileTreatmentAppointmentDetailsActivityResponse>(message.Details),
                            message.ActionUpdatedBy
                        );

                        offenderTreatmentAppointmentService.SaveOffenderTreatmentAppointmentDetails(ProcessorConfig.CmiDbConnString, offenderTreatmentAppointmentDetails);

                        taskExecutionStatus.AutomonAddMessageCount++;
                        message.IsSuccessful = true;

                        Logger.LogDebug(new LogRequest
                        {
                            OperationName = this.GetType().Name,
                            MethodName = "Execute",
                            Message = "New Offender - Treatment Appointment Details added successfully.",
                            AutomonData = JsonConvert.SerializeObject(offenderTreatmentAppointmentDetails),
                            NexusData = JsonConvert.SerializeObject(message)
                        });
                    }
                    catch (CmiException ce)
                    {
                        taskExecutionStatus.AutomonFailureMessageCount++;
                        message.IsSuccessful = false;
                        message.ErrorDetails = ce.ToString();

                        Logger.LogWarning(new LogRequest
                        {
                            OperationName = this.GetType().Name,
                            MethodName = "Execute",
                            Message = "Error occurred while processing a Treatment Appointment activity.",
                            Exception = ce,
                            AutomonData = JsonConvert.SerializeObject(offenderTreatmentAppointmentDetails),
                            NexusData = JsonConvert.SerializeObject(message)
                        });
                    }
                    catch (Exception ex)
                    {
                        taskExecutionStatus.AutomonFailureMessageCount++;
                        message.IsSuccessful = false;
                        message.ErrorDetails = ex.ToString();

                        Logger.LogError(new LogRequest
                        {
                            OperationName = this.GetType().Name,
                            MethodName = "Execute",
                            Message = "Critical error occurred while processing a Treatment Appointment activity.",
                            Exception = ex,
                            AutomonData = JsonConvert.SerializeObject(offenderTreatmentAppointmentDetails),
                            NexusData = JsonConvert.SerializeObject(message)
                        });
                    }
                }

                taskExecutionStatus.IsSuccessful = taskExecutionStatus.AutomonFailureMessageCount == 0;
            }
            catch (Exception ex)
            {
                taskExecutionStatus.IsSuccessful = false;
                messages.ToList().ForEach(m => {
                    m.IsProcessed = true;
                    m.IsSuccessful = false;
                    m.ErrorDetails = ex.ToString();
                });

                Logger.LogError(new LogRequest
                {
                    OperationName = this.GetType().Name,
                    MethodName = "Execute",
                    Message = "Critical error occurred while processing Treatment Appointment activities.",
                    Exception = ex,
                    AutomonData = JsonConvert.SerializeObject(messages)
                });
            }

            //update message wise processing status
            if (messages != null && messages.Any())
            {
                ProcessorProvider.SaveOutboundMessages(messages, messagesReceivedOn);
            }

            Logger.LogInfo(new LogRequest
            {
                OperationName = this.GetType().Name,
                MethodName = "Execute",
                Message = "Treatment Appointment activity processing completed.",
                CustomParams = JsonConvert.SerializeObject(taskExecutionStatus)
            });

            return taskExecutionStatus;
        }
    }
}
