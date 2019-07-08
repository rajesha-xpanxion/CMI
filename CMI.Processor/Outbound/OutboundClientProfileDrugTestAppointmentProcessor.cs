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
    public class OutboundClientProfileDrugTestAppointmentProcessor : OutboundBaseProcessor
    {
        private readonly IOffenderDrugTestAppointmentService offenderDrugTestAppointmentService;
        private readonly ICommonService commonService;

        public OutboundClientProfileDrugTestAppointmentProcessor(
            IServiceProvider serviceProvider,
            IConfiguration configuration,
            IOffenderDrugTestAppointmentService offenderDrugTestAppointmentService,
            ICommonService commonService
        )
            : base(serviceProvider, configuration)
        {
            this.offenderDrugTestAppointmentService = offenderDrugTestAppointmentService;
            this.commonService = commonService;
        }

        public override TaskExecutionStatus Execute(IEnumerable<OutboundMessageDetails> messages, DateTime messagesReceivedOn)
        {
            Logger.LogInfo(new LogRequest
            {
                OperationName = this.GetType().Name,
                MethodName = "Execute",
                Message = "Drug Test Appointment activity processing initiated."
            });

            TaskExecutionStatus taskExecutionStatus = new TaskExecutionStatus
            {
                ProcessorType = Common.Notification.ProcessorType.Outbound,
                TaskName = "Drug Test Appointment",
                IsSuccessful = true,
                NexusReceivedMessageCount = messages.Count()
            };

            try
            {
                foreach (OutboundMessageDetails message in messages)
                {
                    OffenderDrugTestAppointment offenderDrugTestAppointmentDetails = null;
                    message.IsProcessed = true;
                    try
                    {
                        offenderDrugTestAppointmentDetails = (OffenderDrugTestAppointment)ConvertResponseToObject<ClientProfileDrugTestAppointmentDetailsActivityResponse>(
                            message.ClientIntegrationId,
                            message.ActivityIdentifier,
                            RetrieveActivityDetails<ClientProfileDrugTestAppointmentDetailsActivityResponse>(message.Details),
                            message.ActionUpdatedBy
                        );

                        offenderDrugTestAppointmentDetails.Id = offenderDrugTestAppointmentService.SaveOffenderDrugTestAppointmentDetails(ProcessorConfig.CmiDbConnString, offenderDrugTestAppointmentDetails);

                        //check if saving details to Automon was successsful
                        if (offenderDrugTestAppointmentDetails.Id == 0)
                        {
                            throw new CmiException("Offender - Drug Test Appointment details could not be saved in Automon.");
                        }

                        //derive current integration id & new integration id & flag whether integration id has been changed or not
                        string currentIntegrationId = message.ActivityIdentifier, newIntegrationId = string.Format("{0}-{1}", offenderDrugTestAppointmentDetails.Pin, offenderDrugTestAppointmentDetails.Id.ToString());
                        bool isIntegrationIdUpdated = !currentIntegrationId.Equals(newIntegrationId, StringComparison.InvariantCultureIgnoreCase);

                        //update integration identifier in Nexus if it is updated
                        if (isIntegrationIdUpdated)
                        {
                            commonService.UpdateId(offenderDrugTestAppointmentDetails.Pin, new ReplaceIntegrationIdDetails { ElementType = "DrugTestAppointment", CurrentIntegrationId = currentIntegrationId, NewIntegrationId = newIntegrationId });
                        }

                        //mark this message as successful
                        message.IsSuccessful = true;

                        //save new identifier in message details
                        message.AutomonIdentifier = offenderDrugTestAppointmentDetails.Id.ToString();

                        //check if it was add or update operation and update Automon message counter accordingly
                        if (isIntegrationIdUpdated)
                        {
                            taskExecutionStatus.AutomonAddMessageCount++;
                            Logger.LogDebug(new LogRequest
                            {
                                OperationName = this.GetType().Name,
                                MethodName = "Execute",
                                Message = "New Offender - Drug Test Appointment details added successfully.",
                                AutomonData = JsonConvert.SerializeObject(offenderDrugTestAppointmentDetails),
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
                                Message = "Existing Offender - Drug Test Appointment details updated successfully.",
                                AutomonData = JsonConvert.SerializeObject(offenderDrugTestAppointmentDetails),
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
                            Message = "Error occurred while processing a Drug Test Appointment activity.",
                            Exception = ce,
                            AutomonData = JsonConvert.SerializeObject(offenderDrugTestAppointmentDetails),
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
                            Message = "Critical error occurred while processing a Drug Test Appointment activity.",
                            Exception = ex,
                            AutomonData = JsonConvert.SerializeObject(offenderDrugTestAppointmentDetails),
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
                    Message = "Critical error occurred while processing Drug Test Appointment activities.",
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
                Message = "Drug Test Appointment activity processing completed.",
                CustomParams = JsonConvert.SerializeObject(taskExecutionStatus)
            });

            return taskExecutionStatus;
        }
    }
}
