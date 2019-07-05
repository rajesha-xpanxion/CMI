using System;
using System.Net.Http;
using System.Net.Http.Headers;
using CMI.Nexus.Interface;
using CMI.Nexus.Model;
using Microsoft.Extensions.Options;

namespace CMI.Nexus.Service
{
    public class CommonService : ICommonService
    {
        #region Private Member Variables
        private readonly NexusConfig nexusConfig;
        private readonly IAuthService authService;
        #endregion

        #region Constructor
        public CommonService(
            IOptions<NexusConfig> nexusConfig,
            IAuthService authService
        )
        {
            this.nexusConfig = nexusConfig.Value;
            this.authService = authService;
        }
        #endregion

        #region Public Methods
        public bool UpdateId(string clientId, ReplaceIntegrationIdDetails replaceIntegrationIdDetails)
        {
            using (HttpClient apiHost = new HttpClient())
            {
                apiHost.BaseAddress = new Uri(nexusConfig.CaseIntegrationApiBaseUrl);

                apiHost.DefaultRequestHeaders.Accept.Clear();
                apiHost.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.ContentTypeFormatJson));
                apiHost.DefaultRequestHeaders.Add(Constants.HeaderTypeAuthorization, string.Format("{0} {1}", authService.AuthToken.token_type, authService.AuthToken.access_token));

                var apiResponse = apiHost.PutAsJsonAsync<ReplaceIntegrationIdDetails>(string.Format("api/{0}/clients/{1}/replaceIntegrationIds", nexusConfig.CaseIntegrationApiVersion, clientId), replaceIntegrationIdDetails).Result;

                var responseString = apiResponse.Content.ReadAsStringAsync().Result;

                if (apiResponse.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    throw new CmiException(string.Format("Error occurred while updating existing integration Id for element type: {0}. API Response: {1}", replaceIntegrationIdDetails.ElementType, responseString));
                }
            }
        }
        #endregion
    }
}
