using CMI.Automon.Model;
using System;
using System.Collections.Generic;

namespace CMI.Automon.Interface
{
    public interface IOffenderAddressService
    {
        IEnumerable<OffenderAddress> GetAllOffenderAddresses(string CmiDbConnString, DateTime? lastExecutionDateTime);

        int SaveOffenderAddressDetails(string CmiDbConnString, OffenderAddress offenderAddressDetails);
    }
}
