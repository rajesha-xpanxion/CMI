using CMI.Automon.Model;

namespace CMI.Automon.Interface
{
    public interface IOffenderSanctionService
    {
        void SaveOffenderSanctionDetails(string CmiDbConnString, OffenderSanction offenderSanctionDetails);
    }
}
