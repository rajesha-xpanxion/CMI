using CMI.Nexus.Model;

namespace CMI.Nexus.Interface
{
    public interface IAddressService
    {
        bool AddNewAddressDetails(Address address);

        Address GetAddressDetails(string clientId, string addressId);

        bool UpdateAddressDetails(Address address);

        bool DeleteAddressDetails(string clientId, string addressId);
    }
}
