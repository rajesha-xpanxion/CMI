using CMI.Nexus.Model;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Options;
using CMI.Nexus.Interface;

namespace CMI.Nexus.Service
{
    public class AuthService : IAuthService
    {
        #region Private Member Variables
        private readonly NexusConfig nexusConfig;
        private AuthTokenResponse _AuthToken;
        #endregion

        #region Public Member Property
        public AuthTokenResponse AuthToken
        {
            get
            {
                if (IsAuthorized())
                {
                    return _AuthToken;
                }
                else
                {
                    return GenerateToken();
                }
            }
        }
        #endregion

        #region Constructor
        public AuthService(
            IOptions<NexusConfig> nexusConfig
        )
        {
            this.nexusConfig = nexusConfig.Value;
        }
        #endregion

        #region Private Helper Methods
        private bool IsAuthorized()
        {
            if(nexusConfig.IsDevMode)
            {
                return true;
            }

            bool isAuthorized = false;

            if (_AuthToken != null && !string.IsNullOrEmpty(_AuthToken.access_token))
            {
                using (HttpClient apiHost = new HttpClient())
                {
                    apiHost.BaseAddress = new Uri(nexusConfig.CaseIntegrationApiBaseUrl);

                    apiHost.DefaultRequestHeaders.Accept.Clear();
                    apiHost.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.ContentTypeFormatJson));
                    apiHost.DefaultRequestHeaders.Add(Constants.HeaderTypeAuthorization, string.Format("{0} {1}", _AuthToken.token_type, _AuthToken.access_token));

                    var apiResponse = apiHost.GetAsync(string.Format("api/{0}/clients/Index", nexusConfig.CaseIntegrationApiVersion)).Result;

                    var responseString = apiResponse.Content.ReadAsStringAsync();

                    if (apiResponse.IsSuccessStatusCode)
                    {
                        isAuthorized = true;
                    }
                    else if (apiResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        isAuthorized = false;
                    }
                    else
                    {
                        throw new CmiException(string.Format("Error occurred while validating token. API Response: {0}", responseString));
                    }
                }
            }

            return isAuthorized;
        }

        private AuthTokenResponse GenerateToken()
        {
            using (HttpClient apiHost = new HttpClient())
            {
                //set base address
                apiHost.BaseAddress = new Uri(nexusConfig.TokenGeneratorConfig.TokenGeneratorApiBaseUrl);

                //set headers
                var idAndSecret = string.Format("{0}:{1}", nexusConfig.TokenGeneratorConfig.ClientId, nexusConfig.TokenGeneratorConfig.ClientSecret);
                var encodedAuthorization = Convert.ToBase64String(Encoding.UTF8.GetBytes(idAndSecret));
                apiHost.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", encodedAuthorization);
                apiHost.DefaultRequestHeaders.Add("Cookie", "recaptchaVerified=true");

                //derive input param dictionary to pass
                Dictionary<string, string> inputParams = new Dictionary<string, string>();
                inputParams.Add("grant_type", nexusConfig.TokenGeneratorConfig.GrantType);
                inputParams.Add("scope", nexusConfig.TokenGeneratorConfig.Scope);
                inputParams.Add("username", nexusConfig.TokenGeneratorConfig.UserName);
                inputParams.Add("password", CryptographyHelper.DecryptString(nexusConfig.TokenGeneratorConfig.Password, nexusConfig.TokenGeneratorConfig.PassPhrase));
                HttpContent content = new FormUrlEncodedContent(inputParams);

                //call API
                var apiResponse = apiHost.PostAsync("identity/connect/token", content).Result;

                string responseString = apiResponse.Content.ReadAsStringAsync().Result;

                if (apiResponse.IsSuccessStatusCode)
                {
                    _AuthToken = apiResponse.Content.ReadAsAsync<AuthTokenResponse>().Result;
                }
                else
                {
                    throw new CmiException(string.Format("Token could not be generated!!!{0}Response: {1}", Environment.NewLine, responseString));
                }
            }

            return _AuthToken;
        }
        #endregion
    }
}
