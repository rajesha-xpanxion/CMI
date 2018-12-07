using CMI.DAL.Dest.Models;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Extensions.Options;

namespace CMI.DAL.Dest.Nexus
{
    public class AddressService : IAddressService
    {
        #region Private Member Variables
        private readonly DestinationConfig destinationConfig;
        private readonly IAuthService authService;
        #endregion

        #region Constructor
        public AddressService(
            IOptions<DestinationConfig> destinationConfig, 
            IAuthService authService
        )
        {
            this.destinationConfig = destinationConfig.Value;
            this.authService = authService;
        }
        #endregion

        #region Public Methods
        public bool AddNewAddressDetails(Address address)
        {
            using (HttpClient apiHost = new HttpClient())
            {
                apiHost.BaseAddress = new Uri(destinationConfig.CaseIntegrationApiBaseUrl);

                apiHost.DefaultRequestHeaders.Accept.Clear();
                apiHost.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.ContentTypeFormatJson));
                apiHost.DefaultRequestHeaders.Add(Constants.HeaderTypeAuthorization, string.Format("{0} {1}", authService.AuthToken.token_type, authService.AuthToken.access_token));

                var apiResponse = apiHost.PostAsJsonAsync<Address>(string.Format("api/{0}/clients/{1}/addresses", destinationConfig.CaseIntegrationApiVersion, address.ClientId), address).Result;
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
            Address addressDetails = null;

            using (HttpClient apiHost = new HttpClient())
            {
                apiHost.BaseAddress = new Uri(destinationConfig.CaseIntegrationApiBaseUrl);

                apiHost.DefaultRequestHeaders.Accept.Clear();
                apiHost.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.ContentTypeFormatJson));
                apiHost.DefaultRequestHeaders.Add(Constants.HeaderTypeAuthorization, string.Format("{0} {1}", authService.AuthToken.token_type, authService.AuthToken.access_token));

                var apiResponse = apiHost.GetAsync(string.Format("api/{0}/clients/{1}/addresses/{2}", destinationConfig.CaseIntegrationApiVersion, clientId, addressId)).Result;

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

        public bool UpdateAddressDetails(Address address)
        {
            using (HttpClient apiHost = new HttpClient())
            {
                apiHost.BaseAddress = new Uri(destinationConfig.CaseIntegrationApiBaseUrl);

                apiHost.DefaultRequestHeaders.Accept.Clear();
                apiHost.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.ContentTypeFormatJson));
                apiHost.DefaultRequestHeaders.Add(Constants.HeaderTypeAuthorization, string.Format("{0} {1}", authService.AuthToken.token_type, authService.AuthToken.access_token));

                var apiResponse = apiHost.PutAsJsonAsync<Address>(string.Format("api/{0}/clients/{1}/addresses", destinationConfig.CaseIntegrationApiVersion, address.ClientId), address).Result;

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
            using (HttpClient apiHost = new HttpClient())
            {
                apiHost.BaseAddress = new Uri(destinationConfig.CaseIntegrationApiBaseUrl);

                apiHost.DefaultRequestHeaders.Accept.Clear();
                apiHost.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.ContentTypeFormatJson));
                apiHost.DefaultRequestHeaders.Add(Constants.HeaderTypeAuthorization, string.Format("{0} {1}", authService.AuthToken.token_type, authService.AuthToken.access_token));

                var apiResponse = apiHost.DeleteAsync(string.Format("api/{0}/clients/{1}/addresses/{2}", destinationConfig.CaseIntegrationApiVersion, clientId, addressId)).Result;

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
