using System.Collections.Generic;

namespace CMI.DAL.Dest
{
    public class Case
    {
        public string ClientId { get; set; }
        public string CaseNumber { get; set; }
        public string CaseDate { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string EndReason { get; set; }
        public string Status { get; set; }
        public List<Offense> Offenses { get; set; }
    }
}
