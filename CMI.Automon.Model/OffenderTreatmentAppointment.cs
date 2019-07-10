using System;

namespace CMI.Automon.Model
{
    public class OffenderTreatmentAppointment : Offender
    {
        public DateTime StartDate { get; set; }
        public string Comment { get; set; }
        public DateTime EndDate { get; set; }
        public int Status { get; set; }
    }
}
