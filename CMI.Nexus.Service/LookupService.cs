using CMI.Nexus.Model;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using CMI.Nexus.Interface;

namespace CMI.Nexus.Service
{
    public class LookupService : ILookupService
    {
        #region Private Member Variables
        private readonly NexusConfig nexusConfig;
        private readonly IAuthService authService;
        List<string> _AddressTypes;
        List<CaseLoad> _CaseLoads;
        List<string> _ClientTypes;
        List<string> _ContactTypes;
        List<string> _Ethnicities;
        List<string> _Genders;
        List<SupervisingOfficer> _SupervisingOfficers;
        List<string> _TimeZones;
        List<string> _OffenseCategories;
        #endregion

        #region Public Properties
        public List<string> AddressTypes
        {
            get
            {
                if (_AddressTypes != null)
                {
                    return _AddressTypes;
                }
                else
                {
                    return GetAddressTypes();
                }
            }
        }

        public List<CaseLoad> CaseLoads
        {
            get
            {
                if (_CaseLoads != null)
                {
                    return _CaseLoads;
                }
                else
                {
                    return GetCaseLoads();
                }
            }
        }

        public List<string> ClientTypes
        {
            get
            {
                if (_ClientTypes != null)
                {
                    return _ClientTypes;
                }
                else
                {
                    return GetClientTypes();
                }
            }
        }

        public List<string> ContactTypes
        {
            get
            {
                if (_ContactTypes != null)
                {
                    return _ContactTypes;
                }
                else
                {
                    return GetContactTypes();
                }
            }
        }

        public List<string> Ethnicities
        {
            get
            {
                if (_Ethnicities != null)
                {
                    return _Ethnicities;
                }
                else
                {
                    return GetEthnicities();
                }
            }
        }

        public List<string> Genders
        {
            get
            {
                if (_Genders != null)
                {
                    return _Genders;
                }
                else
                {
                    return GetGenders();
                }
            }
        }

        public List<SupervisingOfficer> SupervisingOfficers
        {
            get
            {
                if (_SupervisingOfficers != null)
                {
                    return _SupervisingOfficers;
                }
                else
                {
                    return GetSupervisingOfficers();
                }
            }
        }

        public List<string> TimeZones
        {
            get
            {
                if (_TimeZones != null)
                {
                    return _TimeZones;
                }
                else
                {
                    return GetTimeZones();
                }
            }
        }

        public List<string> OffenseCategories
        {
            get
            {
                if (_OffenseCategories != null)
                {
                    return _OffenseCategories;
                }
                else
                {
                    return GetOffenseCategories();
                }
            }
        }
        #endregion

        #region Constructor
        public LookupService(
            IOptions<NexusConfig> nexusConfig,
            IAuthService authService
        )
        {
            this.nexusConfig = nexusConfig.Value;
            this.authService = authService;

        }
        #endregion

        #region Private Helper Methods
        private List<string> GetAddressTypes()
        {
            using (HttpClient apiHost = new HttpClient())
            {
                apiHost.BaseAddress = new Uri(nexusConfig.CaseIntegrationApiBaseUrl);

                apiHost.DefaultRequestHeaders.Accept.Clear();
                apiHost.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.ContentTypeFormatJson));
                apiHost.DefaultRequestHeaders.Add(Constants.HeaderTypeAuthorization, string.Format(Constants.AuthTokenFormat, authService.AuthToken.token_type, authService.AuthToken.access_token));

                var apiResponse = apiHost.GetAsync(string.Format("api/{0}/addressTypes", nexusConfig.CaseIntegrationApiVersion)).Result;

                if (apiResponse.IsSuccessStatusCode)
                {
                    _AddressTypes = apiResponse.Content.ReadAsAsync<List<string>>().Result;
                }
                else
                {
                    _AddressTypes = null;
                }
            }

