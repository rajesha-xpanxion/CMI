using CMI.Nexus.Model;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using CMI.Nexus.Interface;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System.Linq;

namespace CMI.Nexus.Service
{
    public class ContactService : IContactService
    {
        #region Private Member Variables
        private readonly NexusConfig nexusConfig;
        private readonly IAuthService authService;
        #endregion

        #region Constructor
        public ContactService(
            IOptions<NexusConfig> nexusConfig, 
            IAuthService authService
        )
        {
            this.nexusConfig = nexusConfig.Value;
            this.authService = authService;
        }
        #endregion

        #region Public Methods
        public bool AddNewContactDetails(Contact contact)
        {
            if (nexusConfig.IsDevMode)
            {
                return true;
            }

            using (HttpClient apiHost = new HttpClient())
            {
                apiHost.BaseAddress = new Uri(nexusConfig.CaseIntegrationApiBaseUrl);

                apiHost.DefaultRequestHeaders.Accept.Clear();
                apiHost.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.ContentTypeFormatJson));
                apiHost.DefaultRequestHeaders.Add(Constants.HeaderTypeAuthorization, string.Format("{0} {1}", authService.AuthToken.token_type, authService.AuthToken.access_token));

                var apiResponse = apiHost.PostAsJsonAsync<Contact>(string.Format("api/{0}/clients/{1}/contacts", nexusConfig.CaseIntegrationApiVersion, contact.ClientId), contact).Result;
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
            if (nexusConfig.IsDevMode)
            {
                return GetAllContactDetails(clientId).Where(a => a.ContactId.Equals(contactId, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            }

            Contact contactDetails = null;

            using (HttpClient apiHost = new HttpClient())
            {
                apiHost.BaseAddress = new Uri(nexusConfig.CaseIntegrationApiBaseUrl);

                apiHost.DefaultRequestHeaders.Accept.Clear();
                apiHost.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.ContentTypeFormatJson));
                apiHost.DefaultRequestHeaders.Add(Constants.HeaderTypeAuthorization, string.Format("{0} {1}", authService.AuthToken.token_type, authService.AuthToken.access_token));

                var apiResponse = apiHost.GetAsync(string.Format("api/{0}/clients/{1}/contacts/{2}", nexusConfig.CaseIntegrationApiVersion, clientId, contactId)).Result;

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

        public List<Contact> GetAllContactDetails(string clientId)
        {
            if (nexusConfig.IsDevMode)
            {
                //test data
                string testDataJsonFileName = Path.Combine(nexusConfig.TestDataJsonRepoPath, TestDataJsonFileName.AllClientContactDetails);

                return File.Exists(testDataJsonFileName)
                    ? JsonConvert.DeserializeObject<List<Contact>>(File.ReadAllText(testDataJsonFileName)).Where(c => c.ClientId.Equals(clientId, StringComparison.InvariantCultureIgnoreCase)).ToList()
                    : new List<Contact>();
            }
            else
            {
                List<Contact> allContactDetails = null;

                using (HttpClient apiHost = new HttpClient())
                {
                    apiHost.BaseAddress = new Uri(nexusConfig.CaseIntegrationApiBaseUrl);

                    apiHost.DefaultRequestHeaders.Accept.Clear();
                    apiHost.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.ContentTypeFormatJson));
                    apiHost.DefaultRequestHeaders.Add(Constants.HeaderTypeAuthorization, string.Format("{0} {1}", authService.AuthToken.token_type, authService.AuthToken.access_token));

                    var apiResponse = apiHost.GetAsync(string.Format("api/{0}/clients/{1}/contacts/", nexusConfig.CaseIntegrationApiVersion, clientId)).Result;

                    if (apiResponse.IsSuccessStatusCode)
                    {
                        allContactDetails = apiResponse.Content.ReadAsAsync<List<Contact>>().Result;
                    }
                    else
                    {
                        allContactDetails = null;
                    }
                }

                return allContactDetails;
            }
        }

        public bool UpdateContactDetails(Contact contact)
        {
            if (nexusConfig.IsDevMode)
            {
                return true;
            }

            using (HttpClient apiHost = new HttpClient())
            {
                apiHost.BaseAddress = new Uri(nexusConfig.CaseIntegrationApiBaseUrl);

                apiHost.DefaultRequestHeaders.Accept.Clear();
                apiHost.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.ContentTypeFormatJson));
                apiHost.DefaultRequestHeaders.Add(Constants.HeaderTypeAuthorization, string.Format("{0} {1}", authService.AuthToken.token_type, authService.AuthToken.access_token));

                var apiResponse = apiHost.PutAsJsonAsync<Contact>(string.Format("api/{0}/clients/{1}/contacts", nexusConfig.CaseIntegrationApiVersion, contact.ClientId), contact).Result;

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
            if (nexusConfig.IsDevMode)
            {
                return true;
            }

            using (HttpClient apiHost = new HttpClient())
            {
                apiHost.BaseAddress = new Uri(nexusConfig.CaseIntegrationApiBaseUrl);

                apiHost.DefaultRequestHeaders.Accept.Clear();
                apiHost.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.ContentTypeFormatJson));
                apiHost.DefaultRequestHeaders.Add(Constants.HeaderTypeAuthorization, string.Format("{0} {1}", authService.AuthToken.token_type, authService.AuthToken.access_token));

                var apiResponse = apiHost.DeleteAsync(string.Format("api/{0}/clients/{1}/contacts/{2}", nexusConfig.CaseIntegrationApiVersion, clientId, contactId)).Result;

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
