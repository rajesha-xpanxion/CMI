using CMI.Automon.Model;

namespace CMI.Automon.Interface
{
    public interface IOffenderSanctionService
    {
        int SaveOffenderSanctionDetails(string CmiDbConnString, OffenderSanction offenderSanctionDetails);
    }
}
