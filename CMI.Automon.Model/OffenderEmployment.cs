
namespace CMI.Automon.Model
{
    public class OffenderEmployment : Offender
    {
        public string OrganizationName { get; set; }
        public string OrganizationAddress { get; set; }
        public string OrganizationPhone { get; set; }
        public string PayFrequency { get; set; }
        public string PayRate { get; set; }
        public string WorkType { get; set; }
        public string JobTitle { get; set; }
        public bool IsActive { get; set; }
    }
}
