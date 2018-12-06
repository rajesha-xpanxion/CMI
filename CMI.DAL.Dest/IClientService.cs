
namespace CMI.DAL.Dest
{
    public interface IClientService
    {
        bool AddNewClientDetails(Client client);

        Client GetClientDetails(string clientId);

        bool UpdateClientDetails(Client client);
    }
}
