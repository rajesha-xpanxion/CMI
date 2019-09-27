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
    public class OutboundClientProfilePictureProcessor : OutboundBaseProcessor
    {
        private readonly IOffenderProfilePictureService offenderProfilePictureService;

        public OutboundClientProfilePictureProcessor(
            IServiceProvider serviceProvider,
            IConfiguration configuration,
            IOffenderProfilePictureService offenderProfilePictureService
        )
            : base(serviceProvider, configuration)
        {
            this.offenderProfilePictureService = offenderProfilePictureService;
        }

        public override TaskExecutionStatus Execute(IEnumerable<OutboundMessageDetails> messages, DateTime messagesReceivedOn)
        {
            Logger.LogInfo(new LogRequest
            {
                OperationName = this.GetType().Name,
                MethodName = "Execute",
                Message = "Client Profile - Picture Details activity processing initiated."
            });

            TaskExecutionStatus taskExecutionStatus = new TaskExecutionStatus
            {
                ProcessorType = Common.Notification.ProcessorType.Outbound,
                TaskName = "Client Profile - Picture Details",
                IsSuccessful = true,
                NexusReceivedMessageCount = messages.Count()
            };

            try
            {
                foreach (OutboundMessageDetails message in messages)
                {
                    OffenderMugshot offenderMugshotDetails = null;
                    message.IsProcessed = true;
                    try
                    {
                        offenderMugshotDetails = (OffenderMugshot)ConvertResponseToObject<ClientProfilePictureDetailsActivityResponse>(
                            message.ClientIntegrationId,
                            message.ActivityIdentifier,
                            message.AutomonIdentifier,
                            RetrieveActivityDetails<ClientProfilePictureDetailsActivityResponse>(message.Details),
                            message.ActionUpdatedBy
                        );

                        if (
                            message.ActionReasonName.Equals(OutboundProcessorActionReason.Created, StringComparison.InvariantCultureIgnoreCase)
                            || message.ActionReasonName.Equals(OutboundProcessorActionReason.Updated, StringComparison.InvariantCultureIgnoreCase)
                        )
                        {
                            //save details to Automon and get Id
                            int automonId = offenderProfilePictureService.SaveOffenderMugshotPhoto(ProcessorConfig.CmiDbConnString, offenderMugshotDetails);

                            //check if saving details to Automon was successsful
                            if (automonId == 0)
                            {
                                throw new CmiException("Offender - Mugshot Photo could not be saved in Automon.");
                            }

                            //check if details got newly added in Automon
                            bool isDetailsAddedInAutomon = offenderMugshotDetails.Id == 0 && automonId > 0 && offenderMugshotDetails.Id != automonId;

                            offenderMugshotDetails.Id = automonId;

                            //save new identifier in message details
                            message.AutomonIdentifier = offenderMugshotDetails.Id.ToString();

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
                                    Message = "New Offender - Mugshot Photo added successfully.",
                                    AutomonData = JsonConvert.SerializeObject(offenderMugshotDetails),
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
                                    Message = "Existing Offender - Mugshot Photo updated successfully.",
                                    AutomonData = JsonConvert.SerializeObject(offenderMugshotDetails),
                                    NexusData = JsonConvert.SerializeObject(message)
                                });
                            }
                        }
                        else if (message.ActionReasonName.Equals(OutboundProcessorActionReason.Removed, StringComparison.InvariantCultureIgnoreCase))
                        {
                            offenderProfilePictureService.DeleteOffenderMugshotPhoto(ProcessorConfig.CmiDbConnString, offenderMugshotDetails);
                            taskExecutionStatus.AutomonDeleteMessageCount++;
                            Logger.LogDebug(new LogRequest
                            {
                                OperationName = this.GetType().Name,
                                MethodName = "Execute",
                                Message = "Offender - Mugshot Photo removed successfully.",
                                AutomonData = JsonConvert.SerializeObject(offenderMugshotDetails),
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
                            Message = "Error occurred while processing a Client Profile - Picture Details activity.",
                            Exception = ce,
                            AutomonData = JsonConvert.SerializeObject(offenderMugshotDetails),
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
                            Message = "Critical error occurred while processing a Client Profile - Picture Details activity.",
                            Exception = ex,
                            AutomonData = JsonConvert.SerializeObject(offenderMugshotDetails),
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
                    Message = "Critical error occurred while processing Client Profile - Picture Details activities.",
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
                Message = "Client Profile - Picture Details activity processing completed.",
                CustomParams = JsonConvert.SerializeObject(taskExecutionStatus)
            });

            return taskExecutionStatus;
        }
    }
}
