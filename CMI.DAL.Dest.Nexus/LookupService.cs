using CMI.DAL.Dest.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;

namespace CMI.DAL.Dest.Nexus
{
    public class LookupService : ILookupService
    {
        #region Private Member Variables
        DestinationConfig destinationConfig;
        IAuthService authService;
        List<string> _AddressTypes;
        List<CaseLoad> _CaseLoads;
        List<string> _ClientTypes;
        List<string> _ContactTypes;
        List<string> _Ethnicities;
        List<string> _Genders;
        List<string> _Offenses;
        List<SupervisingOfficer> _SupervisingOfficers;
        List<string> _TimeZones;
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
        
        public List<string> Offenses
        {
            get
            {
                if (_Offenses != null)
                {
                    return _Offenses;
                }
                else
                {
                    return GetOffenses();
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
        #endregion

        #region Constructor
        public LookupService(Microsoft.Extensions.Options.IOptions<DestinationConfig> destinationConfig, IAuthService authService)
        {
            this.destinationConfig = destinationConfig.Value;
            this.authService = authService;

        }
        #endregion

        #region Private Helper Methods
        private List<string> GetAddressTypes()
        {
            using (HttpClient apiHost = new HttpClient())
            {
                apiHost.BaseAddress = new Uri(destinationConfig.CaseIntegrationAPIBaseURL);

                apiHost.DefaultRequestHeaders.Accept.Clear();
                apiHost.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.ContentTypeFormatJSON));
                apiHost.DefaultRequestHeaders.Add(Constants.HeaderTypeAuthorization, string.Format("{0} {1}", authService.AuthToken.token_type, authService.AuthToken.access_token));

                var apiResponse = apiHost.GetAsync("api/LookUp/AddressType").Result;

                var responseString = apiResponse.Content.ReadAsStringAsync().Result;

                if (apiResponse.IsSuccessStatusCode)
                {
                    _AddressTypes = apiResponse.Content.ReadAsAsync<List<string>>().Result;
                    Console.WriteLine("{0}Lookup Address Types received:{0}{1}", Environment.NewLine, responseString);
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
                apiHost.BaseAddress = new Uri(destinationConfig.CaseIntegrationAPIBaseURL);

                apiHost.DefaultRequestHeaders.Accept.Clear();
                apiHost.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.ContentTypeFormatJSON));
                apiHost.DefaultRequestHeaders.Add(Constants.HeaderTypeAuthorization, string.Format("{0} {1}", authService.AuthToken.token_type, authService.AuthToken.access_token));

                var apiResponse = apiHost.GetAsync("api/LookUp/Caseloads").Result;

                var responseString = apiResponse.Content.ReadAsStringAsync().Result;

                if (apiResponse.IsSuccessStatusCode)
                {
                    _CaseLoads = apiResponse.Content.ReadAsAsync<List<CaseLoad>>().Result;
                    Console.WriteLine("{0}Lookup Caseloads received:{0}{1}", Environment.NewLine, responseString);
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
                apiHost.BaseAddress = new Uri(destinationConfig.CaseIntegrationAPIBaseURL);

                apiHost.DefaultRequestHeaders.Accept.Clear();
                apiHost.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.ContentTypeFormatJSON));
                apiHost.DefaultRequestHeaders.Add(Constants.HeaderTypeAuthorization, string.Format("{0} {1}", authService.AuthToken.token_type, authService.AuthToken.access_token));

                var apiResponse = apiHost.GetAsync("api/LookUp/ClientType").Result;

                var responseString = apiResponse.Content.ReadAsStringAsync().Result;

                if (apiResponse.IsSuccessStatusCode)
                {
                    _ClientTypes = apiResponse.Content.ReadAsAsync<List<string>>().Result;
                    Console.WriteLine("{0}Lookup Client Types received:{0}{1}", Environment.NewLine, responseString);
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
                apiHost.BaseAddress = new Uri(destinationConfig.CaseIntegrationAPIBaseURL);

                apiHost.DefaultRequestHeaders.Accept.Clear();
                apiHost.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.ContentTypeFormatJSON));
                apiHost.DefaultRequestHeaders.Add(Constants.HeaderTypeAuthorization, string.Format("{0} {1}", authService.AuthToken.token_type, authService.AuthToken.access_token));

                var apiResponse = apiHost.GetAsync("api/LookUp/ContactType").Result;

                var responseString = apiResponse.Content.ReadAsStringAsync().Result;

                if (apiResponse.IsSuccessStatusCode)
                {
                    _ContactTypes = apiResponse.Content.ReadAsAsync<List<string>>().Result;
                    Console.WriteLine("{0}Lookup Contact Types received:{0}{1}", Environment.NewLine, responseString);
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
                apiHost.BaseAddress = new Uri(destinationConfig.CaseIntegrationAPIBaseURL);

                apiHost.DefaultRequestHeaders.Accept.Clear();
                apiHost.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.ContentTypeFormatJSON));
                apiHost.DefaultRequestHeaders.Add(Constants.HeaderTypeAuthorization, string.Format("{0} {1}", authService.AuthToken.token_type, authService.AuthToken.access_token));

