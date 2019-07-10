
namespace CMI.Automon.Model
{
    public class OffenderPhone : Offender
    {
        public string PhoneNumberType { get; set; }
        public string Phone { get; set; }
        public bool IsPrimary { get; set; }
        public string Comment { get; set; }
        public bool IsActive { get; set; }
    }
}
