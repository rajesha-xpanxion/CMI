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
    public class OutboundClientProfileEmploymentProcessor : OutboundBaseProcessor
    {
        private readonly IOffenderEmploymentService offenderEmploymentService;
        private readonly ICommonService commonService;

        public OutboundClientProfileEmploymentProcessor(
            IServiceProvider serviceProvider,
            IConfiguration configuration,
            IOffenderEmploymentService offenderEmploymentService,
            ICommonService commonService
        )
            : base(serviceProvider, configuration)
        {
            this.offenderEmploymentService = offenderEmploymentService;
            this.commonService = commonService;
        }

        public override TaskExecutionStatus Execute(IEnumerable<OutboundMessageDetails> messages, DateTime messagesReceivedOn)
        {
            Logger.LogInfo(new LogRequest
            {
                OperationName = this.GetType().Name,
                MethodName = "Execute",
                Message = "Client Profile - Employment Details activity processing initiated."
            });

            TaskExecutionStatus taskExecutionStatus = new TaskExecutionStatus
            {
                ProcessorType = Common.Notification.ProcessorType.Outbound,
                TaskName = "Client Profile - Employment Details",
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
                        Message = string.Format("{0} Client Profile - Employment Details messages received for processing.", messages.Count()),
                        CustomParams = JsonConvert.SerializeObject(messages)
                    });
                }

                foreach (OutboundMessageDetails message in messages)
                {
                    OffenderEmployment offenderEmploymentDetails = null;
                    message.IsProcessed = true;
                    try
                    {
                        offenderEmploymentDetails = (OffenderEmployment)ConvertResponseToObject<ClientProfileEmploymentDetailsActivityResponse>(
                            message.ClientIntegrationId,
                            message.ActivityIdentifier,
                            message.AutomonIdentifier,
                            RetrieveActivityDetails<ClientProfileEmploymentDetailsActivityResponse>(message.Details),
                            message.ActionUpdatedBy
                        );

                        if (
                            message.ActionReasonName.Equals(OutboundProcessorActionReason.Created, StringComparison.InvariantCultureIgnoreCase)
                            || message.ActionReasonName.Equals(OutboundProcessorActionReason.Updated, StringComparison.InvariantCultureIgnoreCase)
                        )
                        {
                            //save details to Automon and get Id
                            offenderEmploymentDetails.Id = offenderEmploymentService.SaveOffenderEmploymentDetails(ProcessorConfig.CmiDbConnString, offenderEmploymentDetails);

                            //check if saving details to Automon was successsful
                            if (offenderEmploymentDetails.Id == 0)
                            {
                                throw new CmiException("Offender - Employment details could not be saved in Automon.");
                            }

                            //derive current integration id & new integration id & flag whether integration id has been changed or not
                            string 
                                    currentIntegrationId = string.IsNullOrEmpty(message.AutomonIdentifier) ? message.ActivityIdentifier : string.Format("{0}-{1}", offenderEmploymentDetails.Pin, message.AutomonIdentifier),
                                    newIntegrationId = string.Format("{0}-{1}", offenderEmploymentDetails.Pin, offenderEmploymentDetails.Id.ToString());
                            bool isIntegrationIdUpdated = !currentIntegrationId.Equals(newIntegrationId, StringComparison.InvariantCultureIgnoreCase);

                            //update integration identifier in Nexus if it is updated
                            if (isIntegrationIdUpdated)
                            {
                                ReplaceIntegrationIdDetails replaceClientEmploymentIntegrationIdDetails = new ReplaceIntegrationIdDetails
                                {
                                    ElementType = DataElementType.Employer,
                                    CurrentIntegrationId = currentIntegrationId,
                                    NewIntegrationId = newIntegrationId
                                };
                                if(commonService.UpdateId(offenderEmploymentDetails.Pin, replaceClientEmploymentIntegrationIdDetails))
                                {
                                    Logger.LogDebug(new LogRequest
                                    {
                                        OperationName = this.GetType().Name,
                                        MethodName = "Execute",
                                        Message = "Client employment integration Id updated successfully in Nexus.",
                                        AutomonData = JsonConvert.SerializeObject(offenderEmploymentDetails),
                                        NexusData = JsonConvert.SerializeObject(replaceClientEmploymentIntegrationIdDetails)
                                    });
                                }
                            }

                            //save new identifier in message details
                            message.AutomonIdentifier = offenderEmploymentDetails.Id.ToString();

                            //check if it was add or update operation and update Automon message counter accordingly
                            if (isIntegrationIdUpdated)
                            {
                                taskExecutionStatus.AutomonAddMessageCount++;
                                Logger.LogDebug(new LogRequest
                                {
                                    OperationName = this.GetType().Name,
                                    MethodName = "Execute",
                                    Message = "New Offender - Employment details added successfully.",
                                    AutomonData = JsonConvert.SerializeObject(offenderEmploymentDetails),
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
                                    Message = "Existing Offender - Employment details updated successfully.",
                                    AutomonData = JsonConvert.SerializeObject(offenderEmploymentDetails),
                                    NexusData = JsonConvert.SerializeObject(message)
                                });
                            }
                        }
                        else if (message.ActionReasonName.Equals(OutboundProcessorActionReason.Removed, StringComparison.InvariantCultureIgnoreCase))
                        {
                            offenderEmploymentService.DeleteOffenderEmploymentDetails(ProcessorConfig.CmiDbConnString, offenderEmploymentDetails);
                            taskExecutionStatus.AutomonDeleteMessageCount++;
                            Logger.LogDebug(new LogRequest
                            {
                                OperationName = this.GetType().Name,
                                MethodName = "Execute",
                                Message = "Offender - Employment Details removed successfully.",
                                AutomonData = JsonConvert.SerializeObject(offenderEmploymentDetails),
                                NexusData = JsonConvert.SerializeObject(message)
                            });
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
                            Message = "Error occurred while processing a Client Profile - Employment Details activity.",
                            Exception = ce,
                            AutomonData = JsonConvert.SerializeObject(offenderEmploymentDetails),
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
                            Message = "Critical error occurred while processing a Client Profile - Employment Details activity.",
                            Exception = ex,
                            AutomonData = JsonConvert.SerializeObject(offenderEmploymentDetails),
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
                    Message = "Critical error occurred while processing Client Profile - Employment Details activities.",
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
                Message = "Client Profile - Employment Details activity processing completed.",
                CustomParams = JsonConvert.SerializeObject(taskExecutionStatus)
            });

            return taskExecutionStatus;
        }
    }
}
