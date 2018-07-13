using System;
using System.Collections.Generic;
using System.Text;

namespace CMI.DAL.Dest
{
    public interface ICaseService
    {
        bool AddNewCaseDetails(Case @case);

        Case GetCaseDetails(string clientId, string caseNumber);

        bool UpdateCaseDetails(Case @case);
    }
}
