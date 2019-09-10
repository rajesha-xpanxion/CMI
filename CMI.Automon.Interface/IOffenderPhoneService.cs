using CMI.Automon.Model;
using System;
using System.Collections.Generic;
using System.Data;

namespace CMI.Automon.Interface
{
    public interface IOffenderPhoneService
    {
        IEnumerable<OffenderPhone> GetAllOffenderPhones(string CmiDbConnString, DateTime? lastExecutionDateTime, DataTable officerLogonsToFilterTbl);

        int SaveOffenderPhoneDetails(string CmiDbConnString, OffenderPhone offenderPhoneDetails);
    }
}
