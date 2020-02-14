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
    public class NoteService : INoteService
    {
        #region Private Member Variables
        private readonly NexusConfig nexusConfig;
        private readonly IAuthService authService;
        #endregion

        #region Constructor
        public NoteService(
            IOptions<NexusConfig> nexusConfig, 
            IAuthService authService
        )
        {
            this.nexusConfig = nexusConfig.Value;
            this.authService = authService;
        }
        #endregion

        #region Public Methods
        public bool AddNewNoteDetails(Note note)
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

                var apiResponse = apiHost.PostAsJsonAsync<Note>(string.Format("api/{0}/clients/{1}/notes", nexusConfig.CaseIntegrationApiVersion, note.ClientId), note).Result;
                var responseString = apiResponse.Content.ReadAsStringAsync().Result;

                if (apiResponse.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    throw new CmiException(string.Format("Error occurred while adding new note details. API Response: {0}", responseString));
                }
            }
        }

        public Note GetNoteDetails(string clientId, string noteId)
        {
            if (nexusConfig.IsDevMode)
            {
                return GetAllNoteDetails(clientId).Where(a => a.NoteId.Equals(noteId, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            }

            Note noteDetails = null;

            using (HttpClient apiHost = new HttpClient())
            {
                apiHost.BaseAddress = new Uri(nexusConfig.CaseIntegrationApiBaseUrl);

                apiHost.DefaultRequestHeaders.Accept.Clear();
                apiHost.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.ContentTypeFormatJson));
                apiHost.DefaultRequestHeaders.Add(Constants.HeaderTypeAuthorization, string.Format("{0} {1}", authService.AuthToken.token_type, authService.AuthToken.access_token));

                var apiResponse = apiHost.GetAsync(string.Format("api/{0}/clients/{1}/notes/{2}", nexusConfig.CaseIntegrationApiVersion, clientId, noteId)).Result;

                var responseString = apiResponse.Content.ReadAsStringAsync().Result;

                if (apiResponse.IsSuccessStatusCode)
                {
                    noteDetails = apiResponse.Content.ReadAsAsync<Note>().Result;
                }
                else
                {
                    noteDetails = null;
                }
            }


            return noteDetails;
        }

        public List<Note> GetAllNoteDetails(string clientId)
        {
            if (nexusConfig.IsDevMode)
            {
                //test data
                string testDataJsonFileName = Path.Combine(nexusConfig.TestDataJsonRepoPath, TestDataJsonFileName.AllClientNoteDetails);

                return File.Exists(testDataJsonFileName)
                    ? JsonConvert.DeserializeObject<List<Note>>(File.ReadAllText(testDataJsonFileName)).Where(c => c.ClientId.Equals(clientId, StringComparison.InvariantCultureIgnoreCase)).ToList()
                    : new List<Note>();
            }
            else
            {
                List<Note> allNoteDetails = null;

                using (HttpClient apiHost = new HttpClient())
                {
                    apiHost.BaseAddress = new Uri(nexusConfig.CaseIntegrationApiBaseUrl);

                    apiHost.DefaultRequestHeaders.Accept.Clear();
                    apiHost.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.ContentTypeFormatJson));
                    apiHost.DefaultRequestHeaders.Add(Constants.HeaderTypeAuthorization, string.Format("{0} {1}", authService.AuthToken.token_type, authService.AuthToken.access_token));

                    var apiResponse = apiHost.GetAsync(string.Format("api/{0}/clients/{1}/notes", nexusConfig.CaseIntegrationApiVersion, clientId)).Result;

                    if (apiResponse.IsSuccessStatusCode)
                    {
                        allNoteDetails = apiResponse.Content.ReadAsAsync<List<Note>>().Result;
                    }
                    else
                    {
                        allNoteDetails = null;
                    }
                }

                return allNoteDetails;
            }
        }

        public bool UpdateNoteDetails(Note note)
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

                var apiResponse = apiHost.PutAsJsonAsync<Note>(string.Format("api/{0}/clients/{1}/notes", nexusConfig.CaseIntegrationApiVersion, note.ClientId), note).Result;

                var responseString = apiResponse.Content.ReadAsStringAsync().Result;

                if (apiResponse.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    throw new CmiException(string.Format("Error occurred while updating existing note details. API Response: {0}", responseString));
                }
            }
        }

        public bool DeleteNoteDetails(string clientId, string noteId)
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

                var apiResponse = apiHost.DeleteAsync(string.Format("api/{0}/clients/{1}/notes/{2}", nexusConfig.CaseIntegrationApiVersion, clientId, noteId)).Result;

                var responseString = apiResponse.Content.ReadAsStringAsync().Result;

                if (apiResponse.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    throw new CmiException(string.Format("Error occurred while deleting existing note details. API Response: {0}", responseString));
                }
            }
        }
        #endregion
    }
}
