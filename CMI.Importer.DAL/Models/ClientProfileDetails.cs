
namespace CMI.Importer.DAL
{
    public class ClientProfileDetails
    {
        public int Id { get; set; }
        public bool IsImportSuccessful { get; set; }
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
    }
}
