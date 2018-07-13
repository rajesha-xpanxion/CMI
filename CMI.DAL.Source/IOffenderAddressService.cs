using System;
using System.Collections.Generic;
using System.Text;

namespace CMI.DAL.Source
{
    public interface IOffenderAddressService
    {
        IEnumerable<OffenderAddress> GetAllOffenderAddresses(DateTime lastExecutionDateTime);
    }
}
