using CMI.Automon.Interface;
using CMI.Automon.Model;
using CMI.Common.Logging;
using CMI.Common.Notification;
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

        public override TaskExecutionStatus Execute(DateTime? lastExecutionDateTime, IEnumerable<string> officerLogonsToFilter)
        {
            Logger.LogInfo(new LogRequest
            {
                OperationName = this.GetType().Name,
                MethodName = "Execute",
                Message = "Address processing initiated."
            });

            //load required lookup data
            LoadLookupData();

            IEnumerable<OffenderAddress> allOffenderAddresses = null;
            TaskExecutionStatus taskExecutionStatus = new TaskExecutionStatus { ProcessorType = Common.Notification.ProcessorType.Inbound, TaskName = "Process Addresses" };

            try
            {
                //retrieve data from Automon for processing
                allOffenderAddresses = offenderAddressService.GetAllOffenderAddresses(ProcessorConfig.CmiDbConnString, lastExecutionDateTime, GetOfficerLogonToFilterDataTable(officerLogonsToFilter));

                //check if there are any records to process
                if (allOffenderAddresses.Any())
                {
                    //check if there are any records to process
                    Logger.LogDebug(new LogRequest
                    {
                        OperationName = this.GetType().Name,
                        MethodName = "Execute",
                        Message = "Offender Address records received from Automon.",
                        CustomParams = allOffenderAddresses.Count().ToString()
                    });

                    //retrieve distinct list of offender pin
                    List<string> distinctOffenderPin = allOffenderAddresses.Select(p => FormatId(p.Pin)).Distinct().ToList();

                    //iterate through each of offender pin
                    foreach(string currentOffenderPin in distinctOffenderPin)
                    {
                        //check if client exist on Nexus side for current offender pin. This is to avoid validation errors from Nexus API in further calls.
                        if (ClientService.GetClientDetails(currentOffenderPin) != null)
                        {
                            //get all addresses for given offender pin
                            var allExistingAddressDetails = addressService.GetAllAddressDetails(currentOffenderPin);

                            if (allExistingAddressDetails != null && allExistingAddressDetails.Any())
                            {
                                //set ClientId value
                                allExistingAddressDetails.ForEach(ea => ea.ClientId = currentOffenderPin);
                            }

                            //iterate through each of offender address details for current offender pin
                            foreach (var offenderAddressDetails in allOffenderAddresses.Where(a => a.Pin.Equals(currentOffenderPin, StringComparison.InvariantCultureIgnoreCase)))
                            {
                                taskExecutionStatus.AutomonReceivedRecordCount++;

                                Address address = null;
                                try
                                {
                                    //transform offender address details in Nexus compliant model
                                    address = new Address()
                                    {
                                        ClientId = FormatId(offenderAddressDetails.Pin),
                                        AddressId = string.Format("{0}-{1}", FormatId(offenderAddressDetails.Pin), offenderAddressDetails.Id),
                                        AddressType = MapAddressType(offenderAddressDetails.AddressType),
                                        FullAddress = MapFullAddress(offenderAddressDetails.Line1, offenderAddressDetails.Line2, offenderAddressDetails.City, offenderAddressDetails.State, offenderAddressDetails.Zip),
                                        Comment = FormatComment(offenderAddressDetails.Comment),
                                        IsPrimary = offenderAddressDetails.IsPrimary,
                                        IsActive = offenderAddressDetails.IsActive
                                    };

                                    //get crud action type based on comparison
                                    switch(GetCrudActionType(address, allExistingAddressDetails))
                                    {
                                        case CrudActionType.Add:
                                            if (addressService.AddNewAddressDetails(address))
                                            {
                                                taskExecutionStatus.NexusAddRecordCount++;

                                                Logger.LogDebug(new LogRequest
                                                {
                                                    OperationName = this.GetType().Name,
                                                    MethodName = "Execute",
                                                    Message = "New Client Address details added successfully.",
                                                    AutomonData = JsonConvert.SerializeObject(offenderAddressDetails),
                                                    NexusData = JsonConvert.SerializeObject(address)
                                                });
                                            }
                                            break;
                                        case CrudActionType.Update:
                                            if (addressService.UpdateAddressDetails(address))
                                            {
                                                taskExecutionStatus.NexusUpdateRecordCount++;

                                                Logger.LogDebug(new LogRequest
                                                {
                                                    OperationName = this.GetType().Name,
                                                    MethodName = "Execute",
                                                    Message = "Existing Client Address details updated successfully.",
                                                    AutomonData = JsonConvert.SerializeObject(offenderAddressDetails),
                                                    NexusData = JsonConvert.SerializeObject(address)
                                                });
                                            }
                                            break;
                                        case CrudActionType.Delete:
                                            if (addressService.DeleteAddressDetails(address.ClientId, address.AddressId))
                                            {
                                                taskExecutionStatus.NexusDeleteRecordCount++;

                                                Logger.LogDebug(new LogRequest
                                                {
                                                    OperationName = this.GetType().Name,
                                                    MethodName = "Execute",
                                                    Message = "Existing Client Address details deleted successfully.",
                                                    AutomonData = JsonConvert.SerializeObject(offenderAddressDetails),
                                                    NexusData = JsonConvert.SerializeObject(address)
                                                });
                                            }
                                            break;
                                        default:
                                            taskExecutionStatus.AutomonReceivedRecordCount--;
                                            break;
                                    }
                                }
                                catch (CmiException ce)
                                {
                                    taskExecutionStatus.NexusFailureRecordCount++;

                                    Logger.LogWarning(new LogRequest
                                    {
                                        OperationName = this.GetType().Name,
                                        MethodName = "Execute",
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
                                        OperationName = this.GetType().Name,
                                        MethodName = "Execute",
                                        Message = "Error occurred while processing a Client Address.",
                                        Exception = ex,
                                        AutomonData = JsonConvert.SerializeObject(offenderAddressDetails),
                                        NexusData = JsonConvert.SerializeObject(address)
                                    });
                                }
                            }
                        }
                    }
                }

                taskExecutionStatus.IsSuccessful = taskExecutionStatus.NexusFailureRecordCount == 0;
            }
            catch (Exception ex)
            {
                taskExecutionStatus.IsSuccessful = false;

                Logger.LogError(new LogRequest
                {
                    OperationName = this.GetType().Name,
                    MethodName = "Execute",
                    Message = "Error occurred while processing Addresses.",
                    Exception = ex,
                    AutomonData = JsonConvert.SerializeObject(allOffenderAddresses)
                });
            }

            Logger.LogInfo(new LogRequest
            {
                OperationName = this.GetType().Name,
                MethodName = "Execute",
                Message = "Addresses processing completed.",
                CustomParams = JsonConvert.SerializeObject(taskExecutionStatus)
            });

            return taskExecutionStatus;
        }

        protected override void LoadLookupData()
        {
            //load AddressTypes lookup data
            try
            {
                if (LookupService.AddressTypes != null)
                {
                    Logger.LogDebug(new LogRequest
                    {
                        OperationName = this.GetType().Name,
                        MethodName = "LoadLookupData",
                        Message = "Successfully retrieved AddressTypes from lookup",
                        CustomParams = JsonConvert.SerializeObject(LookupService.AddressTypes)
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(new LogRequest
                {
                    OperationName = this.GetType().Name,
                    MethodName = "LoadLookupData",
                    Message = "Error occurred while loading AddressTypes lookup data",
                    Exception = ex
                });
            }
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

        private string FormatComment(string automonComment)
        {
            if(string.IsNullOrEmpty(automonComment))
            {
                return null;
            }

            string formattedComment = automonComment.Replace(Environment.NewLine, " ").Replace("\"", @"""").Replace("+", string.Empty).Trim();

            if(formattedComment.Length > 200)
            {
                formattedComment = formattedComment.Take(200).ToString();

                Logger.LogDebug(new LogRequest
                {
                    OperationName = this.GetType().Name,
                    MethodName = "Execute",
                    Message = "Address Comment found to be greater than 200 characters.",
                    AutomonData = JsonConvert.SerializeObject(automonComment),
                    NexusData = JsonConvert.SerializeObject(formattedComment)
                });
            }

            return formattedComment;
        }

        private CrudActionType GetCrudActionType(Address address, IEnumerable<Address> addresses)
        {
            //check if list is null
            if(addresses == null)
            {
                return address.IsActive ? CrudActionType.Add : CrudActionType.None;
            }

            //try to get existing record using ClientId & AddressId
            Address existingAddress = addresses.Where(a 
                => 
                    a.ClientId.Equals(address.ClientId, StringComparison.InvariantCultureIgnoreCase) 
                    && a.AddressId.Equals(address.AddressId, StringComparison.InvariantCultureIgnoreCase)
            )
            .FirstOrDefault();

            //check if record already exists
            if(existingAddress == null)
            {
                //record does not exist.
                //check if record is active. YES = return Add action type, NO = return None action type
                if(address.IsActive)
                {
                    return CrudActionType.Add;
                }
                else
                {
                    return CrudActionType.None;
                }
            }
            else
            {
                //record already exist.
                //check if record is active. YES = compare it with existing record. NO = return Delete action type
                if(address.IsActive)
                {
                    //record is active.
                    //compare with existing record. Equal = return None action type, Not Equal = return Update action type
                    if(address.Equals(existingAddress))
                    {
                        return CrudActionType.None;
                    }
                    else
                    {
                        return CrudActionType.Update;
                    }
                }
                else
                {
                    return CrudActionType.Delete;
                }
            }
        }
    }
}
