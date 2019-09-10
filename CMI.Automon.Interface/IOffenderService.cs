using CMI.Automon.Model;
using System;
using System.Collections.Generic;
using System.Data;

namespace CMI.Automon.Interface
{
    public interface IOffenderService
    {
        IEnumerable<Offender> GetAllOffenderDetails(string CmiDbConnString, DateTime? lastExecutionDateTime, DataTable officerLogonsToFilterTbl);

        string SaveOffenderDetails(string CmiDbConnString, OffenderDetails offenderDetails);

        string GetTimeZone();
    }
}
