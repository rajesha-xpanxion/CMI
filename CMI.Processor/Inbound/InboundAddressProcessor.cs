using CMI.Automon.Interface;
using CMI.Automon.Model;
using CMI.Common.Logging;
using CMI.Nexus.Interface;
using CMI.Nexus.Model;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace CMI.Processor
{
    public class InboundAddressProcessor : InboundBaseProcessor
    {
        private readonly IOffenderAddressService offenderAddressService;
        private readonly IAddressService addressService;

        public InboundAddressProcessor(
            IServiceProvider serviceProvider,
            IConfiguration configuration,
            IOffenderAddressService offenderAddressService,
            IAddressService addressService
        )
            : base(serviceProvider, configuration)
        {
            this.offenderAddressService = offenderAddressService;
            this.addressService = addressService;
        }

        public override Common.Notification.TaskExecutionStatus Execute()
        {
            Logger.LogInfo(new LogRequest
            {
                OperationName = "Processor",
                MethodName = "ProcessAddresses",
                Message = "Address processing initiated."
            });

            IEnumerable<OffenderAddress> allOffenderAddresses = null;
            Common.Notification.TaskExecutionStatus taskExecutionStatus = new Common.Notification.TaskExecutionStatus { TaskName = "Process Addresses" };

            try
            {
                allOffenderAddresses = offenderAddressService.GetAllOffenderAddresses(ProcessorConfig.CmiDbConnString, LastExecutionDateTime);

                foreach (var offenderAddressDetails in allOffenderAddresses)
                {
                    taskExecutionStatus.AutomonReceivedRecordCount++;

                    Address address = null;
                    try
                    {
                        address = new Address()
                        {
                            ClientId = FormatId(offenderAddressDetails.Pin),
                            AddressId = string.Format("{0}-{1}", FormatId(offenderAddressDetails.Pin), offenderAddressDetails.Id),
                            AddressType = MapAddressType(offenderAddressDetails.AddressType),
                            FullAddress = MapFullAddress(offenderAddressDetails.Line1, offenderAddressDetails.Line2, offenderAddressDetails.City, offenderAddressDetails.State, offenderAddressDetails.Zip),
                            IsPrimary = offenderAddressDetails.IsPrimary,
                            Comment = string.IsNullOrEmpty(offenderAddressDetails.Comment) ? offenderAddressDetails.Comment : offenderAddressDetails.Comment.Replace("/", "-"),
                            IsActive = offenderAddressDetails.IsActive
                        };

                        if (ClientService.GetClientDetails(address.ClientId) != null)
                        {
                            if (addressService.GetAddressDetails(address.ClientId, address.AddressId) == null)
                            {
                                if (address.IsActive && addressService.AddNewAddressDetails(address))
                                {
                                    taskExecutionStatus.NexusAddRecordCount++;

                                    Logger.LogDebug(new LogRequest
                                    {
                                        OperationName = "Processor",
                                        MethodName = "ProcessAddresses",
                                        Message = "New Client Address details added successfully.",
                                        AutomonData = JsonConvert.SerializeObject(offenderAddressDetails),
                                        NexusData = JsonConvert.SerializeObject(address)
                                    });
                                }
                                else
                                {
                                    taskExecutionStatus.AutomonReceivedRecordCount--;
                                }
                            }
                            else if (!address.IsActive)
                            {
                                if (addressService.DeleteAddressDetails(address.ClientId, address.AddressId))
                                {
                                    taskExecutionStatus.NexusDeleteRecordCount++;

                                    Logger.LogDebug(new LogRequest
                                    {
                                        OperationName = "Processor",
                                        MethodName = "ProcessAddresses",
                                        Message = "Existing Client Address details deleted successfully.",
                                        AutomonData = JsonConvert.SerializeObject(offenderAddressDetails),
                                        NexusData = JsonConvert.SerializeObject(address)
                                    });
                                }
                            }
                            else
                            {
                                if (addressService.UpdateAddressDetails(address))
                                {
                                    taskExecutionStatus.NexusUpdateRecordCount++;

                                    Logger.LogDebug(new LogRequest
                                    {
                                        OperationName = "Processor",
                                        MethodName = "ProcessAddresses",
                                        Message = "Existing Client Address details updated successfully.",
                                        AutomonData = JsonConvert.SerializeObject(offenderAddressDetails),
                                        NexusData = JsonConvert.SerializeObject(address)
                                    });
                                }
                            }
                        }
                        else
                        {
                            taskExecutionStatus.AutomonReceivedRecordCount--;
                        }
                    }
                    catch (CmiException ce)
                    {
                        taskExecutionStatus.NexusFailureRecordCount++;

                        Logger.LogWarning(new LogRequest
                        {
                            OperationName = "Processor",
                            MethodName = "ProcessAddresses",
                            Message = "Error occurred in API while processing a Client Address.",
                            Exception = ce,
                            AutomonData = JsonConvert.SerializeObject(offenderAddressDetails),
                            NexusData = JsonConvert.SerializeObject(address)
                        });
                    }
                    catch (Exception ex)
                    {
                        taskExecutionStatus.NexusFailureRecordCount++;

                        Logger.LogError(new LogRequest
                        {
                            OperationName = "Processor",
                            MethodName = "ProcessAddresses",
                            Message = "Error occurred while processing a Client Address.",
                            Exception = ex,
                            AutomonData = JsonConvert.SerializeObject(offenderAddressDetails),
                            NexusData = JsonConvert.SerializeObject(address)
                        });
                    }
                }

                taskExecutionStatus.IsSuccessful = taskExecutionStatus.NexusFailureRecordCount == 0;
            }
            catch (Exception ex)
            {
                taskExecutionStatus.IsSuccessful = false;

                Logger.LogError(new LogRequest
                {
                    OperationName = "Processor",
                    MethodName = "ProcessAddresses",
                    Message = "Error occurred while processing Addresses.",
                    Exception = ex,
                    AutomonData = JsonConvert.SerializeObject(allOffenderAddresses)
                });
            }

            Logger.LogInfo(new LogRequest
            {
                OperationName = "Processor",
                MethodName = "ProcessAddresses",
                Message = "Addresses processing completed.",
                CustomParams = JsonConvert.SerializeObject(taskExecutionStatus)
            });

            return taskExecutionStatus;
        }

        private string MapFullAddress(string line1, string line2, string city, string state, string zip)
        {
            string fullAddress = string.Empty;

            if (!string.IsNullOrEmpty(line1))
            {
                fullAddress += line1 + ", ";
            }

            if (!string.IsNullOrEmpty(line2))
            {
                fullAddress += line2 + ", ";
            }

            if (!string.IsNullOrEmpty(city))
            {
                fullAddress += city + ", ";
            }

            if (!string.IsNullOrEmpty(state))
            {
                fullAddress += state + ", ";
            }

            if (!string.IsNullOrEmpty(zip))
            {
                fullAddress += zip + ", ";
            }

            fullAddress = fullAddress.Trim(new char[] { ',', ' ' });

            return string.IsNullOrEmpty(fullAddress) ? fullAddress : fullAddress.Replace("/", "-");
        }

        private string MapAddressType(string automonAddressType)
        {
            string nexusAddressType = string.Empty;

            switch (automonAddressType)
            {
                case "Mailing":
                    nexusAddressType = "Shipping Address";
                    break;
                case "Residential":
                    nexusAddressType = "Home Address";
                    break;
                case "Work/Business":
                    nexusAddressType = "Work Address";
                    break;
                default:
                    nexusAddressType = "Other";
                    break;
            }

            return nexusAddressType;
        }
    }
}
