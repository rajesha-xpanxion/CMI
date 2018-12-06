using System;
using System.Collections.Generic;

namespace CMI.DAL.Source
{
    public interface IOffenderPhoneService
    {
        IEnumerable<OffenderPhone> GetAllOffenderPhones(string CmiDbConnString, DateTime? lastExecutionDateTime);
    }
}
