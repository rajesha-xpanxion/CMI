using System;

namespace CMI.MessageRetriever.Model
{
    public class ClientProfileFieldVisitDetailsActivityResponse
    {
        public DateTime DateTime { get; set; }
        public string Status { get; set; }
        public string Note { get; set; }
        public FoundContrabandDetails[] FoundContraband { get; set; }
        public VisitedLocationDetails[] VisitedLocations { get; set; }
    }

    public class VisitedLocationDetails
    {
        public string Address { get; set; }
        public string Type { get; set; }
        public string ClientPresent { get; set; }
        public string[] SearchedAreas { get; set; }
    }

    public class FoundContrabandDetails
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Quantity { get; set; }
        public string Description { get; set; }
    }
}
