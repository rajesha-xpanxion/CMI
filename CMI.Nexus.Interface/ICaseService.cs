
using CMI.Nexus.Model;
using System.Collections.Generic;

namespace CMI.Nexus.Interface
{
    public interface ICaseService
    {
        bool AddNewCaseDetails(Case @case);

        Case GetCaseDetails(string clientId, string caseNumber);

        List<Case> GetAllCaseDetails(string clientId);

        Case GetCaseDetailsUsingAllEndPoint(string clientId, string caseNumber);

        bool UpdateCaseDetails(Case @case);
    }
}
