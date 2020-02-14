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
    public class VehicleService : IVehicleService
    {
        #region Private Member Variables
        private readonly NexusConfig nexusConfig;
        private readonly IAuthService authService;
        #endregion

        #region Constructor
        public VehicleService(
            IOptions<NexusConfig> nexusConfig,
            IAuthService authService
        )
        {
            this.nexusConfig = nexusConfig.Value;
            this.authService = authService;
        }
        #endregion

        #region Public Methods
        public bool AddNewVehicleDetails(Vehicle vehicle)
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

                var apiResponse = apiHost.PostAsJsonAsync<Vehicle>(string.Format("api/{0}/clients/{1}/vehicles", nexusConfig.CaseIntegrationApiVersion, vehicle.ClientId), vehicle).Result;
                var responseString = apiResponse.Content.ReadAsStringAsync().Result;

                if (apiResponse.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    throw new CmiException(string.Format("Error occurred while adding new client vehicle details. API Response: {0}", responseString));
                }
            }
        }

        public Vehicle GetVehicleDetails(string clientId, string vehicleId)
        {
            if (nexusConfig.IsDevMode)
            {
                return GetAllVehicleDetails(clientId).Where(a => a.VehicleId.Equals(vehicleId, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            }

            Vehicle vehicleDetails = null;

            using (HttpClient apiHost = new HttpClient())
            {
                apiHost.BaseAddress = new Uri(nexusConfig.CaseIntegrationApiBaseUrl);

                apiHost.DefaultRequestHeaders.Accept.Clear();
                apiHost.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.ContentTypeFormatJson));
                apiHost.DefaultRequestHeaders.Add(Constants.HeaderTypeAuthorization, string.Format("{0} {1}", authService.AuthToken.token_type, authService.AuthToken.access_token));

                var apiResponse = apiHost.GetAsync(string.Format("api/{0}/clients/{1}/vehicles/{2}", nexusConfig.CaseIntegrationApiVersion, clientId, vehicleId)).Result;

                if (apiResponse.IsSuccessStatusCode)
                {
                    vehicleDetails = apiResponse.Content.ReadAsAsync<Vehicle>().Result;
                }
                else
                {
                    vehicleDetails = null;
                }
            }

            return vehicleDetails;
        }

        public List<Vehicle> GetAllVehicleDetails(string clientId)
        {
            if (nexusConfig.IsDevMode)
            {
                //test data
                string testDataJsonFileName = Path.Combine(nexusConfig.TestDataJsonRepoPath, TestDataJsonFileName.AllClientVehicleDetails);

                return File.Exists(testDataJsonFileName)
                    ? JsonConvert.DeserializeObject<List<Vehicle>>(File.ReadAllText(testDataJsonFileName)).Where(c => c.ClientId.Equals(clientId, StringComparison.InvariantCultureIgnoreCase)).ToList()
                    : new List<Vehicle>();
            }
            else
            {
                List<Vehicle> allVehicleDetails = null;

                using (HttpClient apiHost = new HttpClient())
                {
                    apiHost.BaseAddress = new Uri(nexusConfig.CaseIntegrationApiBaseUrl);

                    apiHost.DefaultRequestHeaders.Accept.Clear();
                    apiHost.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.ContentTypeFormatJson));
                    apiHost.DefaultRequestHeaders.Add(Constants.HeaderTypeAuthorization, string.Format("{0} {1}", authService.AuthToken.token_type, authService.AuthToken.access_token));

                    var apiResponse = apiHost.GetAsync(string.Format("api/{0}/clients/{1}/vehicles", nexusConfig.CaseIntegrationApiVersion, clientId)).Result;

                    if (apiResponse.IsSuccessStatusCode)
                    {
                        allVehicleDetails = apiResponse.Content.ReadAsAsync<List<Vehicle>>().Result;
                    }
                    else
                    {
                        allVehicleDetails = null;
                    }
                }

                return allVehicleDetails;
            }
        }

        public bool UpdateVehicleDetails(Vehicle vehicle)
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

                var apiResponse = apiHost.PutAsJsonAsync<Vehicle>(string.Format("api/{0}/clients/{1}/vehicles", nexusConfig.CaseIntegrationApiVersion, vehicle.ClientId), vehicle).Result;

                var responseString = apiResponse.Content.ReadAsStringAsync().Result;

                if (apiResponse.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    throw new CmiException(string.Format("Error occurred while updating existing client vehicle details. API Response: {0}", responseString));
                }
            }
        }

        public bool DeleteVehicleDetails(string clientId, string vehicleId)
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

                var apiResponse = apiHost.DeleteAsync(string.Format("api/{0}/clients/{1}/vehicles/{2}", nexusConfig.CaseIntegrationApiVersion, clientId, vehicleId)).Result;

                var responseString = apiResponse.Content.ReadAsStringAsync().Result;

                if (apiResponse.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    throw new CmiException(string.Format("Error occurred while deleting existing client vehicle details. API Response: {0}", responseString));
                }
            }
        }
        #endregion
    }
}
