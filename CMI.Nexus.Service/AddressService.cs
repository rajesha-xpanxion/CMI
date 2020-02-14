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
    public class AddressService : IAddressService
    {
        #region Private Member Variables
        private readonly NexusConfig nexusConfig;
        private readonly IAuthService authService;
        #endregion

        #region Constructor
        public AddressService(
            IOptions<NexusConfig> nexusConfig, 
            IAuthService authService
        )
        {
            this.nexusConfig = nexusConfig.Value;
            this.authService = authService;
        }
        #endregion

        #region Public Methods
        public bool AddNewAddressDetails(Address address)
        {
            if(nexusConfig.IsDevMode)
            {
                return true;
            }

            using (HttpClient apiHost = new HttpClient())
            {
                apiHost.BaseAddress = new Uri(nexusConfig.CaseIntegrationApiBaseUrl);

                apiHost.DefaultRequestHeaders.Accept.Clear();
                apiHost.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.ContentTypeFormatJson));
                apiHost.DefaultRequestHeaders.Add(Constants.HeaderTypeAuthorization, string.Format("{0} {1}", authService.AuthToken.token_type, authService.AuthToken.access_token));

                var apiResponse = apiHost.PostAsJsonAsync<Address>(string.Format("api/{0}/clients/{1}/addresses", nexusConfig.CaseIntegrationApiVersion, address.ClientId), address).Result;
                var responseString = apiResponse.Content.ReadAsStringAsync().Result;

                if (apiResponse.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    throw new CmiException(string.Format("Error occurred while adding new client address details. API Response: {0}", responseString));
                }
            }
        }

        public Address GetAddressDetails(string clientId, string addressId)
        {
            if(nexusConfig.IsDevMode)
            {
                return GetAllAddressDetails(clientId).Where(a => a.AddressId.Equals(addressId, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            }

            Address addressDetails = null;

            using (HttpClient apiHost = new HttpClient())
            {
                apiHost.BaseAddress = new Uri(nexusConfig.CaseIntegrationApiBaseUrl);

                apiHost.DefaultRequestHeaders.Accept.Clear();
                apiHost.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.ContentTypeFormatJson));
                apiHost.DefaultRequestHeaders.Add(Constants.HeaderTypeAuthorization, string.Format("{0} {1}", authService.AuthToken.token_type, authService.AuthToken.access_token));

                var apiResponse = apiHost.GetAsync(string.Format("api/{0}/clients/{1}/addresses/{2}", nexusConfig.CaseIntegrationApiVersion, clientId, addressId)).Result;

                if (apiResponse.IsSuccessStatusCode)
                {
                    addressDetails = apiResponse.Content.ReadAsAsync<Address>().Result;
                }
                else
                {
                    addressDetails = null;
                }
            }

            return addressDetails;
        }

        public List<Address> GetAllAddressDetails(string clientId)
        {
            if (nexusConfig.IsDevMode)
            {
                //test data
                string testDataJsonFileName = Path.Combine(nexusConfig.TestDataJsonRepoPath, TestDataJsonFileName.AllClientAddressDetails);

                return File.Exists(testDataJsonFileName)
                    ? JsonConvert.DeserializeObject<List<Address>>(File.ReadAllText(testDataJsonFileName)).Where(a => a.ClientId.Equals(clientId, StringComparison.InvariantCultureIgnoreCase)).ToList()
                    : new List<Address>();
            }
            else
            {
                List<Address> allAddressDetails = null;

                using (HttpClient apiHost = new HttpClient())
                {
                    apiHost.BaseAddress = new Uri(nexusConfig.CaseIntegrationApiBaseUrl);

                    apiHost.DefaultRequestHeaders.Accept.Clear();
                    apiHost.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.ContentTypeFormatJson));
                    apiHost.DefaultRequestHeaders.Add(Constants.HeaderTypeAuthorization, string.Format("{0} {1}", authService.AuthToken.token_type, authService.AuthToken.access_token));

                    var apiResponse = apiHost.GetAsync(string.Format("api/{0}/clients/{1}/addresses", nexusConfig.CaseIntegrationApiVersion, clientId)).Result;

                    if (apiResponse.IsSuccessStatusCode)
                    {
                        allAddressDetails = apiResponse.Content.ReadAsAsync<List<Address>>().Result;
                    }
                    else
                    {
                        allAddressDetails = null;
                    }
                }

                return allAddressDetails;
            }
        }

        public bool UpdateAddressDetails(Address address)
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

                var apiResponse = apiHost.PutAsJsonAsync<Address>(string.Format("api/{0}/clients/{1}/addresses", nexusConfig.CaseIntegrationApiVersion, address.ClientId), address).Result;

                var responseString = apiResponse.Content.ReadAsStringAsync().Result;

                if (apiResponse.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    throw new CmiException(string.Format("Error occurred while updating existing client address details. API Response: {0}", responseString));
                }
            }
        }

        public bool DeleteAddressDetails(string clientId, string addressId)
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

                var apiResponse = apiHost.DeleteAsync(string.Format("api/{0}/clients/{1}/addresses/{2}", nexusConfig.CaseIntegrationApiVersion, clientId, addressId)).Result;

                var responseString = apiResponse.Content.ReadAsStringAsync().Result;

                if (apiResponse.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    throw new CmiException(string.Format("Error occurred while deleting existing client address details. API Response: {0}", responseString));
                }
            }
        }
        #endregion
    }
}
