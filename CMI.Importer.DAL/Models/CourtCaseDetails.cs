
namespace CMI.Importer.DAL
{
    public class CourtCaseDetails : ClientProfileDetails
    {
        public string CaseNumber { get; set; }
        public string CaseDate { get; set; }
        public string Status { get; set; }
        public string EndDate { get; set; }
        public string EarlyReleaseDate { get; set; }
        public string EndReason { get; set; }
    }
}
