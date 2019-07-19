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
    public class OutboundClientProfileContactProcessor : OutboundBaseProcessor
    {
        private readonly IOffenderEmailService offenderEmailService;
        private readonly IOffenderPhoneService offenderPhoneService;
        private readonly ICommonService commonService;

        public OutboundClientProfileContactProcessor(
            IServiceProvider serviceProvider,
            IConfiguration configuration,
            IOffenderEmailService offenderEmailService,
            IOffenderPhoneService offenderPhoneService,
            ICommonService commonService
        )
            : base(serviceProvider, configuration)
        {
            this.offenderEmailService = offenderEmailService;
            this.offenderPhoneService = offenderPhoneService;
            this.commonService = commonService;
        }

        public override TaskExecutionStatus Execute(IEnumerable<OutboundMessageDetails> messages, DateTime messagesReceivedOn)
        {
            Logger.LogInfo(new LogRequest
            {
                OperationName = this.GetType().Name,
                MethodName = "Execute",
                Message = "Client Profile - Contact Details activity processing initiated."
            });

            TaskExecutionStatus taskExecutionStatus = new TaskExecutionStatus
            {
                ProcessorType = Common.Notification.ProcessorType.Outbound,
                TaskName = "Client Profile - Contact Details",
                IsSuccessful = true,
                NexusReceivedMessageCount = messages.Count()
            };

            try
            {
                foreach (OutboundMessageDetails message in messages)
                {
                    Offender offenderContactDetails = null;
                    OffenderPhone offenderPhoneDetails = null;
                    OffenderEmail offenderEmailDetails = null;
                    message.IsProcessed = true;
                    try
                    {
                        offenderContactDetails = (Offender)ConvertResponseToObject<ClientProfileContactDetailsActivityResponse>(
                            message.ClientIntegrationId,
                            message.ActivityIdentifier,
                            message.AutomonIdentifier,
                            RetrieveActivityDetails<ClientProfileContactDetailsActivityResponse>(message.Details),
                            message.ActionUpdatedBy
                        );

                        string currentIntegrationId = string.Empty, newIntegrationId = string.Empty;
                        bool isIntegrationIdUpdated = false;

                        if (offenderContactDetails.GetType() == typeof(OffenderEmail))
                        {
                            offenderEmailDetails = (OffenderEmail)offenderContactDetails;
                            offenderEmailDetails.Id = offenderEmailService.SaveOffenderEmailDetails(ProcessorConfig.CmiDbConnString, offenderEmailDetails);

                            //check if saving details to Automon was successsful
                            if (offenderEmailDetails.Id == 0)
                            {
                                throw new CmiException("Offender - Email details could not be saved in Automon.");
                            }

                            //derive current integration id & new integration id & flag whether integration id has been changed or not
                            currentIntegrationId = message.ActivityIdentifier;
                            newIntegrationId = string.Format("{0}-{1}", offenderEmailDetails.Pin, offenderEmailDetails.Id.ToString());
                            isIntegrationIdUpdated = !currentIntegrationId.Equals(newIntegrationId, StringComparison.InvariantCultureIgnoreCase);

                            //save new identifier in message details
                            message.AutomonIdentifier = offenderEmailDetails.Id.ToString();

                            //check if it was add or update operation and update Automon message counter accordingly
                            if (isIntegrationIdUpdated)
                            {
                                taskExecutionStatus.AutomonAddMessageCount++;
                                Logger.LogDebug(new LogRequest
                                {
                                    OperationName = this.GetType().Name,
                                    MethodName = "Execute",
                                    Message = "New Offender - Email details added successfully.",
                                    AutomonData = JsonConvert.SerializeObject(offenderEmailDetails),
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
                                    Message = "Existing Offender - Email details updated successfully.",
                                    AutomonData = JsonConvert.SerializeObject(offenderEmailDetails),
                                    NexusData = JsonConvert.SerializeObject(message)
                                });
                            }
                        }
                        else
                        {
                            offenderPhoneDetails = (OffenderPhone)offenderContactDetails;
                            offenderPhoneDetails.Id = offenderPhoneService.SaveOffenderPhoneDetails(ProcessorConfig.CmiDbConnString, offenderPhoneDetails);

                            //check if saving details to Automon was successsful
                            if (offenderPhoneDetails.Id == 0)
                            {
                                throw new CmiException("Offender - Phone details could not be saved in Automon.");
                            }

                            //derive current integration id & new integration id & flag whether integration id has been changed or not
                            currentIntegrationId = message.ActivityIdentifier;
                            newIntegrationId = string.Format("{0}-{1}", offenderPhoneDetails.Pin, offenderPhoneDetails.Id.ToString());
                            isIntegrationIdUpdated = !currentIntegrationId.Equals(newIntegrationId, StringComparison.InvariantCultureIgnoreCase);

                            //save new identifier in message details
                            message.AutomonIdentifier = offenderEmailDetails.Id.ToString();

                            //check if it was add or update operation and update Automon message counter accordingly
                            if (isIntegrationIdUpdated)
                            {
                                taskExecutionStatus.AutomonAddMessageCount++;
                                Logger.LogDebug(new LogRequest
                                {
                                    OperationName = this.GetType().Name,
                                    MethodName = "Execute",
                                    Message = "New Offender - Phone details added successfully.",
                                    AutomonData = JsonConvert.SerializeObject(offenderPhoneDetails),
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
                                    Message = "Existing Offender - Phone details updated successfully.",
                                    AutomonData = JsonConvert.SerializeObject(offenderPhoneDetails),
                                    NexusData = JsonConvert.SerializeObject(message)
                                });
                            }
                        }

                        //update integration identifier in Nexus if it is updated
                        if (isIntegrationIdUpdated)
                        {
                            commonService.UpdateId(offenderEmailDetails.Pin, new ReplaceIntegrationIdDetails { ElementType = DataElementType.Contact, CurrentIntegrationId = currentIntegrationId, NewIntegrationId = newIntegrationId });
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
                            Message = "Error occurred while processing a Client Profile - Contact Details activity.",
                            Exception = ce,
                            AutomonData = JsonConvert.SerializeObject(offenderContactDetails),
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
                            Message = "Critical error occurred while processing a Client Profile - Contact Details activity.",
                            Exception = ex,
                            AutomonData = JsonConvert.SerializeObject(offenderContactDetails),
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
                    Message = "Critical error occurred while processing Client Profile - Contact Details activities.",
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
                Message = "Client Profile - Contact Details activity processing completed.",
                CustomParams = JsonConvert.SerializeObject(taskExecutionStatus)
            });

            return taskExecutionStatus;
        }
    }
}
