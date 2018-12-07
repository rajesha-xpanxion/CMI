using CMI.DAL.Dest.Models;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Extensions.Options;

namespace CMI.DAL.Dest.Nexus
{
    public class ContactService : IContactService
    {
        #region Private Member Variables
        private readonly DestinationConfig destinationConfig;
        private readonly IAuthService authService;
        #endregion

        #region Constructor
        public ContactService(
            IOptions<DestinationConfig> destinationConfig, 
            IAuthService authService
        )
        {
            this.destinationConfig = destinationConfig.Value;
            this.authService = authService;
        }
        #endregion

        #region Public Methods
        public bool AddNewContactDetails(Contact contact)
        {
            using (HttpClient apiHost = new HttpClient())
            {
                apiHost.BaseAddress = new Uri(destinationConfig.CaseIntegrationApiBaseUrl);

                apiHost.DefaultRequestHeaders.Accept.Clear();
                apiHost.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.ContentTypeFormatJson));
                apiHost.DefaultRequestHeaders.Add(Constants.HeaderTypeAuthorization, string.Format("{0} {1}", authService.AuthToken.token_type, authService.AuthToken.access_token));

                var apiResponse = apiHost.PostAsJsonAsync<Contact>(string.Format("api/{0}/clients/{1}/contacts", destinationConfig.CaseIntegrationApiVersion, contact.ClientId), contact).Result;
                var responseString = apiResponse.Content.ReadAsStringAsync().Result;

                if (apiResponse.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    throw new CmiException(string.Format("Error occurred while adding new client contact details. API Response: {0}", responseString));
                }
            }
        }

        public Contact GetContactDetails(string clientId, string contactId)
        {
            Contact contactDetails = null;

            using (HttpClient apiHost = new HttpClient())
            {
                apiHost.BaseAddress = new Uri(destinationConfig.CaseIntegrationApiBaseUrl);

                apiHost.DefaultRequestHeaders.Accept.Clear();
                apiHost.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.ContentTypeFormatJson));
                apiHost.DefaultRequestHeaders.Add(Constants.HeaderTypeAuthorization, string.Format("{0} {1}", authService.AuthToken.token_type, authService.AuthToken.access_token));

                var apiResponse = apiHost.GetAsync(string.Format("api/{0}/clients/{1}/contacts/{2}", destinationConfig.CaseIntegrationApiVersion, clientId, contactId)).Result;

                if (apiResponse.IsSuccessStatusCode)
                {
                    contactDetails = apiResponse.Content.ReadAsAsync<Contact>().Result;
                }
                else
                {
                    contactDetails = null;
                }
            }

            return contactDetails;
        }

        public bool UpdateContactDetails(Contact contact)
        {
            using (HttpClient apiHost = new HttpClient())
            {
                apiHost.BaseAddress = new Uri(destinationConfig.CaseIntegrationApiBaseUrl);

                apiHost.DefaultRequestHeaders.Accept.Clear();
                apiHost.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.ContentTypeFormatJson));
                apiHost.DefaultRequestHeaders.Add(Constants.HeaderTypeAuthorization, string.Format("{0} {1}", authService.AuthToken.token_type, authService.AuthToken.access_token));

                var apiResponse = apiHost.PutAsJsonAsync<Contact>(string.Format("api/{0}/clients/{1}/contacts", destinationConfig.CaseIntegrationApiVersion, contact.ClientId), contact).Result;

                var responseString = apiResponse.Content.ReadAsStringAsync().Result;

                if (apiResponse.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    throw new CmiException(string.Format("Error occurred while updating existing client contact details. API Response: {0}", responseString));
                }
            }
        }

        public bool DeleteContactDetails(string clientId, string contactId)
        {
            using (HttpClient apiHost = new HttpClient())
            {
                apiHost.BaseAddress = new Uri(destinationConfig.CaseIntegrationApiBaseUrl);

                apiHost.DefaultRequestHeaders.Accept.Clear();
                apiHost.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.ContentTypeFormatJson));
                apiHost.DefaultRequestHeaders.Add(Constants.HeaderTypeAuthorization, string.Format("{0} {1}", authService.AuthToken.token_type, authService.AuthToken.access_token));

                var apiResponse = apiHost.DeleteAsync(string.Format("api/{0}/clients/{1}/contacts/{2}", destinationConfig.CaseIntegrationApiVersion, clientId, contactId)).Result;

                var responseString = apiResponse.Content.ReadAsStringAsync().Result;

                if (apiResponse.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    throw new CmiException(string.Format("Error occurred while deleting existing client contact details. API Response: {0}", responseString));
                }
            }
        }
        #endregion
    }
}
