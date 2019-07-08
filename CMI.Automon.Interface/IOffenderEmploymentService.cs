using CMI.Automon.Model;

namespace CMI.Automon.Interface
{
    public interface IOffenderEmploymentService
    {
        int SaveOffenderEmploymentDetails(string CmiDbConnString, OffenderEmployment offenderEmploymentDetails);
        void DeleteOffenderEmploymentDetails(string CmiDbConnString, OffenderEmployment offenderEmploymentDetails);
    }
}
