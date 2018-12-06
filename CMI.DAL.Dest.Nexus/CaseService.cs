using CMI.DAL.Dest.Models;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Extensions.Options;

namespace CMI.DAL.Dest.Nexus
{
    public class CaseService : ICaseService
    {
        #region Private Member Variables
        private readonly DestinationConfig destinationConfig;
        private readonly IAuthService authService;
        #endregion

        #region Constructor
        public CaseService(
            IOptions<DestinationConfig> destinationConfig, 
            IAuthService authService
        )
        {
            this.destinationConfig = destinationConfig.Value;
            this.authService = authService;
        }
        #endregion

        #region Public Methods
        public bool AddNewCaseDetails(Case @case)
        {
            using (HttpClient apiHost = new HttpClient())
            {
                apiHost.BaseAddress = new Uri(destinationConfig.CaseIntegrationApiBaseUrl);

                apiHost.DefaultRequestHeaders.Accept.Clear();
                apiHost.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.ContentTypeFormatJson));
                apiHost.DefaultRequestHeaders.Add(Constants.HeaderTypeAuthorization, string.Format("{0} {1}", authService.AuthToken.token_type, authService.AuthToken.access_token));

                var apiResponse = apiHost.PostAsJsonAsync<Case>(string.Format("api/{0}/clients/{1}/cases", destinationConfig.CaseIntegrationApiVersion, @case.ClientId), @case).Result;
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
                apiHost.BaseAddress = new Uri(destinationConfig.CaseIntegrationApiBaseUrl);

                apiHost.DefaultRequestHeaders.Accept.Clear();
                apiHost.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.ContentTypeFormatJson));
                apiHost.DefaultRequestHeaders.Add(Constants.HeaderTypeAuthorization, string.Format("{0} {1}", authService.AuthToken.token_type, authService.AuthToken.access_token));

                var apiResponse = apiHost.GetAsync(string.Format("api/{0}/clients/{1}/cases/{2}", destinationConfig.CaseIntegrationApiVersion, clientId, caseNumber)).Result;

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
                apiHost.BaseAddress = new Uri(destinationConfig.CaseIntegrationApiBaseUrl);

                apiHost.DefaultRequestHeaders.Accept.Clear();
                apiHost.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.ContentTypeFormatJson));
                apiHost.DefaultRequestHeaders.Add(Constants.HeaderTypeAuthorization, string.Format("{0} {1}", authService.AuthToken.token_type, authService.AuthToken.access_token));

                var apiResponse = apiHost.PutAsJsonAsync<Case>(string.Format("api/{0}/clients/{1}/cases", destinationConfig.CaseIntegrationApiVersion, @case.ClientId), @case).Result;

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
        #endregion
    }
}
