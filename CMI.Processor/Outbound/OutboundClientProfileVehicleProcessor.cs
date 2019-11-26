using CMI.Automon.Interface;
using CMI.Automon.Model;
using CMI.Common.Logging;
using CMI.Common.Notification;
using CMI.MessageRetriever.Model;
using CMI.Nexus.Interface;
using CMI.Nexus.Model;
using CMI.Nexus.Service;
using CMI.Processor.DAL;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CMI.Processor
{
    public class OutboundClientProfileVehicleProcessor : OutboundBaseProcessor
    {
        private readonly IOffenderVehicleService offenderVehicleService;
        private readonly ICommonService commonService;

        public OutboundClientProfileVehicleProcessor(
            IServiceProvider serviceProvider,
            IConfiguration configuration,
            IOffenderVehicleService offenderVehicleService,
            ICommonService commonService
        )
            : base(serviceProvider, configuration)
        {
            this.offenderVehicleService = offenderVehicleService;
            this.commonService = commonService;
        }

        public override TaskExecutionStatus Execute(IEnumerable<OutboundMessageDetails> messages, DateTime messagesReceivedOn)
        {
            Logger.LogInfo(new LogRequest
            {
                OperationName = this.GetType().Name,
                MethodName = "Execute",
                Message = "Client Profile - Vehicle Details activity processing initiated."
            });

            TaskExecutionStatus taskExecutionStatus = new TaskExecutionStatus
            {
                ProcessorType = Common.Notification.ProcessorType.Outbound,
                TaskName = "Client Profile - Vehicle Details",
                IsSuccessful = true,
                NexusReceivedMessageCount = messages.Count()
            };

            try
            {
                if (messages.Any())
                {
                    Logger.LogDebug(new LogRequest
                    {
                        OperationName = this.GetType().Name,
                        MethodName = "Execute",
                        Message = string.Format("{0} Client Profile - Vehicle Details messages received for processing.", messages.Count()),
                        CustomParams = JsonConvert.SerializeObject(messages)
                    });
                }

                foreach (OutboundMessageDetails message in messages)
                {
                    OffenderVehicle offenderVehicleDetails = null;
                    message.IsProcessed = true;
                    try
                    {
                        offenderVehicleDetails = (OffenderVehicle)ConvertResponseToObject<ClientProfileVehicleDetailsActivityResponse>(
                            message.ClientIntegrationId,
                            message.ActivityIdentifier,
                            message.AutomonIdentifier,
                            RetrieveActivityDetails<ClientProfileVehicleDetailsActivityResponse>(message.Details),
                            message.ActionUpdatedBy
                        );

                        if (
                            message.ActionReasonName.Equals(OutboundProcessorActionReason.Created, StringComparison.InvariantCultureIgnoreCase)
                            || message.ActionReasonName.Equals(OutboundProcessorActionReason.Updated, StringComparison.InvariantCultureIgnoreCase)
                        )
                        {
                            offenderVehicleDetails.Id = offenderVehicleService.SaveOffenderVehicleDetails(ProcessorConfig.CmiDbConnString, offenderVehicleDetails);

                            //check if saving details to Automon was successsful
                            if (offenderVehicleDetails.Id == 0)
                            {
                                throw new CmiException("Offender - Vehicle details could not be saved in Automon.");
                            }

                            

                            //derive current integration id & new integration id & flag whether integration id has been changed or not
                            string currentIntegrationId = message.ActivityIdentifier, newIntegrationId = string.Format("{0}-{1}", offenderVehicleDetails.Pin, offenderVehicleDetails.Id.ToString());
                            bool isIntegrationIdUpdated = !currentIntegrationId.Equals(newIntegrationId, StringComparison.InvariantCultureIgnoreCase);

                            //update integration identifier in Nexus if it is updated
                            if (isIntegrationIdUpdated)
                            {
                                ReplaceIntegrationIdDetails replaceClientVehicleIntegrationIdDetails = new ReplaceIntegrationIdDetails
                                {
                                    ElementType = DataElementType.Vehicle,
                                    CurrentIntegrationId = currentIntegrationId,
                                    NewIntegrationId = newIntegrationId
                                };
                                if (commonService.UpdateId(offenderVehicleDetails.Pin, replaceClientVehicleIntegrationIdDetails))
                                {
                                    Logger.LogDebug(new LogRequest
                                    {
                                        OperationName = this.GetType().Name,
                                        MethodName = "Execute",
                                        Message = "Client vehicle integration Id updated successfully in Nexus.",
                                        AutomonData = JsonConvert.SerializeObject(offenderVehicleDetails),
                                        NexusData = JsonConvert.SerializeObject(replaceClientVehicleIntegrationIdDetails)
                                    });
                                }
                            }

                            //save new identifier in message details
                            message.AutomonIdentifier = offenderVehicleDetails.Id.ToString();

                            //update automon identifier for rest of messages having same activity identifier
                            messages.Where(
                                x =>
                                    string.IsNullOrEmpty(x.AutomonIdentifier)
                                    && x.ActivityIdentifier.Equals(message.ActivityIdentifier, StringComparison.InvariantCultureIgnoreCase)
                            ).
                            ToList().
                            ForEach(y => y.AutomonIdentifier = message.AutomonIdentifier);

                            //check if it was add or update operation and update Automon message counter accordingly
                            if (isIntegrationIdUpdated)
                            {
                                taskExecutionStatus.AutomonAddMessageCount++;
                                Logger.LogDebug(new LogRequest
                                {
                                    OperationName = this.GetType().Name,
                                    MethodName = "Execute",
                                    Message = "New Offender - Vehicle details added successfully.",
                                    AutomonData = JsonConvert.SerializeObject(offenderVehicleDetails),
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
                                    Message = "Existing Offender - Vehicle details updated successfully.",
                                    AutomonData = JsonConvert.SerializeObject(offenderVehicleDetails),
                                    NexusData = JsonConvert.SerializeObject(message)
                                });
                            }
                        }
                        else if(message.ActionReasonName.Equals(OutboundProcessorActionReason.Removed, StringComparison.InvariantCultureIgnoreCase))
                        {
                            offenderVehicleService.DeleteOffenderVehicleDetails(ProcessorConfig.CmiDbConnString, offenderVehicleDetails);
                            taskExecutionStatus.AutomonDeleteMessageCount++;

                            Logger.LogDebug(new LogRequest
                            {
                                OperationName = this.GetType().Name,
                                MethodName = "Execute",
                                Message = "Offender - Vehicle Details removed successfully.",
                                AutomonData = JsonConvert.SerializeObject(offenderVehicleDetails),
                                NexusData = JsonConvert.SerializeObject(message)
                            });
                        }
                        else
                        {
                            throw new CmiException("Invalid action reason found.");
                        }

                        //mark this message as successful
                        message.IsSuccessful = true;
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
                            Message = "Error occurred while processing a Client Profile - Vehicle Details activity.",
                            Exception = ce,
                            AutomonData = JsonConvert.SerializeObject(offenderVehicleDetails),
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
                            Message = "Critical error occurred while processing a Client Profile - Vehicle Details activity.",
                            Exception = ex,
                            AutomonData = JsonConvert.SerializeObject(offenderVehicleDetails),
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
                    Message = "Critical error occurred while processing Client Profile - Vehicle Details activities.",
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
                Message = "Client Profile - Vehicle Details activity processing completed.",
                CustomParams = JsonConvert.SerializeObject(taskExecutionStatus)
            });

            return taskExecutionStatus;
        }
    }
}
