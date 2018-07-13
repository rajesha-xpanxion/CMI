using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Http;
using System.Net.Http.Headers;
using CMI.DAL.Dest.Models;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CMI.DAL.Dest.Nexus
{
    public class ClientService : IClientService
    {
        DestinationConfig destinationConfig;
        IAuthService authService;

        public ClientService(Microsoft.Extensions.Options.IOptions<DestinationConfig> destinationConfig, IAuthService authService)
        {
            this.destinationConfig = destinationConfig.Value;
            this.authService = authService;
        }

        public bool AddNewClientDetails(Client client)
        {
            using (HttpClient apiHost = new HttpClient())
            {
                apiHost.BaseAddress = new Uri(destinationConfig.CaseIntegrationAPIBaseURL);

                apiHost.DefaultRequestHeaders.Accept.Clear();
                apiHost.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.ContentTypeFormatJSON));
                apiHost.DefaultRequestHeaders.Add(Constants.HeaderTypeAuthorization, string.Format("{0} {1}", authService.AuthToken.token_type, authService.AuthToken.access_token));

                var apiResponse = apiHost.PostAsJsonAsync<Client>("api/v1/clients", client).Result;
                var responseString = apiResponse.Content.ReadAsStringAsync().Result;

                if (apiResponse.IsSuccessStatusCode)
                {
                    //Console.WriteLine("New client details added successfully.{0}Response: {1}", Environment.NewLine, responseString);
                    return true;
                }
                else
                {
                    throw new ApplicationException(string.Format("Error occurred while adding new client details. API Response: {0}", responseString));
                }
            }
        }

        public Client GetClientDetails(string clientId)
        {
            Client clientDetails = null;

            using (HttpClient apiHost = new HttpClient())
            {
                apiHost.BaseAddress = new Uri(destinationConfig.CaseIntegrationAPIBaseURL);

                apiHost.DefaultRequestHeaders.Accept.Clear();
                apiHost.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.ContentTypeFormatJSON));
                apiHost.DefaultRequestHeaders.Add(Constants.HeaderTypeAuthorization, string.Format("{0} {1}", authService.AuthToken.token_type, authService.AuthToken.access_token));

                var apiResponse = apiHost.GetAsync(string.Format("api/v1/clients/{0}", clientId)).Result;

                var responseString = apiResponse.Content.ReadAsStringAsync().Result;

                if (apiResponse.IsSuccessStatusCode)
                {
                    clientDetails = apiResponse.Content.ReadAsAsync<Client>().Result;
                    Console.WriteLine("{0}Client details received:{0}{1}", Environment.NewLine, responseString);
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
                apiHost.BaseAddress = new Uri(destinationConfig.CaseIntegrationAPIBaseURL);

                apiHost.DefaultRequestHeaders.Accept.Clear();
                apiHost.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.ContentTypeFormatJSON));
                apiHost.DefaultRequestHeaders.Add(Constants.HeaderTypeAuthorization, string.Format("{0} {1}", authService.AuthToken.token_type, authService.AuthToken.access_token));

                var apiResponse = apiHost.PutAsJsonAsync<Client>("api/v1/clients", client).Result;

                var responseString = apiResponse.Content.ReadAsStringAsync().Result;

                if (apiResponse.IsSuccessStatusCode)
                {
                    return true;
                    //Console.WriteLine("Existing client details updated successfully.{0}Response: {1}", Environment.NewLine, responseString);
                }
                else
                {
                    throw new ApplicationException(string.Format("Error occurred while updating existing client details. API Response: {0}", responseString));
                }
            }
        }
    }
}
