using CMI.Automon.Model;

namespace CMI.Automon.Interface
{
    public interface IOffenderTouchPointCheckInService
    {
        int SaveOffenderTouchPointCheckInDetails(string CmiDbConnString, OffenderTouchPointCheckIn offenderTouchPointCheckInDetails);
    }
}
