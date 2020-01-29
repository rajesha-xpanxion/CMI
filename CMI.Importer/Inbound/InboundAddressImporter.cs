using CMI.Common.Logging;
using CMI.Importer.DAL;
using CMI.Nexus.Interface;
using CMI.Nexus.Model;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CMI.Importer
{
    public class InboundAddressImporter : InboundBaseImporter
    {
        private readonly IAddressService addressService;

        public InboundAddressImporter(
            IServiceProvider serviceProvider,
            IConfiguration configuration,
            IAddressService addressService
        )
            : base(serviceProvider, configuration)
        {
            this.addressService = addressService;
        }

        public override void Execute()
        {
            Logger.LogInfo(new LogRequest
            {
                OperationName = this.GetType().Name,
                MethodName = "Execute",
                Message = "Address import initiated."
            });

            IEnumerable<AddressDetails> retrievedAddresses = null;

            try
            {
                DateTime startDateTime = DateTime.Now;
                retrievedAddresses = ImporterProvider.RetrieveAddressDataFromExcel();
                DateTime endDateTime = DateTime.Now;

                TimeSpan timeSpan = endDateTime - startDateTime;

                if (retrievedAddresses != null && retrievedAddresses.Any())
                {
                        Logger.LogDebug(new LogRequest
                        {
                            OperationName = this.GetType().Name,
                            MethodName = "Execute",
                            Message = string.Format("#{0} Address records retrieved in {1} seconds.", retrievedAddresses.Count(), timeSpan.TotalSeconds)
                        });

                    var toBeProcessedAddresses = ImporterProvider.SaveAddressesToDatabase(retrievedAddresses);

                    foreach (var addressDetails in toBeProcessedAddresses.Where(x => x.IsImportSuccessful == false))
                    {
                        Address address = null;
                        try
                        {
                            address = new Address()
                            {
                                ClientId = addressDetails.IntegrationId,
                                AddressId = addressDetails.AddressId,
                                AddressType = addressDetails.AddressType,
                                FullAddress = addressDetails.FullAddress,
                                IsPrimary = !string.IsNullOrEmpty(addressDetails.IsPrimary) && addressDetails.IsPrimary.Equals("Yes", StringComparison.InvariantCultureIgnoreCase)
                            };

                            if (ClientService.GetClientDetails(address.ClientId) != null)
                            {
                                if (addressService.GetAddressDetails(address.ClientId, address.AddressId) == null)
                                {
                                    if (addressService.AddNewAddressDetails(address))
                                    {
                                        Logger.LogDebug(new LogRequest
                                        {
                                            OperationName = this.GetType().Name,
                                            MethodName = "Execute",
                                            Message = "New Address details added successfully.",
                                            NexusData = JsonConvert.SerializeObject(address)
                                        });

                                        addressDetails.IsImportSuccessful = true;
                                    }
                                }
                                else
                                {
                                    if (addressService.UpdateAddressDetails(address))
                                    {
                                        Logger.LogDebug(new LogRequest
                                        {
                                            OperationName = this.GetType().Name,
                                            MethodName = "Execute",
                                            Message = "Existing Address details updated successfully.",
                                            NexusData = JsonConvert.SerializeObject(address)
                                        });

                                        addressDetails.IsImportSuccessful = true;
                                    }
                                }
                            }
                            else
                            {
                                Logger.LogWarning(new LogRequest
                                {
                                    OperationName = this.GetType().Name,
                                    MethodName = "Execute",
                                    Message = "Client Profile does not exist.",
                                    NexusData = JsonConvert.SerializeObject(address)
                                });

                                addressDetails.IsImportSuccessful = false;
                            }
                        }
                        catch (CmiException ce)
                        {
                            Logger.LogWarning(new LogRequest
                            {
                                OperationName = this.GetType().Name,
                                MethodName = "Execute",
                                Message = "Error occurred in API while importing a Address.",
                                Exception = ce,
                                NexusData = JsonConvert.SerializeObject(address)
                            });

                            addressDetails.IsImportSuccessful = false;
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError(new LogRequest
                            {
                                OperationName = this.GetType().Name,
                                MethodName = "Execute",
                                Message = "Error occurred while importing a Address.",
                                Exception = ex,
                                NexusData = JsonConvert.SerializeObject(address)
                            });

                            addressDetails.IsImportSuccessful = false;
                        }
                    }

                    ImporterProvider.SaveAddressesToDatabase(toBeProcessedAddresses);
                }
                else
                {
                    Logger.LogWarning(new LogRequest
                    {
                        OperationName = this.GetType().Name,
                        MethodName = "Execute",
                        Message = "No record available for Address import."
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(new LogRequest
                {
                    OperationName = this.GetType().Name,
                    MethodName = "Execute",
                    Message = "Error occurred while importing Addresses.",
                    Exception = ex,
                    AutomonData = JsonConvert.SerializeObject(retrievedAddresses)
                });
            }

            Logger.LogInfo(new LogRequest
            {
                OperationName = this.GetType().Name,
                MethodName = "Execute",
                Message = "Addresses import completed."
            });
        }
    }
}
