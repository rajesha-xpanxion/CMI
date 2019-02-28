
namespace CMI.Automon.Model
{
    public class OffenderAddress : Offender
    {
        public int Id { get; set; }

        public string AddressType { get; set; }

        public string Line1 { get; set; }

        public string Line2 { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        public string Zip { get; set; }

        public bool IsPrimary { get; set; }

        public string Comment { get; set; }

        public bool IsActive { get; set; }
    }
}
