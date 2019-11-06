using System;

namespace CMI.MessageRetriever.Model
{
    public class ClientProfileOnDemandSanctionDetailsActivityResponse
    {
        public OnDemandSanctionedActivityDetails[] SanctionedActivities { get; set; }
        public AssignedOnDemandSanctionDetails AssignedSanction { get; set; }
        public string Status { get; set; }
        public string Notes { get; set; }
    }

    public class OnDemandSanctionedActivityDetails
    {
        public string TermOfSupervision { get; set; }
        public string Description { get; set; }
        public DateTime ViolationDateTime { get; set; }
    }

    public class AssignedOnDemandSanctionDetails
    {
        public string Magnitude { get; set; }
        public string Description { get; set; }
    }
}
