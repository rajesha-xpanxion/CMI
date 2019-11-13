using System;
using System.Collections.Generic;

namespace CMI.Automon.Model
{
    public class OffenderOnDemandSanction : Offender
    {
        public string Magnitude { get; set; }
        public string Response { get; set; }
        public bool IsSkipped { get; set; }
        public string Notes { get; set; }
        public IEnumerable<OnDemandSanctionedActivityDetails> OnDemandSanctionedActivities { get; set; }
    }

    public class OnDemandSanctionedActivityDetails
    {
        public string TermOfSupervision { get; set; }
        public string Description { get; set; }
        public DateTime EventDateTime { get; set; }
    }
}
