using CMI.Automon.Model;
using System;
using System.Collections.Generic;
using System.Data;

namespace CMI.Automon.Interface
{
    public interface IOffenderVehicleService
    {
        int SaveOffenderVehicleDetails(string CmiDbConnString, OffenderVehicle offenderVehicleDetails);
        void DeleteOffenderVehicleDetails(string CmiDbConnString, OffenderVehicle offenderVehicleDetails);
        IEnumerable<OffenderVehicle> GetAllOffenderVehicles(string CmiDbConnString, DateTime? lastExecutionDateTime, DataTable officerLogonsToFilterTbl);
    }
}
