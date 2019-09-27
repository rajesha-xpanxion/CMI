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
    public class OutboundClientProfileAddressProcessor : OutboundBaseProcessor
    {
        private readonly IOffenderAddressService offenderAddressService;
        private readonly ICommonService commonService;

        public OutboundClientProfileAddressProcessor(
            IServiceProvider serviceProvider,
            IConfiguration configuration,
            IOffenderAddressService offenderAddressService,
            ICommonService commonService
        )
            : base(serviceProvider, configuration)
        {
            this.offenderAddressService = offenderAddressService;
            this.commonService = commonService;
        }

        public override TaskExecutionStatus Execute(IEnumerable<OutboundMessageDetails> messages, DateTime messagesReceivedOn)
        {
            Logger.LogInfo(new LogRequest
            {
                OperationName = this.GetType().Name,
                MethodName = "Execute",
                Message = "Client Profile - Address Details activity processing initiated."
            });

            TaskExecutionStatus taskExecutionStatus = new TaskExecutionStatus
            {
                ProcessorType = Common.Notification.ProcessorType.Outbound,
                TaskName = "Client Profile - Address Details",
                IsSuccessful = true,
                NexusReceivedMessageCount = messages.Count()
            };

            try
            {
                foreach (OutboundMessageDetails message in messages)
                {
                    OffenderAddress offenderAddressDetails = null;
                    message.IsProcessed = true;
                    try
                    {
                        offenderAddressDetails = (OffenderAddress)ConvertResponseToObject<ClientProfileAddressDetailsActivityResponse>(
                            message.ClientIntegrationId,
                            message.ActivityIdentifier,
                            message.AutomonIdentifier,
                            RetrieveActivityDetails<ClientProfileAddressDetailsActivityResponse>(message.Details),
                            message.ActionUpdatedBy
                        );

                        offenderAddressDetails.Id = offenderAddressService.SaveOffenderAddressDetails(ProcessorConfig.CmiDbConnString, offenderAddressDetails);

                        //check if saving details to Automon was successsful
                        if (offenderAddressDetails.Id == 0)
                        {
                            throw new CmiException("Offender - Address details could not be saved in Automon.");
                        }

                        //derive current integration id & new integration id & flag whether integration id has been changed or not
                        string 
                            currentIntegrationId = string.IsNullOrEmpty(message.AutomonIdentifier) ? message.ActivityIdentifier : string.Format("{0}-{1}", offenderAddressDetails.Pin, message.AutomonIdentifier),
                            newIntegrationId = string.Format("{0}-{1}", offenderAddressDetails.Pin, offenderAddressDetails.Id.ToString());
                        bool isIntegrationIdUpdated = !currentIntegrationId.Equals(newIntegrationId, StringComparison.InvariantCultureIgnoreCase);

                        //update integration identifier in Nexus if it is updated
                        if (isIntegrationIdUpdated)
                        {
                            ReplaceIntegrationIdDetails replaceAddressIntegrationIdDetails = new ReplaceIntegrationIdDetails
                            {
                                ElementType = DataElementType.Address,
                                CurrentIntegrationId = currentIntegrationId,
                                NewIntegrationId = newIntegrationId
                            };
                            if(commonService.UpdateId(offenderAddressDetails.Pin, replaceAddressIntegrationIdDetails))
                            {
                                Logger.LogDebug(new LogRequest
                                {
                                    OperationName = this.GetType().Name,
                                    MethodName = "Execute",
                                    Message = "Client address integration Id updated successfully in Nexus.",
                                    AutomonData = JsonConvert.SerializeObject(offenderAddressDetails),
                                    NexusData = JsonConvert.SerializeObject(replaceAddressIntegrationIdDetails)
                                });
                            }
                        }

                        //save new identifier in message details
                        message.AutomonIdentifier = offenderAddressDetails.Id.ToString();

                        //update automon identifier for rest of messages having same activity identifier
                        messages.Where(
                            x =>
                                string.IsNullOrEmpty(x.AutomonIdentifier)
                                && x.ActivityIdentifier.Equals(message.ActivityIdentifier, StringComparison.InvariantCultureIgnoreCase)
                        ).
                        ToList().
                        ForEach(y => y.AutomonIdentifier = message.AutomonIdentifier);

                        //mark this message as successful
                        message.IsSuccessful = true;

                        //check if it was add or update operation and update Automon message counter accordingly
                        if (isIntegrationIdUpdated)
                        {
                            taskExecutionStatus.AutomonAddMessageCount++;
                            Logger.LogDebug(new LogRequest
                            {
                                OperationName = this.GetType().Name,
                                MethodName = "Execute",
                                Message = "New Offender - Address details added successfully.",
                                AutomonData = JsonConvert.SerializeObject(offenderAddressDetails),
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
                                Message = "Existing Offender - Address details updated successfully.",
                                AutomonData = JsonConvert.SerializeObject(offenderAddressDetails),
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
                            Message = "Error occurred while processing a Client Profile - Address Details activity.",
                            Exception = ce,
                            AutomonData = JsonConvert.SerializeObject(offenderAddressDetails),
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
                            Message = "Critical error occurred while processing a Client Profile - Address Details activity.",
                            Exception = ex,
                            AutomonData = JsonConvert.SerializeObject(offenderAddressDetails),
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
                    Message = "Critical error occurred while processing Client Profile - Address Details activities.",
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
                Message = "Client Profile - Address Details activity processing completed.",
                CustomParams = JsonConvert.SerializeObject(taskExecutionStatus)
            });

            return taskExecutionStatus;
        }
    }
}
