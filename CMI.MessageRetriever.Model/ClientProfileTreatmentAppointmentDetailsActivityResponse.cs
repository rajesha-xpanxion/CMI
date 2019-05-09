using System;

namespace CMI.MessageRetriever.Model
{
    public class ClientProfileTreatmentAppointmentDetailsActivityResponse
    {
        public string TreatmentType { get; set; }
        public DateTime AppointmentDateTime { get; set; }
        public string Status { get; set; }
        public string Notes { get; set; }
        public QuestionsAndAnswerDetails[] QuestionsAndAnswers { get; set; }
    }

    public class QuestionsAndAnswerDetails
    {
        public string Question { get; set; }
        public string Answer { get; set; }
    }
}
