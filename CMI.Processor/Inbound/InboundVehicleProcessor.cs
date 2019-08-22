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

        public override TaskExecutionStatus Execute(DateTime? lastExecutionDateTime)
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
                allOffenderVehicles = offenderVehicleService.GetAllOffenderVehicles(ProcessorConfig.CmiDbConnString, lastExecutionDateTime);

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
                            Model = offenderVehicleDetails.BodyStyle,
                            Year = offenderVehicleDetails.VehicleYear,
                            LicensePlate = offenderVehicleDetails.LicensePlate,
                            Color = offenderVehicleDetails.Color,
                            IsActive = offenderVehicleDetails.IsActive
                        };

                        if (ClientService.GetClientDetails(vehicle.ClientId) != null)
                        {
                            if (vehicleService.GetVehicleDetails(vehicle.ClientId, vehicle.VehicleId) == null)
                            {

                                if (vehicle.IsActive && vehicleService.AddNewVehicleDetails(vehicle))
                                {
                                    taskExecutionStatus.NexusAddRecordCount++;

                                    Logger.LogDebug(new LogRequest
                                    {
                                        OperationName = this.GetType().Name,
                                        MethodName = "Execute",
                                        Message = "New Client Phone Vehicle details added successfully.",
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
                                        Message = "Existing Client Vehicle Contact details deleted successfully.",
                                        AutomonData = JsonConvert.SerializeObject(offenderVehicleDetails),
                                        NexusData = JsonConvert.SerializeObject(vehicle)
                                    });
                                }
                            }
                            else
                            {
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
