
using CMI.Automon.Model;

namespace CMI.Automon.Interface
{
    public interface IOffenderDrugTestResultService
    {
        int SaveOffenderDrugTestResultDetails(string CmiDbConnString, OffenderDrugTestResult offenderDrugTestResultDetails);
        void DeleteOffenderDrugTestResultDetails(string CmiDbConnString, OffenderDrugTestResult offenderDrugTestResultDetails);
    }
}
