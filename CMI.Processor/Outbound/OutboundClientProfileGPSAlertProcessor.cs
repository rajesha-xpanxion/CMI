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
    public class OutboundClientProfileGPSAlertProcessor : OutboundBaseProcessor
    {
        private readonly IOffenderGPSViolationService offenderGPSViolationService;
        private readonly ICommonService commonService;

        public OutboundClientProfileGPSAlertProcessor(
            IServiceProvider serviceProvider,
            IConfiguration configuration,
            IOffenderGPSViolationService offenderGPSViolationService,
            ICommonService commonService
        )
            : base(serviceProvider, configuration)
        {
            this.offenderGPSViolationService = offenderGPSViolationService;
            this.commonService = commonService;
        }

        public override TaskExecutionStatus Execute(IEnumerable<OutboundMessageDetails> messages, DateTime messagesReceivedOn)
        {
            Logger.LogInfo(new LogRequest
            {
                OperationName = this.GetType().Name,
                MethodName = "Execute",
                Message = "GPS Alert activity processing initiated."
            });

            TaskExecutionStatus taskExecutionStatus = new TaskExecutionStatus
            {
                ProcessorType = Common.Notification.ProcessorType.Outbound,
                TaskName = "GPS Alert",
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
                        Message = string.Format("{0} GPS Alert messages received for processing.", messages.Count()),
                        CustomParams = JsonConvert.SerializeObject(messages)
                    });
                }

                foreach (OutboundMessageDetails message in messages)
                {
                    OffenderGPSViolation offenderGPSViolationDetails = null;
                    message.IsProcessed = true;
                    try
                    {
                        //transform message details into required Automon object
                        offenderGPSViolationDetails = (OffenderGPSViolation)ConvertResponseToObject<ClientProfileGPSAlertDetailsActivityResponse>(
                            message.ClientIntegrationId,
                            message.ActivityIdentifier,
                            message.AutomonIdentifier,
                            RetrieveActivityDetails<ClientProfileGPSAlertDetailsActivityResponse>(message.Details),
                            message.ActionUpdatedBy
                        );

                        //check if compliant message is received. Yes = ignore message, No = process message
                        if (!offenderGPSViolationDetails.ViolationStatus.Equals(Nexus.Service.Status.Compliant, StringComparison.InvariantCultureIgnoreCase))
                        {

                            //save details to Automon and get Id
                            int automonId = offenderGPSViolationService.SaveOffenderGPSViolationDetails(ProcessorConfig.CmiDbConnString, offenderGPSViolationDetails);

                            //check if saving details to Automon was successsful
                            if (automonId == 0)
                            {
                                throw new CmiException("Offender - GPS Violation details could not be saved in Automon.");
                            }

                            //check if details got newly added in Automon
                            bool isDetailsAddedInAutomon = offenderGPSViolationDetails.Id == 0 && automonId > 0 && offenderGPSViolationDetails.Id != automonId;

                            offenderGPSViolationDetails.Id = automonId;

                            //mark this message as successful
                            message.IsSuccessful = true;

                            //save new identifier in message details
                            message.AutomonIdentifier = offenderGPSViolationDetails.Id.ToString();

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
                                    Message = "New Offender - GPS Violation details added successfully.",
                                    AutomonData = JsonConvert.SerializeObject(offenderGPSViolationDetails),
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
                                    Message = "Existing Offender - GPS Violation details updated successfully.",
                                    AutomonData = JsonConvert.SerializeObject(offenderGPSViolationDetails),
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
                                Message = "Compliant GPS Alert message received.",
                                AutomonData = JsonConvert.SerializeObject(offenderGPSViolationDetails),
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
                            Message = "Error occurred while processing a Client Profile - GPS Alert Details activity.",
                            Exception = ce,
                            AutomonData = JsonConvert.SerializeObject(offenderGPSViolationDetails),
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
                            Message = "Critical error occurred while processing a Client Profile - GPS Alert Details activity.",
                            Exception = ex,
                            AutomonData = JsonConvert.SerializeObject(offenderGPSViolationDetails),
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
                    Message = "Critical error occurred while processing Client Profile - GPS Alert Details activities.",
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
                Message = "GPS Alert activity processing completed.",
                CustomParams = JsonConvert.SerializeObject(taskExecutionStatus)
            });

            return taskExecutionStatus;
        }
    }
}
