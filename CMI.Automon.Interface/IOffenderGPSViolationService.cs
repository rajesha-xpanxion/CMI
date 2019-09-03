using CMI.Automon.Model;

namespace CMI.Automon.Interface
{
    public interface IOffenderGPSViolationService
    {
        int SaveOffenderGPSViolationDetails(string CmiDbConnString, OffenderGPSViolation offenderGPSViolationDetails);
    }
}
