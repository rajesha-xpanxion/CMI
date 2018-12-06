using System;
using System.Collections.Generic;

namespace CMI.DAL.Source
{
    public interface IOffenderCaseService
    {
        IEnumerable<OffenderCase> GetAllOffenderCases(string CmiDbConnString, DateTime? lastExecutionDateTime);
    }
}
