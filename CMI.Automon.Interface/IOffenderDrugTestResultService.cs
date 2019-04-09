
using CMI.Automon.Model;

namespace CMI.Automon.Interface
{
    public interface IOffenderDrugTestResultService
    {
        void SaveOffenderDrugTestResultDetails(string CmiDbConnString, OffenderDrugTestResult offenderDrugTestResultDetails);
    }
}
