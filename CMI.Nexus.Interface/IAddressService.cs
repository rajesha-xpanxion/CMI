using CMI.Nexus.Model;
using System.Collections.Generic;

namespace CMI.Nexus.Interface
{
    public interface IAddressService
    {
        bool AddNewAddressDetails(Address address);

        Address GetAddressDetails(string clientId, string addressId);

        List<Address> GetAllAddressDetails(string clientId);

        bool UpdateAddressDetails(Address address);

        bool DeleteAddressDetails(string clientId, string addressId);
    }
}
