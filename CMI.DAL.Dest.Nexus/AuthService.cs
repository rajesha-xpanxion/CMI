using CMI.DAL.Dest.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace CMI.DAL.Dest.Nexus
{
    public class AuthService : IAuthService
    {
        DestinationConfig destinationConfig;

        private AuthTokenResponse _AuthToken;
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

        public AuthService(Microsoft.Extensions.Options.IOptions<DestinationConfig> destinationConfig)
        {
            this.destinationConfig = destinationConfig.Value;
        }

        private bool IsAuthorized()
        {
            bool isAuthorized = false;

            if (_AuthToken != null && !string.IsNullOrEmpty(_AuthToken.access_token))
            {
                using (HttpClient apiHost = new HttpClient())
                {
                    apiHost.BaseAddress = new Uri(destinationConfig.CaseIntegrationAPIBaseURL);

                    apiHost.DefaultRequestHeaders.Accept.Clear();
                    apiHost.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.ContentTypeFormatJSON));
                    apiHost.DefaultRequestHeaders.Add(Constants.HeaderTypeAuthorization, string.Format("{0} {1}", _AuthToken.token_type, _AuthToken.access_token));

                    var apiResponse = apiHost.GetAsync(string.Format("api/{0}/clients/IAmAlive", destinationConfig.CaseIntegrationAPIVersion)).Result;

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
                        throw new ApplicationException(string.Format("Error occurred while validating token. API Response: {0}", responseString));
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
                apiHost.BaseAddress = new Uri(destinationConfig.TokenGeneratorConfig.TokenGeneratorAPIBaseURL);

                //set headers
                var idAndSecret = string.Format("{0}:{1}", destinationConfig.TokenGeneratorConfig.ClientId, destinationConfig.TokenGeneratorConfig.ClientSecret);
                var encodedAuthorization = Convert.ToBase64String(Encoding.UTF8.GetBytes(idAndSecret));
                apiHost.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", encodedAuthorization);
                apiHost.DefaultRequestHeaders.Add("Cookie", "recaptchaVerified=true");

                //derive input param dictionary to pass
                Dictionary<string, string> inputParams = new Dictionary<string, string>();
                inputParams.Add("grant_type", destinationConfig.TokenGeneratorConfig.GrantType);
                inputParams.Add("scope", destinationConfig.TokenGeneratorConfig.Scope);
                inputParams.Add("username", destinationConfig.TokenGeneratorConfig.UserName);
                inputParams.Add("password", destinationConfig.TokenGeneratorConfig.Password);
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
                    throw new ApplicationException(string.Format("Token could not be generated!!!{0}Response: {1}", Environment.NewLine, responseString));
                }

            }

            return _AuthToken;
        }
    }
}
