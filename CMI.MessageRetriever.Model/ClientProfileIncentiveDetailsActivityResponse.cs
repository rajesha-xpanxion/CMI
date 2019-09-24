using System;

namespace CMI.MessageRetriever.Model
{
    public class ClientProfileIncentiveDetailsActivityResponse : DetailsResponse
    {
        public IncentedActivityDetails[] IncentedActivities { get; set; }
        public AssignedIncentiveDetails AssignedIncentive { get; set; }
        public string Status { get; set; }
        public string Notes { get; set; }
    }

    public class IncentedActivityDetails
    {
        public string Activity { get; set; }
        public string ActivityIdentifier { get; set; }
        public DateTime ActivityDateTime { get; set; }
    }

    public class AssignedIncentiveDetails
    {
        public string Magnitude { get; set; }
        public string Description { get; set; }
        public string DateAssigned { get; set; }
    }
}
