using System;

namespace CMI.MessageRetriever.Model
{
    public class ClientProfileSanctionDetailsActivityResponse
    {
        public SanctionedActivityDetails[] SanctionActivities { get; set; }
        public AssignedSanctionDetails AssignedSanction { get; set; }
        public string Status { get; set; }
        public string Notes { get; set; }
    }

    public class SanctionedActivityDetails
    {
        public string Activity { get; set; }
        public string ActivityIdentifier { get; set; }
        public DateTime ActivityDateTime { get; set; }
    }

    public class AssignedSanctionDetails
    {
        public string Magnitude { get; set; }
        public string Description { get; set; }
        public string DateAssigned { get; set; }
    }
}
