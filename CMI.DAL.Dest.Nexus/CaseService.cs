using CMI.DAL.Dest.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace CMI.DAL.Dest.Nexus
{
    public class CaseService : ICaseService
    {
        DestinationConfig destinationConfig;
        IAuthService authService;

        public CaseService(Microsoft.Extensions.Options.IOptions<DestinationConfig> destinationConfig, IAuthService authService)
        {
            this.destinationConfig = destinationConfig.Value;
            this.authService = authService;
        }

        public bool AddNewCaseDetails(Case @case)
        {
            using (HttpClient apiHost = new HttpClient())
            {
                apiHost.BaseAddress = new Uri(destinationConfig.CaseIntegrationAPIBaseURL);

                apiHost.DefaultRequestHeaders.Accept.Clear();
                apiHost.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.ContentTypeFormatJSON));
                apiHost.DefaultRequestHeaders.Add(Constants.HeaderTypeAuthorization, string.Format("{0} {1}", authService.AuthToken.token_type, authService.AuthToken.access_token));

                var apiResponse = apiHost.PostAsJsonAsync<Case>(string.Format("api/{0}/clients/{1}/cases", destinationConfig.CaseIntegrationAPIVersion, @case.ClientId), @case).Result;
                var responseString = apiResponse.Content.ReadAsStringAsync().Result;

                if (apiResponse.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    throw new ApplicationException(string.Format("Error occurred while adding new client case details. API Response: {0}", responseString));
                }
            }
        }

        public Case GetCaseDetails(string clientId, string caseNumber)
        {
            Case caseDetails = null;

            using (HttpClient apiHost = new HttpClient())
            {
                apiHost.BaseAddress = new Uri(destinationConfig.CaseIntegrationAPIBaseURL);

                apiHost.DefaultRequestHeaders.Accept.Clear();
                apiHost.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.ContentTypeFormatJSON));
                apiHost.DefaultRequestHeaders.Add(Constants.HeaderTypeAuthorization, string.Format("{0} {1}", authService.AuthToken.token_type, authService.AuthToken.access_token));

                var apiResponse = apiHost.GetAsync(string.Format("api/{0}/clients/{1}/cases/{2}", destinationConfig.CaseIntegrationAPIVersion, clientId, caseNumber)).Result;

                var responseString = apiResponse.Content.ReadAsStringAsync().Result;

                if (apiResponse.IsSuccessStatusCode)
                {
                    caseDetails = apiResponse.Content.ReadAsAsync<Case>().Result;
                }
                else
                {
                    caseDetails = null;
                }
            }


            return caseDetails;
        }

        public bool UpdateCaseDetails(Case @case)
        {
            using (HttpClient apiHost = new HttpClient())
            {
                apiHost.BaseAddress = new Uri(destinationConfig.CaseIntegrationAPIBaseURL);

                apiHost.DefaultRequestHeaders.Accept.Clear();
                apiHost.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.ContentTypeFormatJSON));
                apiHost.DefaultRequestHeaders.Add(Constants.HeaderTypeAuthorization, string.Format("{0} {1}", authService.AuthToken.token_type, authService.AuthToken.access_token));

                var apiResponse = apiHost.PutAsJsonAsync<Case>(string.Format("api/{0}/clients/{1}/cases", destinationConfig.CaseIntegrationAPIVersion, @case.ClientId), @case).Result;

                var responseString = apiResponse.Content.ReadAsStringAsync().Result;

                if (apiResponse.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    throw new ApplicationException(string.Format("Error occurred while updating existing client case details. API Response: {0}", responseString));
                }
            }
        }
    }
}
