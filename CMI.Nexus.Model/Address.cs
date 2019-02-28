
namespace CMI.Nexus.Model
{
    public class Address
    {
        public string ClientId { get; set; }
        public string AddressId { get; set; }
        public string Comment { get; set; }
        public string FullAddress { get; set; }
        public string AddressType { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsActive { get; set; }
    }
}
