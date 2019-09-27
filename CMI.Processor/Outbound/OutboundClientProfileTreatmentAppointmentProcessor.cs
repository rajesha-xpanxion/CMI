using CMI.Automon.Interface;
using CMI.Automon.Model;
using CMI.Common.Logging;
using CMI.Common.Notification;
using CMI.MessageRetriever.Model;
using CMI.Nexus.Interface;
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
        private readonly ICommonService commonService;

        public OutboundClientProfileTreatmentAppointmentProcessor(
            IServiceProvider serviceProvider,
            IConfiguration configuration,
            IOffenderTreatmentAppointmentService offenderTreatmentAppointmentService,
            ICommonService commonService
        )
            : base(serviceProvider, configuration)
        {
            this.offenderTreatmentAppointmentService = offenderTreatmentAppointmentService;
            this.commonService = commonService;
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
                            message.ActivityIdentifier,
                            message.AutomonIdentifier,
                            RetrieveActivityDetails<ClientProfileTreatmentAppointmentDetailsActivityResponse>(message.Details),
                            message.ActionUpdatedBy
                        );

                        //save details to Automon and get Id
                        int automonId = offenderTreatmentAppointmentService.SaveOffenderTreatmentAppointmentDetails(ProcessorConfig.CmiDbConnString, offenderTreatmentAppointmentDetails);

                        //check if saving details to Automon was successsful
                        if (automonId == 0)
                        {
                            throw new CmiException("Offender - Treatment Appointment details could not be saved in Automon.");
                        }

                        //check if details got newly added in Automon
                        bool isDetailsAddedInAutomon = offenderTreatmentAppointmentDetails.Id == 0 && automonId > 0 && offenderTreatmentAppointmentDetails.Id != automonId;

                        offenderTreatmentAppointmentDetails.Id = automonId;

                        //mark this message as successful
                        message.IsSuccessful = true;

                        //save new identifier in message details
                        message.AutomonIdentifier = offenderTreatmentAppointmentDetails.Id.ToString();

                        //update automon identifier for rest of messages having same activity identifier
                        messages.Where(
                            x =>
                                string.IsNullOrEmpty(x.AutomonIdentifier)
                                && x.ActivityIdentifier.Equals(message.ActivityIdentifier, StringComparison.InvariantCultureIgnoreCase)
                        ).
                        ToList().
                        ForEach(y => y.AutomonIdentifier = message.AutomonIdentifier);

                        //check if it was add or update operation and update Automon message counter accordingly
                        if (isDetailsAddedInAutomon)
                        {
                            taskExecutionStatus.AutomonAddMessageCount++;
                            Logger.LogDebug(new LogRequest
                            {
                                OperationName = this.GetType().Name,
                                MethodName = "Execute",
                                Message = "New Offender - Treatment Appointment details added successfully.",
                                AutomonData = JsonConvert.SerializeObject(offenderTreatmentAppointmentDetails),
                                NexusData = JsonConvert.SerializeObject(message)
                            });
                        }
                        else
                        {
                            taskExecutionStatus.AutomonUpdateMessageCount++;
                            Logger.LogDebug(new LogRequest
                            {
                                OperationName = this.GetType().Name,
                                MethodName = "Execute",
                                Message = "Existing Offender - Treatment Appointment details updated successfully.",
                                AutomonData = JsonConvert.SerializeObject(offenderTreatmentAppointmentDetails),
                                NexusData = JsonConvert.SerializeObject(message)
                            });
                        }
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
                ProcessorProvider.SaveOutboundMessagesToDatabase(messages);
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
