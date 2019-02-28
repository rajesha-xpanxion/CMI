using CMI.Automon.Model;
using System;
using System.Collections.Generic;

namespace CMI.Automon.Interface
{
    public interface IOffenderCaseService
    {
        IEnumerable<OffenderCase> GetAllOffenderCases(string CmiDbConnString, DateTime? lastExecutionDateTime);
    }
}
