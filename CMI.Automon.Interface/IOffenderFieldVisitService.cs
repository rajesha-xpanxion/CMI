
using CMI.Automon.Model;

namespace CMI.Automon.Interface
{
    public interface IOffenderFieldVisitService
    {
        int SaveOffenderFieldVisitDetails(string CmiDbConnString, OffenderFieldVisit offenderFieldVisitDetails);
    }
}
