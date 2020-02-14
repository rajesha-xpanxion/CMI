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
    public class EmploymentService : IEmploymentService
    {
        #region Private Member Variables
        private readonly NexusConfig nexusConfig;
        private readonly IAuthService authService;
        #endregion

        #region Constructor
        public EmploymentService(
            IOptions<NexusConfig> nexusConfig,
            IAuthService authService
        )
        {
            this.nexusConfig = nexusConfig.Value;
            this.authService = authService;
        }
        #endregion

        #region Public Methods
        public bool AddNewEmploymentDetails(Employment employment)
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

                var apiResponse = apiHost.PostAsJsonAsync<Employment>(string.Format("api/{0}/clients/{1}/employers", nexusConfig.CaseIntegrationApiVersion, employment.ClientId), employment).Result;
                var responseString = apiResponse.Content.ReadAsStringAsync().Result;

                if (apiResponse.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    throw new CmiException(string.Format("Error occurred while adding new client employment details. API Response: {0}", responseString));
                }
            }
        }

        public Employment GetEmploymentDetails(string clientId, string employmentId)
        {
            if (nexusConfig.IsDevMode)
            {
                return GetAllEmploymentDetails(clientId).Where(a => a.EmployerId.Equals(employmentId, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            }

            Employment employmentDetails = null;

            using (HttpClient apiHost = new HttpClient())
            {
                apiHost.BaseAddress = new Uri(nexusConfig.CaseIntegrationApiBaseUrl);

                apiHost.DefaultRequestHeaders.Accept.Clear();
                apiHost.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.ContentTypeFormatJson));
                apiHost.DefaultRequestHeaders.Add(Constants.HeaderTypeAuthorization, string.Format("{0} {1}", authService.AuthToken.token_type, authService.AuthToken.access_token));

                var apiResponse = apiHost.GetAsync(string.Format("api/{0}/clients/{1}/employers/{2}", nexusConfig.CaseIntegrationApiVersion, clientId, employmentId)).Result;

                if (apiResponse.IsSuccessStatusCode)
                {
                    employmentDetails = apiResponse.Content.ReadAsAsync<Employment>().Result;
                }
                else
                {
                    employmentDetails = null;
                }
            }

            return employmentDetails;
        }

        public List<Employment> GetAllEmploymentDetails(string clientId)
        {
            if (nexusConfig.IsDevMode)
            {
                //test data
                string testDataJsonFileName = Path.Combine(nexusConfig.TestDataJsonRepoPath, TestDataJsonFileName.AllClientEmployerDetails);

                return File.Exists(testDataJsonFileName)
                    ? JsonConvert.DeserializeObject<List<Employment>>(File.ReadAllText(testDataJsonFileName)).Where(c => c.ClientId.Equals(clientId, StringComparison.InvariantCultureIgnoreCase)).ToList()
                    : new List<Employment>();
            }
            else
            {
                List<Employment> allEmploymentDetails = null;

                using (HttpClient apiHost = new HttpClient())
                {
                    apiHost.BaseAddress = new Uri(nexusConfig.CaseIntegrationApiBaseUrl);

                    apiHost.DefaultRequestHeaders.Accept.Clear();
                    apiHost.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.ContentTypeFormatJson));
                    apiHost.DefaultRequestHeaders.Add(Constants.HeaderTypeAuthorization, string.Format("{0} {1}", authService.AuthToken.token_type, authService.AuthToken.access_token));

                    var apiResponse = apiHost.GetAsync(string.Format("api/{0}/clients/{1}/employers", nexusConfig.CaseIntegrationApiVersion, clientId)).Result;

                    if (apiResponse.IsSuccessStatusCode)
                    {
                        allEmploymentDetails = apiResponse.Content.ReadAsAsync<List<Employment>>().Result;
                    }
                    else
                    {
                        allEmploymentDetails = null;
                    }
                }

                return allEmploymentDetails;
            }
        }

        public bool UpdateEmploymentDetails(Employment employment)
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

                var apiResponse = apiHost.PutAsJsonAsync<Employment>(string.Format("api/{0}/clients/{1}/employers", nexusConfig.CaseIntegrationApiVersion, employment.ClientId), employment).Result;

                var responseString = apiResponse.Content.ReadAsStringAsync().Result;

                if (apiResponse.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    throw new CmiException(string.Format("Error occurred while updating existing client employment details. API Response: {0}", responseString));
                }
            }
        }

        public bool DeleteEmploymentDetails(string clientId, string employmentId)
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

                var apiResponse = apiHost.DeleteAsync(string.Format("api/{0}/clients/{1}/employers/{2}", nexusConfig.CaseIntegrationApiVersion, clientId, employmentId)).Result;

                var responseString = apiResponse.Content.ReadAsStringAsync().Result;

                if (apiResponse.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    throw new CmiException(string.Format("Error occurred while deleting existing client employment details. API Response: {0}", responseString));
                }
            }
        }
        #endregion
    }
}
