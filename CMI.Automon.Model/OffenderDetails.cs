
namespace CMI.Automon.Model
{
    public class OffenderDetails : Offender
    {
        public string EmailAddress { get; set; }
        public string AddressType { get; set; }
        public string Line1 { get; set; }
        public string Line2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string PhoneNumberType { get; set; }
        public string Phone { get; set; }
    }
}
