
using CMI.Automon.Model;

namespace CMI.Automon.Interface
{
    public interface IOffenderFieldVisitService
    {
        void SaveOffenderFieldVisitDetails(string CmiDbConnString, OffenderFieldVisit offenderFieldVisitDetails);
    }
}
