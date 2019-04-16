
using CMI.Nexus.Model;

namespace CMI.Nexus.Interface
{
    public interface IClientService
    {
        bool AddNewClientDetails(Client client);

        Client GetClientDetails(string clientId);

        bool UpdateClientDetails(Client client);

        bool UpdateClientId(string oldClientId, string newClientId);
    }
}
