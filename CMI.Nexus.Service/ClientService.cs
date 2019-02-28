using System;
using System.Net.Http;
using System.Net.Http.Headers;
using CMI.Nexus.Interface;
using CMI.Nexus.Model;
using Microsoft.Extensions.Options;

namespace CMI.Nexus.Service
{
    public class ClientService : IClientService
    {
        #region Private Member Variables
        private readonly NexusConfig nexusConfig;
        private readonly IAuthService authService;
        #endregion

        #region Constructor
        public ClientService(
            IOptions<NexusConfig> nexusConfig, 
            IAuthService authService
        )
        {
            this.nexusConfig = nexusConfig.Value;
            this.authService = authService;
        }
        #endregion

        #region Public Methods
        public bool AddNewClientDetails(Client client)
        {
            using (HttpClient apiHost = new HttpClient())
            {
                apiHost.BaseAddress = new Uri(nexusConfig.CaseIntegrationApiBaseUrl);

                apiHost.DefaultRequestHeaders.Accept.Clear();
                apiHost.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.ContentTypeFormatJson));
                apiHost.DefaultRequestHeaders.Add(Constants.HeaderTypeAuthorization, string.Format("{0} {1}", authService.AuthToken.token_type, authService.AuthToken.access_token));

                var apiResponse = apiHost.PostAsJsonAsync<Client>(string.Format("api/{0}/clients", nexusConfig.CaseIntegrationApiVersion), client).Result;
                var responseString = apiResponse.Content.ReadAsStringAsync().Result;

                if (apiResponse.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    throw new CmiException(string.Format("Error occurred while adding new client details. API Response: {0}", responseString));
                }
            }
        }

        public Client GetClientDetails(string clientId)
        {
            Client clientDetails = null;

            using (HttpClient apiHost = new HttpClient())
            {
                apiHost.BaseAddress = new Uri(nexusConfig.CaseIntegrationApiBaseUrl);

                apiHost.DefaultRequestHeaders.Accept.Clear();
                apiHost.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.ContentTypeFormatJson));
                apiHost.DefaultRequestHeaders.Add(Constants.HeaderTypeAuthorization, string.Format("{0} {1}", authService.AuthToken.token_type, authService.AuthToken.access_token));

                var apiResponse = apiHost.GetAsync(string.Format("api/{0}/clients/{1}", nexusConfig.CaseIntegrationApiVersion, clientId)).Result;

                if (apiResponse.IsSuccessStatusCode)
                {
                    clientDetails = apiResponse.Content.ReadAsAsync<Client>().Result;
                }
                else
                {
                    clientDetails = null;
                }
            }

            return clientDetails;
        }

        public bool UpdateClientDetails(Client client)
        {
            using (HttpClient apiHost = new HttpClient())
            {
                apiHost.BaseAddress = new Uri(nexusConfig.CaseIntegrationApiBaseUrl);

                apiHost.DefaultRequestHeaders.Accept.Clear();
                apiHost.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.ContentTypeFormatJson));
                apiHost.DefaultRequestHeaders.Add(Constants.HeaderTypeAuthorization, string.Format("{0} {1}", authService.AuthToken.token_type, authService.AuthToken.access_token));

                var apiResponse = apiHost.PutAsJsonAsync<Client>(string.Format("api/{0}/clients", nexusConfig.CaseIntegrationApiVersion), client).Result;

                var responseString = apiResponse.Content.ReadAsStringAsync().Result;

                if (apiResponse.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    throw new CmiException(string.Format("Error occurred while updating existing client details. API Response: {0}", responseString));
                }
            }
        }
        #endregion
    }
}
