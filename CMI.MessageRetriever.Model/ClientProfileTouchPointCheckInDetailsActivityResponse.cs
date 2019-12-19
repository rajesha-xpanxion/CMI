using System;

namespace CMI.MessageRetriever.Model
{
    public class ClientProfileTouchPointCheckInDetailsActivityResponse
    {
        public DateTime CheckInDateTime { get; set; }
        public string FaceMatch { get; set; }
        public string Status { get; set; }
        public ResponseDetails[] Responses { get; set; }
        public string[] Notes { get; set; }
        public string[] Warnings { get; set; }
        public string Location { get; set; }
    }

    public class ResponseDetails
    {
        public string Question { get; set; }
        public string Answer { get; set; }
    }
}
