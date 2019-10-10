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
    public class OutboundClientProfileCAMAlertProcessor : OutboundBaseProcessor
    {
        private readonly IOffenderCAMViolationService offenderCAMViolationService;
        private readonly ICommonService commonService;

        public OutboundClientProfileCAMAlertProcessor(
            IServiceProvider serviceProvider,
            IConfiguration configuration,
            IOffenderCAMViolationService offenderCAMViolationService,
            ICommonService commonService
        )
            : base(serviceProvider, configuration)
        {
            this.offenderCAMViolationService = offenderCAMViolationService;
            this.commonService = commonService;
        }

        public override TaskExecutionStatus Execute(IEnumerable<OutboundMessageDetails> messages, DateTime messagesReceivedOn)
        {
            Logger.LogInfo(new LogRequest
            {
                OperationName = this.GetType().Name,
                MethodName = "Execute",
                Message = "CAM Alert activity processing initiated."
            });

            TaskExecutionStatus taskExecutionStatus = new TaskExecutionStatus
            {
                ProcessorType = Common.Notification.ProcessorType.Outbound,
                TaskName = "CAM Alert",
                IsSuccessful = true,
                NexusReceivedMessageCount = messages.Count()
            };

            try
            {
                foreach (OutboundMessageDetails message in messages)
                {
                    OffenderCAMViolation offenderCAMViolationDetails = null;
                    message.IsProcessed = true;
                    try
                    {
                        //transform message details into required Automon object
                        offenderCAMViolationDetails = (OffenderCAMViolation)ConvertResponseToObject<ClientProfileCAMAlertDetailsActivityResponse>(
                            message.ClientIntegrationId,
                            message.ActivityIdentifier,
                            message.AutomonIdentifier,
                            RetrieveActivityDetails<ClientProfileCAMAlertDetailsActivityResponse>(message.Details),
                            message.ActionUpdatedBy
                        );

                        //check if compliant message is received. Yes = ignore message, No = process message
                        if (!offenderCAMViolationDetails.ViolationStatus.Equals(Nexus.Service.Status.Compliant, StringComparison.InvariantCultureIgnoreCase))
                        {

                            //save details to Automon and get Id
                            int automonId = offenderCAMViolationService.SaveOffenderCAMViolationDetails(ProcessorConfig.CmiDbConnString, offenderCAMViolationDetails);

                            //check if saving details to Automon was successsful
                            if (automonId == 0)
                            {
                                throw new CmiException("Offender - CAM Violation details could not be saved in Automon.");
                            }

                            //check if details got newly added in Automon
                            bool isDetailsAddedInAutomon = offenderCAMViolationDetails.Id == 0 && automonId > 0 && offenderCAMViolationDetails.Id != automonId;

                            offenderCAMViolationDetails.Id = automonId;

                            //mark this message as successful
                            message.IsSuccessful = true;

                            //save new identifier in message details
                            message.AutomonIdentifier = offenderCAMViolationDetails.Id.ToString();

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
                                    Message = "New Offender - CAM Violation details added successfully.",
                                    AutomonData = JsonConvert.SerializeObject(offenderCAMViolationDetails),
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
                                    Message = "Existing Offender - CAM Violation details updated successfully.",
                                    AutomonData = JsonConvert.SerializeObject(offenderCAMViolationDetails),
                                    NexusData = JsonConvert.SerializeObject(message)
                                });
                            }
                        }
                        else
                        {
                            Logger.LogDebug(new LogRequest
                            {
                                OperationName = this.GetType().Name,
                                MethodName = "Execute",
                                Message = "Compliant CAM Alert message received.",
                                AutomonData = JsonConvert.SerializeObject(offenderCAMViolationDetails),
                                NexusData = JsonConvert.SerializeObject(message)
                            });
                            message.IsSuccessful = true;
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
                            Message = "Error occurred while processing a Client Profile - CAM Alert Details activity.",
                            Exception = ce,
                            AutomonData = JsonConvert.SerializeObject(offenderCAMViolationDetails),
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
                            Message = "Critical error occurred while processing a Client Profile - CAM Alert Details activity.",
                            Exception = ex,
                            AutomonData = JsonConvert.SerializeObject(offenderCAMViolationDetails),
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
                    Message = "Critical error occurred while processing Client Profile - CAM Alert Details activities.",
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
                Message = "CAM Alert activity processing completed.",
                CustomParams = JsonConvert.SerializeObject(taskExecutionStatus)
            });

            return taskExecutionStatus;
        }
    }
}
