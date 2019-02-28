
using CMI.Nexus.Model;

namespace CMI.Nexus.Interface
{
    public interface IContactService
    {
        bool AddNewContactDetails(Contact contact);

        Contact GetContactDetails(string clientId, string contactId);

        bool UpdateContactDetails(Contact contact);

        bool DeleteContactDetails(string clientId, string contactId);
    }
}
