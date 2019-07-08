﻿using CMI.Automon.Interface;
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
    public class OutboundClientProfileDrugTestResultProcessor : OutboundBaseProcessor
    {
        private readonly IOffenderDrugTestResultService offenderDrugTestResultService;
        private readonly ICommonService commonService;

        public OutboundClientProfileDrugTestResultProcessor(
            IServiceProvider serviceProvider,
            IConfiguration configuration,
            IOffenderDrugTestResultService offenderDrugTestResultService,
            ICommonService commonService
        )
            : base(serviceProvider, configuration)
        {
            this.offenderDrugTestResultService = offenderDrugTestResultService;
            this.commonService = commonService;
        }

        public override TaskExecutionStatus Execute(IEnumerable<OutboundMessageDetails> messages, DateTime messagesReceivedOn)
        {
            Logger.LogInfo(new LogRequest
            {
                OperationName = this.GetType().Name,
                MethodName = "Execute",
                Message = "Drug Test Result activity processing initiated."
            });

            TaskExecutionStatus taskExecutionStatus = new TaskExecutionStatus
            {
                ProcessorType = Common.Notification.ProcessorType.Outbound,
                TaskName = "Drug Test Result",
                IsSuccessful = true,
                NexusReceivedMessageCount = messages.Count()
            };

            try
            {
                foreach (OutboundMessageDetails message in messages)
                {
                    OffenderDrugTestResult offenderDrugTestResultDetails = null;
                    message.IsProcessed = true;
                    try
                    {
                        offenderDrugTestResultDetails = (OffenderDrugTestResult)ConvertResponseToObject<ClientProfileDrugTestResultDetailsActivityResponse>(
                            message.ClientIntegrationId,
                            message.ActivityIdentifier,
                            RetrieveActivityDetails<ClientProfileDrugTestResultDetailsActivityResponse>(message.Details),
                            message.ActionUpdatedBy
                        );

                        offenderDrugTestResultDetails.Id = offenderDrugTestResultService.SaveOffenderDrugTestResultDetails(ProcessorConfig.CmiDbConnString, offenderDrugTestResultDetails);

                        //check if saving details to Automon was successsful
                        if (offenderDrugTestResultDetails.Id == 0)
                        {
                            throw new CmiException("Offender - Drug Test Result details could not be saved in Automon.");
                        }

                        //derive current integration id & new integration id & flag whether integration id has been changed or not
                        string currentIntegrationId = message.ActivityIdentifier, newIntegrationId = string.Format("{0}-{1}", offenderDrugTestResultDetails.Pin, offenderDrugTestResultDetails.Id.ToString());
                        bool isIntegrationIdUpdated = !currentIntegrationId.Equals(newIntegrationId, StringComparison.InvariantCultureIgnoreCase);

                        //update integration identifier in Nexus if it is updated
                        if (isIntegrationIdUpdated)
                        {
                            commonService.UpdateId(offenderDrugTestResultDetails.Pin, new ReplaceIntegrationIdDetails { ElementType = "DrugTestResult", CurrentIntegrationId = currentIntegrationId, NewIntegrationId = newIntegrationId });
                        }

                        //mark this message as successful
                        message.IsSuccessful = true;

                        //save new identifier in message details
                        message.AutomonIdentifier = offenderDrugTestResultDetails.Id.ToString();

                        //check if it was add or update operation and update Automon message counter accordingly
                        if (isIntegrationIdUpdated)
                        {
                            taskExecutionStatus.AutomonAddMessageCount++;
                            Logger.LogDebug(new LogRequest
                            {
                                OperationName = this.GetType().Name,
                                MethodName = "Execute",
                                Message = "New Offender - Drug Test Result details added successfully.",
                                AutomonData = JsonConvert.SerializeObject(offenderDrugTestResultDetails),
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
                                Message = "Existing Offender - Drug Test Result details updated successfully.",
                                AutomonData = JsonConvert.SerializeObject(offenderDrugTestResultDetails),
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
                            Message = "Error occurred while processing a Drug Test Result activity.",
                            Exception = ce,
                            AutomonData = JsonConvert.SerializeObject(offenderDrugTestResultDetails),
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
                            Message = "Critical error occurred while processing a Drug Test Result activity.",
                            Exception = ex,
                            AutomonData = JsonConvert.SerializeObject(offenderDrugTestResultDetails),
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
                    Message = "Critical error occurred while processing Drug Test Result activities.",
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
                Message = "Drug Test Result activity processing completed.",
                CustomParams = JsonConvert.SerializeObject(taskExecutionStatus)
            });

            return taskExecutionStatus;
        }
    }
}