                var apiResponse = apiHost.GetAsync("api/LookUp/Ethnicity").Result;

                var responseString = apiResponse.Content.ReadAsStringAsync().Result;

                if (apiResponse.IsSuccessStatusCode)
                {
                    _Ethnicities = apiResponse.Content.ReadAsAsync<List<string>>().Result;
                    Console.WriteLine("{0}Lookup Ethnicities received:{0}{1}", Environment.NewLine, responseString);
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
                apiHost.BaseAddress = new Uri(destinationConfig.CaseIntegrationAPIBaseURL);

                apiHost.DefaultRequestHeaders.Accept.Clear();
                apiHost.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.ContentTypeFormatJSON));
                apiHost.DefaultRequestHeaders.Add(Constants.HeaderTypeAuthorization, string.Format("{0} {1}", authService.AuthToken.token_type, authService.AuthToken.access_token));

                var apiResponse = apiHost.GetAsync("api/LookUp/Gender").Result;

                var responseString = apiResponse.Content.ReadAsStringAsync().Result;

                if (apiResponse.IsSuccessStatusCode)
                {
                    _Genders = apiResponse.Content.ReadAsAsync<List<string>>().Result;
                    Console.WriteLine("{0}Lookup Genders received:{0}{1}", Environment.NewLine, responseString);
                }
                else
                {
                    _Genders = null;
                }
            }


            return _Genders;
        }

        private List<string> GetOffenses()
        {
            using (HttpClient apiHost = new HttpClient())
            {
                apiHost.BaseAddress = new Uri(destinationConfig.CaseIntegrationAPIBaseURL);

                apiHost.DefaultRequestHeaders.Accept.Clear();
                apiHost.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.ContentTypeFormatJSON));
                apiHost.DefaultRequestHeaders.Add(Constants.HeaderTypeAuthorization, string.Format("{0} {1}", authService.AuthToken.token_type, authService.AuthToken.access_token));

                var apiResponse = apiHost.GetAsync("api/LookUp/Offense").Result;

                var responseString = apiResponse.Content.ReadAsStringAsync().Result;

                if (apiResponse.IsSuccessStatusCode)
                {
                    _Offenses = apiResponse.Content.ReadAsAsync<List<string>>().Result;
                    Console.WriteLine("{0}Lookup Offenses received:{0}{1}", Environment.NewLine, responseString);
                }
                else
                {
                    _Offenses = null;
                }
            }


            return _Offenses;
        }

        private List<SupervisingOfficer> GetSupervisingOfficers()
        {
            using (HttpClient apiHost = new HttpClient())
            {
                apiHost.BaseAddress = new Uri(destinationConfig.CaseIntegrationAPIBaseURL);

                apiHost.DefaultRequestHeaders.Accept.Clear();
                apiHost.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.ContentTypeFormatJSON));
                apiHost.DefaultRequestHeaders.Add(Constants.HeaderTypeAuthorization, string.Format("{0} {1}", authService.AuthToken.token_type, authService.AuthToken.access_token));

                var apiResponse = apiHost.GetAsync("api/LookUp/SupervisingOfficers").Result;

                var responseString = apiResponse.Content.ReadAsStringAsync().Result;

                if (apiResponse.IsSuccessStatusCode)
                {
                    _SupervisingOfficers = apiResponse.Content.ReadAsAsync<List<SupervisingOfficer>>().Result;
                    Console.WriteLine("{0}Lookup Supervising Officers received:{0}{1}", Environment.NewLine, responseString);
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
                apiHost.BaseAddress = new Uri(destinationConfig.CaseIntegrationAPIBaseURL);

                apiHost.DefaultRequestHeaders.Accept.Clear();
                apiHost.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.ContentTypeFormatJSON));
                apiHost.DefaultRequestHeaders.Add(Constants.HeaderTypeAuthorization, string.Format("{0} {1}", authService.AuthToken.token_type, authService.AuthToken.access_token));

                var apiResponse = apiHost.GetAsync("api/LookUp/TimeZone").Result;

                var responseString = apiResponse.Content.ReadAsStringAsync().Result;

                if (apiResponse.IsSuccessStatusCode)
                {
                    _TimeZones = apiResponse.Content.ReadAsAsync<List<string>>().Result;
                    Console.WriteLine("{0}Lookup TimeZones received:{0}{1}", Environment.NewLine, responseString);
                }
                else
                {
                    _TimeZones = null;
                }
            }


            return _TimeZones;
        }
        #endregion
    }
}
