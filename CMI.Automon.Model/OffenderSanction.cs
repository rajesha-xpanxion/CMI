using System;
using System.Collections.Generic;

namespace CMI.Automon.Model
{
    public class OffenderSanction : Offender
    {
        public DateTime EventDateTime { get; set; }
        public string Magnitude { get; set; }
        public string Response { get; set; }
        public string DateIssued { get; set; }
        public bool IsBundled { get; set; }
        public bool IsSkipped { get; set; }
        public IEnumerable<SanctionedActivityDetails> SanctionedActivities { get; set; }
    }

    public class SanctionedActivityDetails
    {
        public string ActivityTypeName { get; set; }
        public string ActivityIdentifier { get; set; }
    }
}
