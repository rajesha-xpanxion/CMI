using System;
using System.Collections.Generic;

namespace CMI.DAL.Source
{
    public interface IOffenderAddressService
    {
        IEnumerable<OffenderAddress> GetAllOffenderAddresses(string CmiDbConnString, DateTime? lastExecutionDateTime);
    }
}
