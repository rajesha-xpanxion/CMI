
namespace CMI.Importer.DAL
{
    public class AddressDetails : ClientProfileDetails
    {
        public string AddressId { get; set; }
        public string FullAddress { get; set; }
        public string AddressType { get; set; }
        public string IsPrimary { get; set; }
    }
}
