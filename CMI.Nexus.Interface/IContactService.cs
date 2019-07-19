
using CMI.Nexus.Model;
using System.Collections.Generic;

namespace CMI.Nexus.Interface
{
    public interface IContactService
    {
        bool AddNewContactDetails(Contact contact);

        Contact GetContactDetails(string clientId, string contactId);

        List<Contact> GetAllContactDetails(string clientId);

        bool UpdateContactDetails(Contact contact);

        bool DeleteContactDetails(string clientId, string contactId);
    }
}
