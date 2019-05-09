using CMI.Automon.Model;

namespace CMI.Automon.Interface
{
    public interface IOffenderTreatmentAppointmentService
    {
        void SaveOffenderTreatmentAppointmentDetails(string CmiDbConnString, OffenderTreatmentAppointment offenderTreatmentAppointmentDetails);
    }
}
