
using CMI.Automon.Model;

namespace CMI.Automon.Interface
{
    public interface IOffenderOfficeVisitService
    {
        void SaveOffenderOfficeVisitDetails(string CmiDbConnString, OffenderOfficeVisit offenderOfficeVisitDetails);
    }
}
