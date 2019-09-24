using CMI.Automon.Model;

namespace CMI.Automon.Interface
{
    public interface IOffenderIncentiveService
    {
        int SaveOffenderIncentiveDetails(string CmiDbConnString, OffenderIncentive offenderIncentiveDetails);
    }
}
