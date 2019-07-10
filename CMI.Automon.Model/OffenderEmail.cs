
namespace CMI.Automon.Model
{
    public class OffenderEmail : Offender
    {
        public string EmailAddress { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsActive { get; set; }
    }
}
