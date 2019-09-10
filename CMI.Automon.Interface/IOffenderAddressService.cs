using CMI.Automon.Model;
using System;
using System.Collections.Generic;
using System.Data;

namespace CMI.Automon.Interface
{
    public interface IOffenderAddressService
    {
        IEnumerable<OffenderAddress> GetAllOffenderAddresses(string CmiDbConnString, DateTime? lastExecutionDateTime, DataTable officerLogonsToFilterTbl);

        int SaveOffenderAddressDetails(string CmiDbConnString, OffenderAddress offenderAddressDetails);
    }
}
