using CMI.Automon.Model;
using System;
using System.Collections.Generic;

namespace CMI.Automon.Interface
{
    public interface IOffenderVehicleService
    {
        void SaveOffenderVehicleDetails(string CmiDbConnString, OffenderVehicle offenderVehicleDetails);
        void DeleteOffenderVehicleDetails(string CmiDbConnString, OffenderVehicle offenderVehicleDetails);
    }
}
