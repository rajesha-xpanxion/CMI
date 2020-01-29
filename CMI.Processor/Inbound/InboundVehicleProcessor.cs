using CMI.Automon.Interface;
using CMI.Automon.Model;
using CMI.Common.Logging;
using CMI.Common.Notification;
using CMI.Nexus.Interface;
using CMI.Nexus.Model;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CMI.Processor
{
    public class InboundVehicleProcessor : InboundBaseProcessor
    {
        private readonly IOffenderVehicleService offenderVehicleService;
        private readonly IVehicleService vehicleService;

        public InboundVehicleProcessor(
            IServiceProvider serviceProvider,
            IConfiguration configuration,
            IOffenderVehicleService offenderVehicleService,
            IVehicleService vehicleService
        )
            : base(serviceProvider, configuration)
        {
            this.offenderVehicleService = offenderVehicleService;
            this.vehicleService = vehicleService;
        }

        public override TaskExecutionStatus Execute(DateTime? lastExecutionDateTime, IEnumerable<string> officerLogonsToFilter)
        {
            Logger.LogInfo(new LogRequest
            {
                OperationName = this.GetType().Name,
                MethodName = "Execute",
                Message = "Vehicle processing initiated."
            });

            //load required lookup data
            LoadLookupData();

            IEnumerable<OffenderVehicle> allOffenderVehicles = null;
            Common.Notification.TaskExecutionStatus taskExecutionStatus = new Common.Notification.TaskExecutionStatus { ProcessorType = ProcessorType.Inbound, TaskName = "Process Vehicles" };

            try
            {
                allOffenderVehicles = offenderVehicleService.GetAllOffenderVehicles(ProcessorConfig.CmiDbConnString, lastExecutionDateTime, GetOfficerLogonToFilterDataTable(officerLogonsToFilter));

                //log number of records received from Automon
                if (allOffenderVehicles.Any())
                {
                    Logger.LogDebug(new LogRequest
                    {
                        OperationName = this.GetType().Name,
                        MethodName = "Execute",
                        Message = "Offender Vehicle records received from Automon.",
                        CustomParams = allOffenderVehicles.Count().ToString()
                    });
                }

                foreach (var offenderVehicleDetails in allOffenderVehicles)
                {
                    taskExecutionStatus.AutomonReceivedRecordCount++;

                    Vehicle vehicle = null;
                    try
                    {
                        vehicle = new Vehicle()
                        {
                            ClientId = FormatId(offenderVehicleDetails.Pin),
                            VehicleId = string.Format("{0}-{1}", FormatId(offenderVehicleDetails.Pin), offenderVehicleDetails.Id),
                            Make = offenderVehicleDetails.Make,
                            Model = Nexus.Service.VehicleModel.Unknown,     //set default value for Model as in Nexus API this field in mandatory
                            Year = offenderVehicleDetails.VehicleYear,
                            LicensePlate = offenderVehicleDetails.LicensePlate,
                            Color = offenderVehicleDetails.Color,
                            IsActive = offenderVehicleDetails.IsActive
                        };

                        if (ClientService.GetClientDetails(vehicle.ClientId) != null)
                        {
                            Vehicle existingVehicleDetails = vehicleService.GetVehicleDetails(vehicle.ClientId, vehicle.VehicleId);
                            if (existingVehicleDetails == null)
                            {
                                if (vehicle.IsActive && vehicleService.AddNewVehicleDetails(vehicle))
                                {
                                    taskExecutionStatus.NexusAddRecordCount++;

                                    Logger.LogDebug(new LogRequest
                                    {
                                        OperationName = this.GetType().Name,
                                        MethodName = "Execute",
                                        Message = "New Client Vehicle details added successfully.",
                                        AutomonData = JsonConvert.SerializeObject(offenderVehicleDetails),
                                        NexusData = JsonConvert.SerializeObject(vehicle)
                                    });
                                }
                                else
                                {
                                    taskExecutionStatus.AutomonReceivedRecordCount--;
                                }
                            }
                            else if (!vehicle.IsActive)
                            {
                                if (vehicleService.DeleteVehicleDetails(vehicle.ClientId, vehicle.VehicleId))
                                {
                                    taskExecutionStatus.NexusDeleteRecordCount++;

                                    Logger.LogDebug(new LogRequest
                                    {
                                        OperationName = this.GetType().Name,
                                        MethodName = "Execute",
                                        Message = "Existing Client Vehicle details deleted successfully.",
                                        AutomonData = JsonConvert.SerializeObject(offenderVehicleDetails),
                                        NexusData = JsonConvert.SerializeObject(vehicle)
                                    });
                                }
                            }
                            else
                            {
                                //log details of existing client profile vehicle details
                                Logger.LogDebug(new LogRequest
                                {
                                    OperationName = this.GetType().Name,
                                    MethodName = "Execute",
                                    Message = "Client Profile - Vehicle details already exists.",
                                    AutomonData = JsonConvert.SerializeObject(offenderVehicleDetails),
                                    NexusData = JsonConvert.SerializeObject(existingVehicleDetails)
                                });

                                //check if any value already exists for Make && no value being passed from Automon, yes = replace it in passing model so that existing value will not be replaced/changed
                                if (
                                    !string.IsNullOrEmpty(existingVehicleDetails.Make) 
                                    && !existingVehicleDetails.Make.Equals(Nexus.Service.VehicleMake.Unknown) 
                                    && 
                                    (
                                        string.IsNullOrEmpty(vehicle.Make)
                                        || vehicle.Make.Equals(Automon.Service.VehicleMake.Unknown, StringComparison.InvariantCultureIgnoreCase)
                                        || vehicle.Make.Equals(Automon.Service.VehicleMake.Other, StringComparison.InvariantCultureIgnoreCase)
                                    )
                                )
                                {
                                    vehicle.Make = existingVehicleDetails.Make;
                                }

                                //restore value of Vehicle Model
                                vehicle.Model = existingVehicleDetails.Model;

                                //check if any value already exists for Color, yes = replace it in passing model so that existing value will not be replaced/changed
                                if (
                                    !string.IsNullOrEmpty(existingVehicleDetails.Color) 
                                    && !existingVehicleDetails.Color.Equals(Nexus.Service.VehicleColor.Unknown)
                                    &&
                                    (
                                        string.IsNullOrEmpty(vehicle.Color)
                                        || vehicle.Color.Equals(Automon.Service.VehicleColor.Unknown, StringComparison.InvariantCultureIgnoreCase)
                                        || vehicle.Color.Equals(Automon.Service.VehicleColor.Other, StringComparison.InvariantCultureIgnoreCase)
                                    )
                                )
                                {
                                    vehicle.Color = existingVehicleDetails.Color;
                                }

                                //update vehicle details in Nexus
                                if (vehicleService.UpdateVehicleDetails(vehicle))
                                {
                                    taskExecutionStatus.NexusUpdateRecordCount++;

                                    Logger.LogDebug(new LogRequest
                                    {
                                        OperationName = this.GetType().Name,
                                        MethodName = "Execute",
                                        Message = "Existing Client Vehicle details updated successfully.",
                                        AutomonData = JsonConvert.SerializeObject(offenderVehicleDetails),
                                        NexusData = JsonConvert.SerializeObject(vehicle)
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
                            OperationName = this.GetType().Name,
                            MethodName = "Execute",
                            Message = "Error occurred in API while processing a Client Vehicle.",
                            Exception = ce,
                            AutomonData = JsonConvert.SerializeObject(offenderVehicleDetails),
                            NexusData = JsonConvert.SerializeObject(vehicle)
                        });
                    }
                    catch (Exception ex)
                    {
                        taskExecutionStatus.NexusFailureRecordCount++;

                        Logger.LogError(new LogRequest
                        {
                            OperationName = this.GetType().Name,
                            MethodName = "Execute",
                            Message = "Error occurred while processing a Client Vehicle.",
                            Exception = ex,
                            AutomonData = JsonConvert.SerializeObject(offenderVehicleDetails),
                            NexusData = JsonConvert.SerializeObject(vehicle)
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
                    OperationName = this.GetType().Name,
                    MethodName = "Execute",
                    Message = "Error occurred while processing Vehicles.",
                    Exception = ex,
                    AutomonData = JsonConvert.SerializeObject(allOffenderVehicles)
                });
            }

            Logger.LogInfo(new LogRequest
            {
                OperationName = this.GetType().Name,
                MethodName = "Execute",
                Message = "Vehicles processing completed.",
                CustomParams = JsonConvert.SerializeObject(taskExecutionStatus)
            });

            return taskExecutionStatus;
        }

        protected override void LoadLookupData()
        {
        }
    }
}
