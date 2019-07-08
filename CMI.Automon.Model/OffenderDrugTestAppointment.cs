using System;
using System.Collections.Generic;
using System.Text;

namespace CMI.Automon.Model
{
    public class OffenderDrugTestAppointment : Offender
    {
        public int Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Status { get; set; }
        public string Location { get; set; }
    }
}
