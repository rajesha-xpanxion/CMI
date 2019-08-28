using CMI.Automon.Model;
using System;
using System.Collections.Generic;

namespace CMI.Automon.Interface
{
    public interface IOffenderService
    {
        IEnumerable<Offender> GetAllOffenderDetails(string CmiDbConnString, DateTime? lastExecutionDateTime);

        string SaveOffenderDetails(string CmiDbConnString, OffenderDetails offenderDetails);

        string GetTimeZone();

        OffenderMugshot GetOffenderMugshotPhoto(string CmiDbConnString, string pin);
    }
}
