using System;

namespace CMI.Automon.Model
{
    public class OffenderDrugTestAppointment : Offender
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Status { get; set; }
        public string Location { get; set; }
        public bool IsOffenderPresent { get; set; }
    }
}
