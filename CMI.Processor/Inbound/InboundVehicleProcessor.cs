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

            IEnumerable<OffenderVehicle> allOffenderVehicles = null;
            TaskExecutionStatus taskExecutionStatus = new TaskExecutionStatus { ProcessorType = Common.Notification.ProcessorType.Inbound, TaskName = "Process Vehicles" };

            try
            {
                //retrieve data from Automon for processing
                allOffenderVehicles = offenderVehicleService.GetAllOffenderVehicles(ProcessorConfig.CmiDbConnString, lastExecutionDateTime, GetOfficerLogonToFilterDataTable(officerLogonsToFilter));

                //check if there are any records to process
                if (allOffenderVehicles.Any())
                {
                    //check if there are any records to process
                    Logger.LogDebug(new LogRequest
                    {
                        OperationName = this.GetType().Name,
                        MethodName = "Execute",
                        Message = "Offender Vehicle records received from Automon.",
                        CustomParams = allOffenderVehicles.Count().ToString()
                    });

                    //retrieve distinct list of offender pin
                    List<string> distinctOffenderPin = allOffenderVehicles.Select(p => p.Pin).Distinct().ToList();

                    //iterate through each of offender pin
                    foreach (string currentOffenderPin in distinctOffenderPin)
                    {
                        //check if client exist on Nexus side for current offender pin. This is to avoid validation errors from Nexus API in further calls.
                        if (ClientService.GetClientDetails(currentOffenderPin) != null)
                        {
                            //get all vehicles for given offender pin
                            var allExistingVehicleDetails = vehicleService.GetAllVehicleDetails(currentOffenderPin);

                            if (allExistingVehicleDetails != null && allExistingVehicleDetails.Any())
                            {
                                //set ClientId value
                                allExistingVehicleDetails.ForEach(ea => ea.ClientId = currentOffenderPin);
                            }

                            //iterate through each of offender vehicle details for current offender pin
                            foreach (var offenderVehicleDetails in allOffenderVehicles.Where(a => a.Pin.Equals(currentOffenderPin, StringComparison.InvariantCultureIgnoreCase)))
                            {
                                taskExecutionStatus.AutomonReceivedRecordCount++;

                                Vehicle vehicle = null;
                                try
                                {
                                    //transform offender vehicle details in Nexus compliant model
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

                                    //get crud action type based on comparison
                                    switch (GetCrudActionType(vehicle, allExistingVehicleDetails))
                                    {
                                        case CrudActionType.Add:
                                            if (vehicleService.AddNewVehicleDetails(vehicle))
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
                                            break;
                                        case CrudActionType.Update:
                                            Vehicle existingVehicleDetails = allExistingVehicleDetails.Where(v
                                                => v.ClientId.Equals(vehicle.ClientId, StringComparison.InvariantCultureIgnoreCase) && v.VehicleId.Equals(vehicle.VehicleId, StringComparison.InvariantCultureIgnoreCase)
                                            ).FirstOrDefault();

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
                                            break;
                                        case CrudActionType.Delete:
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
            throw new NotImplementedException();
        }

        private CrudActionType GetCrudActionType(Vehicle vehicle, IEnumerable<Vehicle> vehicles)
        {
            //check if list is null
            if (vehicles == null)
            {
                return vehicle.IsActive ? CrudActionType.Add : CrudActionType.None;
            }

            //try to get existing record using ClientId & VehicleId
            Vehicle existingVehicle = vehicles.Where(a
                =>
                    a.ClientId.Equals(vehicle.ClientId, StringComparison.InvariantCultureIgnoreCase)
                    && a.VehicleId.Equals(vehicle.VehicleId, StringComparison.InvariantCultureIgnoreCase)
            )
            .FirstOrDefault();

            //check if record already exists
            if (existingVehicle == null)
            {
                //record does not exist.
                //check if record is active. YES = return Add action type, NO = return None action type
                if (vehicle.IsActive)
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
                if (vehicle.IsActive)
                {
                    //record is active.
                    //compare with existing record. Equal = return None action type, Not Equal = return Update action type
                    if (vehicle.Equals(existingVehicle))
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
