using CMI.Automon.Model;
using System;
using System.Collections.Generic;

namespace CMI.Automon.Interface
{
    public interface IOffenderService
    {
        IEnumerable<Offender> GetAllOffenderDetails(string CmiDbConnString, DateTime? lastExecutionDateTime);

        void SaveOffenderPersonalDetails(string CmiDbConnString, Offender offenderDetails);
    }
}
