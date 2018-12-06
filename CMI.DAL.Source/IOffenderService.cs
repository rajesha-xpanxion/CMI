using System;
using System.Collections.Generic;

namespace CMI.DAL.Source
{
    public interface IOffenderService
    {
        IEnumerable<Offender> GetAllOffenderDetails(string CmiDbConnString, DateTime? lastExecutionDateTime);
    }
}
