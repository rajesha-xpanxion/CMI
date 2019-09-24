
using CMI.Nexus.Model;

namespace CMI.Nexus.Interface
{
    public interface ICaseService
    {
        bool AddNewCaseDetails(Case @case);

        Case GetCaseDetails(string clientId, string caseNumber);

        Case GetCaseDetailsUsingAllEndPoint(string clientId, string caseNumber);

        bool UpdateCaseDetails(Case @case);
    }
}
