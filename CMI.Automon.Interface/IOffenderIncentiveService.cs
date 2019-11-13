using CMI.Automon.Model;

namespace CMI.Automon.Interface
{
    public interface IOffenderIncentiveService
    {
        void SaveOffenderIncentiveDetails(string CmiDbConnString, OffenderIncentive offenderIncentiveDetails);
    }
}
