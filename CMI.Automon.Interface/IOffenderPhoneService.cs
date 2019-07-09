using CMI.Automon.Model;
using System;
using System.Collections.Generic;

namespace CMI.Automon.Interface
{
    public interface IOffenderPhoneService
    {
        IEnumerable<OffenderPhone> GetAllOffenderPhones(string CmiDbConnString, DateTime? lastExecutionDateTime);

        int SaveOffenderPhoneDetails(string CmiDbConnString, OffenderPhone offenderPhoneDetails);
    }
}
