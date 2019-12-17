
using CMI.Nexus.Model;

namespace CMI.Nexus.Interface
{
    public interface IClientService
    {
        bool AddNewClientDetails(Client client);

        Client GetClientDetails(string clientId);

        bool UpdateClientDetails(Client client);

        bool UpdateClientId(string oldClientId, string newClientId);

        bool AddNewClientProfilePicture(ClientProfilePicture clientProfilePicture);

        ClientProfilePicture GetClientProfilePicture(string clientId);

        bool UpdateClientProfilePicture(ClientProfilePicture clientProfilePicture);

        bool DeleteClientProfilePicture(string clientId);

        bool UpdateClientCmsStatus(string clientId, string cmsStatus);
    }
}
