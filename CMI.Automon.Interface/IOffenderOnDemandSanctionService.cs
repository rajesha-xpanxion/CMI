using CMI.Automon.Model;

namespace CMI.Automon.Interface
{
    public interface IOffenderOnDemandSanctionService
    {
        void SaveOffenderOnDemandSanctionDetails(string CmiDbConnString, OffenderOnDemandSanction offenderOnDemandSanctionDetails);
    }
}
