using CMI.Automon.Model;

namespace CMI.Automon.Interface
{
    public interface IOffenderCAMViolationService
    {
        int SaveOffenderCAMViolationDetails(string CmiDbConnString, OffenderCAMViolation offenderCAMViolationDetails);
    }
}
