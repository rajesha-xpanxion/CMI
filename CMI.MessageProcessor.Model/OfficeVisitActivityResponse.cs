using System;

namespace CMI.MessageProcessor.Model
{
    public class OfficeVisitActivityResponse : ActivityResponse
    {
        public DateTime DateTime { get; set; }
        public string Status { get; set; }
        public string Notes { get; set; }
    }
}
