
namespace CMI.Nexus.Model
{
    public class Client
    {
        public string IntegrationId { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string ClientType { get; set; }
        public string TimeZone { get; set; }
        public string Gender { get; set; }
        public string Ethnicity { get; set; }
        public string DateOfBirth { get; set; }
        public string SupervisingOfficerEmailId { get; set; }
        public string CaseloadId { get; set; }
        public string StaticRiskRating { get; set; }
        public string NeedsClassification { get; set; }
    }

    public class ClientProfilePicture : Client
    {
        public string ImageBase64String { get; set; }
    }
}
