using System.Collections.Generic;

namespace CMI.Importer.DAL
{
    public interface IImporterProvider
    {
        IEnumerable<ClientProfileDetails> RetrieveClientProfileDataFromExcel();

        IEnumerable<ContactDetails> RetrieveContactDataFromExcel();

        IEnumerable<AddressDetails> RetrieveAddressDataFromExcel();

        IEnumerable<CourtCaseDetails> RetrieveCourtCaseDataFromExcel();

        IEnumerable<ClientProfileDetails> SaveClientProfilesToDatabase(IEnumerable<ClientProfileDetails> receivedClientProfiles);

        IEnumerable<ContactDetails> SaveContactsToDatabase(IEnumerable<ContactDetails> receivedContacts);

        IEnumerable<AddressDetails> SaveAddressesToDatabase(IEnumerable<AddressDetails> receivedAddresses);

        IEnumerable<CourtCaseDetails> SaveCourtCasesToDatabase(IEnumerable<CourtCaseDetails> receivedCourtCases);
    }
}
