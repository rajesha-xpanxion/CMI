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
    public class OutboundClientProfilePersonalDetailsProcessor : OutboundBaseProcessor
    {
        private readonly IOffenderPersonalDetailsService offenderPersonalDetailsService;
        private readonly ICommonService commonService;

        public OutboundClientProfilePersonalDetailsProcessor(
            IServiceProvider serviceProvider,
            IConfiguration configuration,
            IOffenderPersonalDetailsService offenderPersonalDetailsService,
            ICommonService commonService
        )
            : base(serviceProvider, configuration)
        {
            this.offenderPersonalDetailsService = offenderPersonalDetailsService;
            this.commonService = commonService;
        }

        public override TaskExecutionStatus Execute(IEnumerable<OutboundMessageDetails> messages, DateTime messagesReceivedOn)
        {
            Logger.LogInfo(new LogRequest
            {
                OperationName = this.GetType().Name,
                MethodName = "Execute",
                Message = "Client Profile - Personal Details activity processing initiated."
            });

            TaskExecutionStatus taskExecutionStatus = new TaskExecutionStatus
            {
                ProcessorType = Common.Notification.ProcessorType.Outbound,
                TaskName = "Client Profile - Personal Details",
                IsSuccessful = true,
                NexusReceivedMessageCount = messages.Count()
            };

            try
            {
                foreach (OutboundMessageDetails message in messages)
                {
                    Offender offenderPersonalDetails = null;
                    message.IsProcessed = true;
                    try
                    {
                        offenderPersonalDetails = ConvertResponseToObject<ClientProfilePersonalDetailsActivityResponse>(
                            message.ClientIntegrationId,
                            message.ActivityIdentifier,
                            RetrieveActivityDetails<ClientProfilePersonalDetailsActivityResponse>(message.Details),
                            message.ActionUpdatedBy
                        );

                        offenderPersonalDetails.Id = offenderPersonalDetailsService.SaveOffenderPersonalDetails(ProcessorConfig.CmiDbConnString, offenderPersonalDetails);

                        //check if saving details to Automon was successsful
                        if (offenderPersonalDetails.Id == 0)
                        {
                            throw new CmiException("Offender - Personal details could not be saved in Automon.");
                        }

                        //derive current integration id & new integration id & flag whether integration id has been changed or not
                        string currentIntegrationId = message.ActivityIdentifier, newIntegrationId = string.Format("{0}-{1}", offenderPersonalDetails.Pin, offenderPersonalDetails.Id.ToString());
                        bool isIntegrationIdUpdated = !currentIntegrationId.Equals(newIntegrationId, StringComparison.InvariantCultureIgnoreCase);

                        //update integration identifier in Nexus if it is updated
                        if (isIntegrationIdUpdated)
                        {
                            commonService.UpdateId(offenderPersonalDetails.Pin, new ReplaceIntegrationIdDetails { ElementType = "PersonalDetails", CurrentIntegrationId = currentIntegrationId, NewIntegrationId = newIntegrationId });
                        }

                        //mark this message as successful
                        message.IsSuccessful = true;

                        //save new identifier in message details
                        message.AutomonIdentifier = offenderPersonalDetails.Id.ToString();

                        //check if it was add or update operation and update Automon message counter accordingly
                        if (isIntegrationIdUpdated)
                        {
                            taskExecutionStatus.AutomonAddMessageCount++;
                            Logger.LogDebug(new LogRequest
                            {
                                OperationName = this.GetType().Name,
                                MethodName = "Execute",
                                Message = "New Offender - Personal details added successfully.",
                                AutomonData = JsonConvert.SerializeObject(offenderPersonalDetails),
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
                                Message = "Existing Offender - Personal details updated successfully.",
                                AutomonData = JsonConvert.SerializeObject(offenderPersonalDetails),
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
                            Message = "Error occurred while processing a Client Profile - Personal Details activity.",
                            Exception = ce,
                            AutomonData = JsonConvert.SerializeObject(offenderPersonalDetails),
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
                            Message = "Critical error occurred while processing a Client Profile - Personal Details activity.",
                            Exception = ex,
                            AutomonData = JsonConvert.SerializeObject(offenderPersonalDetails),
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
                    Message = "Critical error occurred while processing Client Profile - Personal Details activities.",
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
                Message = "Client Profile - Personal Details activity processing completed.",
                CustomParams = JsonConvert.SerializeObject(taskExecutionStatus)
            });

            return taskExecutionStatus;
        }
    }
}
