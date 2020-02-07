using CMI.Automon.Model;

namespace CMI.Automon.Interface
{
    public interface IOffenderProfilePictureService
    {
        OffenderMugshot GetOffenderMugshotPhoto(string CmiDbConnString, string pin);

        int SaveOffenderMugshotPhoto(string CmiDbConnString, OffenderMugshot offenderMugshot);
        void DeleteOffenderMugshotPhoto(string CmiDbConnString, OffenderMugshot offenderMugshot);
        void SaveOffenderMugshotPhotoToJsonFile(OffenderMugshot offenderMugshotDetails);
    }
}
