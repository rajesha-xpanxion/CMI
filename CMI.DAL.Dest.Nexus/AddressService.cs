using CMI.DAL.Dest.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace CMI.DAL.Dest.Nexus
{
    public class AddressService : IAddressService
    {
        DestinationConfig destinationConfig;
        IAuthService authService;

        public AddressService(Microsoft.Extensions.Options.IOptions<DestinationConfig> destinationConfig, IAuthService authService)
        {
            this.destinationConfig = destinationConfig.Value;
            this.authService = authService;
        }

        public bool AddNewAddressDetails(Address address)
        {
            using (HttpClient apiHost = new HttpClient())
            {
                apiHost.BaseAddress = new Uri(destinationConfig.CaseIntegrationAPIBaseURL);

                apiHost.DefaultRequestHeaders.Accept.Clear();
                apiHost.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.ContentTypeFormatJSON));
                apiHost.DefaultRequestHeaders.Add(Constants.HeaderTypeAuthorization, string.Format("{0} {1}", authService.AuthToken.token_type, authService.AuthToken.access_token));

                var apiResponse = apiHost.PostAsJsonAsync<Address>(string.Format("api/v1/clients/{0}/addresses", address.ClientId), address).Result;
                var responseString = apiResponse.Content.ReadAsStringAsync().Result;

                if (apiResponse.IsSuccessStatusCode)
                {
                    return true;
                    //Console.WriteLine("New client address details added successfully.{0}Response: {1}", Environment.NewLine, responseString);
                }
                else
                {
                    throw new ApplicationException(string.Format("Error occurred while adding new client address details. API Response: {0}", responseString));
                }
            }
        }

        public Address GetAddressDetails(string clientId, string addressId)
        {
            Address addressDetails = null;

            using (HttpClient apiHost = new HttpClient())
            {
                apiHost.BaseAddress = new Uri(destinationConfig.CaseIntegrationAPIBaseURL);

                apiHost.DefaultRequestHeaders.Accept.Clear();
                apiHost.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.ContentTypeFormatJSON));
                apiHost.DefaultRequestHeaders.Add(Constants.HeaderTypeAuthorization, string.Format("{0} {1}", authService.AuthToken.token_type, authService.AuthToken.access_token));

                var apiResponse = apiHost.GetAsync(string.Format("api/v1/clients/{0}/addresses/{1}", clientId, addressId)).Result;

                var responseString = apiResponse.Content.ReadAsStringAsync().Result;

                if (apiResponse.IsSuccessStatusCode)
                {
                    addressDetails = apiResponse.Content.ReadAsAsync<Address>().Result;
                    Console.WriteLine("{0}Client address details received:{0}{1}", Environment.NewLine, responseString);
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
                apiHost.BaseAddress = new Uri(destinationConfig.CaseIntegrationAPIBaseURL);

                apiHost.DefaultRequestHeaders.Accept.Clear();
                apiHost.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.ContentTypeFormatJSON));
                apiHost.DefaultRequestHeaders.Add(Constants.HeaderTypeAuthorization, string.Format("{0} {1}", authService.AuthToken.token_type, authService.AuthToken.access_token));

                var apiResponse = apiHost.PutAsJsonAsync<Address>(string.Format("api/v1/clients/{0}/addresses", address.ClientId), address).Result;

                var responseString = apiResponse.Content.ReadAsStringAsync().Result;

                if (apiResponse.IsSuccessStatusCode)
                {
                    return true;
                    //Console.WriteLine("Existing client address details updated successfully.{0}Response: {1}", Environment.NewLine, responseString);
                }
                else
                {
                    throw new ApplicationException(string.Format("Error occurred while updating existing client address details. API Response: {0}", responseString));
                }
            }
        }
    }
}
