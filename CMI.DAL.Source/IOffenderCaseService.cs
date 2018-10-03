using System;
using System.Collections.Generic;
using System.Text;

namespace CMI.DAL.Source
{
    public interface IOffenderCaseService
    {
        IEnumerable<OffenderCase> GetAllOffenderCases(string CMIDBConnString, DateTime? lastExecutionDateTime);
    }
}
