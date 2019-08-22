using CMI.Nexus.Model;
using System.Collections.Generic;

namespace CMI.Nexus.Interface
{
    public interface IVehicleService
    {
        bool AddNewVehicleDetails(Vehicle vehicle);

        Vehicle GetVehicleDetails(string clientId, string vehicleId);

        List<Vehicle> GetAllVehicleDetails(string clientId);

        bool UpdateVehicleDetails(Vehicle vehicle);

        bool DeleteVehicleDetails(string clientId, string vehicleId);
    }
}
