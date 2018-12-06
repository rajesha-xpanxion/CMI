using CMI.DAL.Dest.Models;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Extensions.Options;

namespace CMI.DAL.Dest.Nexus
{
    public class NoteService : INoteService
    {
        #region Private Member Variables
        private readonly DestinationConfig destinationConfig;
        private readonly IAuthService authService;
        #endregion

        #region Constructor
        public NoteService(
            IOptions<DestinationConfig> destinationConfig, 
            IAuthService authService
        )
        {
            this.destinationConfig = destinationConfig.Value;
            this.authService = authService;
        }
        #endregion

        #region Public Methods
        public bool AddNewNoteDetails(Note note)
        {
            using (HttpClient apiHost = new HttpClient())
            {
                apiHost.BaseAddress = new Uri(destinationConfig.CaseIntegrationApiBaseUrl);

                apiHost.DefaultRequestHeaders.Accept.Clear();
                apiHost.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.ContentTypeFormatJson));
                apiHost.DefaultRequestHeaders.Add(Constants.HeaderTypeAuthorization, string.Format("{0} {1}", authService.AuthToken.token_type, authService.AuthToken.access_token));

                var apiResponse = apiHost.PostAsJsonAsync<Note>(string.Format("api/{0}/clients/{1}/notes", destinationConfig.CaseIntegrationApiVersion, note.ClientId), note).Result;
                var responseString = apiResponse.Content.ReadAsStringAsync().Result;

                if (apiResponse.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    throw new ApplicationException(string.Format("Error occurred while adding new note details. API Response: {0}", responseString));
                }
            }
        }

        public Note GetNoteDetails(string clientId, string noteId)
        {
            Note noteDetails = null;

            using (HttpClient apiHost = new HttpClient())
            {
                apiHost.BaseAddress = new Uri(destinationConfig.CaseIntegrationApiBaseUrl);

                apiHost.DefaultRequestHeaders.Accept.Clear();
                apiHost.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.ContentTypeFormatJson));
                apiHost.DefaultRequestHeaders.Add(Constants.HeaderTypeAuthorization, string.Format("{0} {1}", authService.AuthToken.token_type, authService.AuthToken.access_token));

                var apiResponse = apiHost.GetAsync(string.Format("api/{0}/clients/{1}/notes/{2}", destinationConfig.CaseIntegrationApiVersion, clientId, noteId)).Result;

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

        public bool UpdateNoteDetails(Note note)
        {
            using (HttpClient apiHost = new HttpClient())
            {
                apiHost.BaseAddress = new Uri(destinationConfig.CaseIntegrationApiBaseUrl);

                apiHost.DefaultRequestHeaders.Accept.Clear();
                apiHost.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.ContentTypeFormatJson));
                apiHost.DefaultRequestHeaders.Add(Constants.HeaderTypeAuthorization, string.Format("{0} {1}", authService.AuthToken.token_type, authService.AuthToken.access_token));

                var apiResponse = apiHost.PutAsJsonAsync<Note>(string.Format("api/{0}/clients/{1}/notes", destinationConfig.CaseIntegrationApiVersion, note.ClientId), note).Result;

                var responseString = apiResponse.Content.ReadAsStringAsync().Result;

                if (apiResponse.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    throw new ApplicationException(string.Format("Error occurred while updating existing note details. API Response: {0}", responseString));
                }
            }
        }

        public bool DeleteNoteDetails(string clientId, string noteId)
        {
            using (HttpClient apiHost = new HttpClient())
            {
                apiHost.BaseAddress = new Uri(destinationConfig.CaseIntegrationApiBaseUrl);

                apiHost.DefaultRequestHeaders.Accept.Clear();
                apiHost.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.ContentTypeFormatJson));
                apiHost.DefaultRequestHeaders.Add(Constants.HeaderTypeAuthorization, string.Format("{0} {1}", authService.AuthToken.token_type, authService.AuthToken.access_token));

                var apiResponse = apiHost.DeleteAsync(string.Format("api/{0}/clients/{1}/notes/{2}", destinationConfig.CaseIntegrationApiVersion, clientId, noteId)).Result;

                var responseString = apiResponse.Content.ReadAsStringAsync().Result;

                if (apiResponse.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    throw new ApplicationException(string.Format("Error occurred while deleting existing note details. API Response: {0}", responseString));
                }
            }
        }
        #endregion
    }
}
