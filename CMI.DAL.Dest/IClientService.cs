using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CMI.DAL.Dest
{
    public interface IClientService
    {
        bool AddNewClientDetails(Client client);

        Client GetClientDetails(string clientId);

        bool UpdateClientDetails(Client client);
    }
}
