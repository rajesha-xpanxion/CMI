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
    public class OutboundClientProfileTouchPointCheckInProcessor : OutboundBaseProcessor
    {
        private readonly IOffenderTouchPointCheckInService offenderTouchPointCheckInService;
        private readonly ICommonService commonService;

        public OutboundClientProfileTouchPointCheckInProcessor(
            IServiceProvider serviceProvider,
            IConfiguration configuration,
            IOffenderTouchPointCheckInService offenderTouchPointCheckInService,
            ICommonService commonService
        )
            : base(serviceProvider, configuration)
        {
            this.offenderTouchPointCheckInService = offenderTouchPointCheckInService;
            this.commonService = commonService;
        }

        public override TaskExecutionStatus Execute(IEnumerable<OutboundMessageDetails> messages, DateTime messagesReceivedOn)
        {
            Logger.LogInfo(new LogRequest
            {
                OperationName = this.GetType().Name,
                MethodName = "Execute",
                Message = "TouchPoint Check-In activity processing initiated."
            });

            TaskExecutionStatus taskExecutionStatus = new TaskExecutionStatus
            {
                ProcessorType = Common.Notification.ProcessorType.Outbound,
                TaskName = "TouchPoint Check-In",
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
                        Message = string.Format("{0} TouchPoint Check-In messages received for processing.", messages.Count()),
                        CustomParams = JsonConvert.SerializeObject(messages)
                    });
                }

                foreach (OutboundMessageDetails message in messages)
                {
                    OffenderTouchPointCheckIn offenderTouchPointCheckInDetails = null;
                    message.IsProcessed = true;
                    try
                    {
                        //transform message details into required Automon object
                        offenderTouchPointCheckInDetails = (OffenderTouchPointCheckIn)ConvertResponseToObject<ClientProfileTouchPointCheckInDetailsActivityResponse>(
                            message.ClientIntegrationId,
                            message.ActivityIdentifier,
                            message.AutomonIdentifier,
                            RetrieveActivityDetails<ClientProfileTouchPointCheckInDetailsActivityResponse>(message.Details),
                            message.ActionUpdatedBy
                        );

                        //save details to Automon and get Id
                        int automonId = offenderTouchPointCheckInService.SaveOffenderTouchPointCheckInDetails(ProcessorConfig.CmiDbConnString, offenderTouchPointCheckInDetails);

                        //check if saving details to Automon was successsful
                        if (automonId == 0)
                        {
                            throw new CmiException("Offender - TouchPoint Check-In details could not be saved in Automon.");
                        }

                        //check if details got newly added in Automon
                        bool isDetailsAddedInAutomon = offenderTouchPointCheckInDetails.Id == 0 && automonId > 0 && offenderTouchPointCheckInDetails.Id != automonId;

                        offenderTouchPointCheckInDetails.Id = automonId;

                        //mark this message as successful
                        message.IsSuccessful = true;

                        //save new identifier in message details
                        message.AutomonIdentifier = offenderTouchPointCheckInDetails.Id.ToString();

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
                                Message = "New Offender - TouchPoint Check-In details added successfully.",
                                AutomonData = JsonConvert.SerializeObject(offenderTouchPointCheckInDetails),
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
                                Message = "Existing Offender - TouchPoint Check-In details updated successfully.",
                                AutomonData = JsonConvert.SerializeObject(offenderTouchPointCheckInDetails),
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
                            Message = "Error occurred while processing a Client Profile - TouchPoint Check-In Details activity.",
                            Exception = ce,
                            AutomonData = JsonConvert.SerializeObject(offenderTouchPointCheckInDetails),
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
                            Message = "Critical error occurred while processing a Client Profile - TouchPoint Check-In Details activity.",
                            Exception = ex,
                            AutomonData = JsonConvert.SerializeObject(offenderTouchPointCheckInDetails),
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
                    Message = "Critical error occurred while processing Client Profile - TouchPoint Check-In Details activities.",
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
                Message = "TouchPoint Check-In activity processing completed.",
                CustomParams = JsonConvert.SerializeObject(taskExecutionStatus)
            });

            return taskExecutionStatus;
        }
    }
}
