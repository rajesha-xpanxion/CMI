using System;

namespace CMI.MessageProcessor.Model
{
    public class FieldVisitActivityResponse : ActivityResponse
    {
        public DateTime Datetime { get; set; }
        public string Status { get; set; }
        public string Note { get; set; }
        public string[] FoundContraband { get; set; }
        public VisitedLocationDetails[] VisitedLocations { get; set; }
    }

    public class VisitedLocationDetails
    {
        public string Address { get; set; }
        public string Type { get; set; }
        public string ClientPresent { get; set; }
        public string SearchedAreas { get; set; }
    }
}
