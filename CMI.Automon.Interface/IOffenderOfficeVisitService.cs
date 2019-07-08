
using CMI.Automon.Model;

namespace CMI.Automon.Interface
{
    public interface IOffenderOfficeVisitService
    {
        int SaveOffenderOfficeVisitDetails(string CmiDbConnString, OffenderOfficeVisit offenderOfficeVisitDetails);
    }
}
