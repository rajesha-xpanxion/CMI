using CMI.Automon.Model;

namespace CMI.Automon.Interface
{
    public interface IOffenderTreatmentAppointmentService
    {
        int SaveOffenderTreatmentAppointmentDetails(string CmiDbConnString, OffenderTreatmentAppointment offenderTreatmentAppointmentDetails);
    }
}
