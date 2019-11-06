using CMI.Automon.Model;

namespace CMI.Automon.Interface
{
    public interface IOffenderOnDemandSanctionService
    {
        int SaveOffenderOnDemandSanctionDetails(string CmiDbConnString, OffenderOnDemandSanction offenderOnDemandSanctionDetails);
    }
}
