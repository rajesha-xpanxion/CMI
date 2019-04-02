using System;

namespace CMI.MessageRetriever.Model
{
    public class DrugTestAppointmentActivityResponse : ActivityResponse
    {
        public DateTime AppointmentDateTime { get; set; }
        public string AppointmentStatus { get; set; }
        public string Location { get; set; }
    }
}