            return _AddressTypes;
        }

        private List<CaseLoad> GetCaseLoads()
        {
            using (HttpClient apiHost = new HttpClient())
            {
                apiHost.BaseAddress = new Uri(nexusConfig.CaseIntegrationApiBaseUrl);

                apiHost.DefaultRequestHeaders.Accept.Clear();
                apiHost.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.ContentTypeFormatJson));
                apiHost.DefaultRequestHeaders.Add(Constants.HeaderTypeAuthorization, string.Format(Constants.AuthTokenFormat, authService.AuthToken.token_type, authService.AuthToken.access_token));

                var apiResponse = apiHost.GetAsync(string.Format("api/{0}/caseloads", nexusConfig.CaseIntegrationApiVersion)).Result;

                if (apiResponse.IsSuccessStatusCode)
                {
                    _CaseLoads = apiResponse.Content.ReadAsAsync<List<CaseLoad>>().Result;
                }
                else
                {
                    _CaseLoads = null;
                }
            }

            return _CaseLoads;
        }

        private List<string> GetClientTypes()
        {
            using (HttpClient apiHost = new HttpClient())
            {
                apiHost.BaseAddress = new Uri(nexusConfig.CaseIntegrationApiBaseUrl);

                apiHost.DefaultRequestHeaders.Accept.Clear();
                apiHost.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.ContentTypeFormatJson));
                apiHost.DefaultRequestHeaders.Add(Constants.HeaderTypeAuthorization, string.Format(Constants.AuthTokenFormat, authService.AuthToken.token_type, authService.AuthToken.access_token));

                var apiResponse = apiHost.GetAsync(string.Format("api/{0}/clientTypes", nexusConfig.CaseIntegrationApiVersion)).Result;

                if (apiResponse.IsSuccessStatusCode)
                {
                    _ClientTypes = apiResponse.Content.ReadAsAsync<List<string>>().Result;
                }
                else
                {
                    _ClientTypes = null;
                }
            }

            return _ClientTypes;
        }

        private List<string> GetContactTypes()
        {
            using (HttpClient apiHost = new HttpClient())
            {
                apiHost.BaseAddress = new Uri(nexusConfig.CaseIntegrationApiBaseUrl);

                apiHost.DefaultRequestHeaders.Accept.Clear();
                apiHost.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.ContentTypeFormatJson));
                apiHost.DefaultRequestHeaders.Add(Constants.HeaderTypeAuthorization, string.Format(Constants.AuthTokenFormat, authService.AuthToken.token_type, authService.AuthToken.access_token));

                var apiResponse = apiHost.GetAsync(string.Format("api/{0}/contactTypes", nexusConfig.CaseIntegrationApiVersion)).Result;

                if (apiResponse.IsSuccessStatusCode)
                {
                    _ContactTypes = apiResponse.Content.ReadAsAsync<List<string>>().Result;
                }
                else
                {
                    _ContactTypes = null;
                }
            }

            return _ContactTypes;
        }

        private List<string> GetEthnicities()
        {
            using (HttpClient apiHost = new HttpClient())
            {
                apiHost.BaseAddress = new Uri(nexusConfig.CaseIntegrationApiBaseUrl);

                apiHost.DefaultRequestHeaders.Accept.Clear();
                apiHost.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.ContentTypeFormatJson));
                apiHost.DefaultRequestHeaders.Add(Constants.HeaderTypeAuthorization, string.Format(Constants.AuthTokenFormat, authService.AuthToken.token_type, authService.AuthToken.access_token));

                var apiResponse = apiHost.GetAsync(string.Format("api/{0}/ethnicities", nexusConfig.CaseIntegrationApiVersion)).Result;

                if (apiResponse.IsSuccessStatusCode)
                {
                    _Ethnicities = apiResponse.Content.ReadAsAsync<List<string>>().Result;
                }
                else
                {
                    _Ethnicities = null;
                }
            }

            return _Ethnicities;
        }

        private List<string> GetGenders()
        {
            using (HttpClient apiHost = new HttpClient())
            {
                apiHost.BaseAddress = new Uri(nexusConfig.CaseIntegrationApiBaseUrl);

                apiHost.DefaultRequestHeaders.Accept.Clear();
                apiHost.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.ContentTypeFormatJson));
                apiHost.DefaultRequestHeaders.Add(Constants.HeaderTypeAuthorization, string.Format(Constants.AuthTokenFormat, authService.AuthToken.token_type, authService.AuthToken.access_token));

                var apiResponse = apiHost.GetAsync(string.Format("api/{0}/genders", nexusConfig.CaseIntegrationApiVersion)).Result;

                if (apiResponse.IsSuccessStatusCode)
                {
                    _Genders = apiResponse.Content.ReadAsAsync<List<string>>().Result;
                }
                else
                {
                    _Genders = null;
                }
            }

            return _Genders;
        }

        private List<SupervisingOfficer> GetSupervisingOfficers()
        {
            using (HttpClient apiHost = new HttpClient())
            {
                apiHost.BaseAddress = new Uri(nexusConfig.CaseIntegrationApiBaseUrl);

                apiHost.DefaultRequestHeaders.Accept.Clear();
                apiHost.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.ContentTypeFormatJson));
                apiHost.DefaultRequestHeaders.Add(Constants.HeaderTypeAuthorization, string.Format(Constants.AuthTokenFormat, authService.AuthToken.token_type, authService.AuthToken.access_token));

                var apiResponse = apiHost.GetAsync(string.Format("api/{0}/supervisingOfficers", nexusConfig.CaseIntegrationApiVersion)).Result;

                if (apiResponse.IsSuccessStatusCode)
                {
                    _SupervisingOfficers = apiResponse.Content.ReadAsAsync<List<SupervisingOfficer>>().Result;
                }
                else
                {
                    _SupervisingOfficers = null;
                }
            }

            return _SupervisingOfficers;
        }

        private List<string> GetTimeZones()
        {
            using (HttpClient apiHost = new HttpClient())
            {
                apiHost.BaseAddress = new Uri(nexusConfig.CaseIntegrationApiBaseUrl);

                apiHost.DefaultRequestHeaders.Accept.Clear();
                apiHost.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.ContentTypeFormatJson));
                apiHost.DefaultRequestHeaders.Add(Constants.HeaderTypeAuthorization, string.Format(Constants.AuthTokenFormat, authService.AuthToken.token_type, authService.AuthToken.access_token));

                var apiResponse = apiHost.GetAsync(string.Format("api/{0}/timeZones", nexusConfig.CaseIntegrationApiVersion)).Result;

                if (apiResponse.IsSuccessStatusCode)
                {
                    _TimeZones = apiResponse.Content.ReadAsAsync<List<string>>().Result;
                }
                else
                {
                    _TimeZones = null;
                }
            }

            return _TimeZones;
        }

        private List<string> GetOffenseCategories()
        {
            using (HttpClient apiHost = new HttpClient())
            {
                apiHost.BaseAddress = new Uri(nexusConfig.CaseIntegrationApiBaseUrl);

                apiHost.DefaultRequestHeaders.Accept.Clear();
                apiHost.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.ContentTypeFormatJson));
                apiHost.DefaultRequestHeaders.Add(Constants.HeaderTypeAuthorization, string.Format(Constants.AuthTokenFormat, authService.AuthToken.token_type, authService.AuthToken.access_token));

                var apiResponse = apiHost.GetAsync(string.Format("api/{0}/offenseCategories", nexusConfig.CaseIntegrationApiVersion)).Result;

                if (apiResponse.IsSuccessStatusCode)
                {
                    _OffenseCategories = apiResponse.Content.ReadAsAsync<List<string>>().Result;
                }
                else
                {
                    _OffenseCategories = null;
                }
            }

            return _OffenseCategories;
        }
        #endregion
    }
}
