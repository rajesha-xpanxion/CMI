using CMI.DAL.Dest.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace CMI.DAL.Dest.Nexus
{
    public class NoteService : INoteService
    {
        DestinationConfig destinationConfig;
        IAuthService authService;

        public NoteService(Microsoft.Extensions.Options.IOptions<DestinationConfig> destinationConfig, IAuthService authService)
        {
            this.destinationConfig = destinationConfig.Value;
            this.authService = authService;
        }

        public bool AddNewNoteDetails(Note note)
        {
            using (HttpClient apiHost = new HttpClient())
            {
                apiHost.BaseAddress = new Uri(destinationConfig.CaseIntegrationAPIBaseURL);

                apiHost.DefaultRequestHeaders.Accept.Clear();
                apiHost.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.ContentTypeFormatJSON));
                apiHost.DefaultRequestHeaders.Add(Constants.HeaderTypeAuthorization, string.Format("{0} {1}", authService.AuthToken.token_type, authService.AuthToken.access_token));

                var apiResponse = apiHost.PostAsJsonAsync<Note>(string.Format("api/v1/clients/{0}/notes", note.ClientId), note).Result;
                var responseString = apiResponse.Content.ReadAsStringAsync().Result;

                if (apiResponse.IsSuccessStatusCode)
                {
                    return true;
                    //Console.WriteLine("New note details added successfully.{0}Response: {1}", Environment.NewLine, responseString);
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
                apiHost.BaseAddress = new Uri(destinationConfig.CaseIntegrationAPIBaseURL);

                apiHost.DefaultRequestHeaders.Accept.Clear();
                apiHost.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.ContentTypeFormatJSON));
                apiHost.DefaultRequestHeaders.Add(Constants.HeaderTypeAuthorization, string.Format("{0} {1}", authService.AuthToken.token_type, authService.AuthToken.access_token));

                var apiResponse = apiHost.GetAsync(string.Format("api/v1/clients/{0}/notes/{1}", clientId, noteId)).Result;

                var responseString = apiResponse.Content.ReadAsStringAsync().Result;

                if (apiResponse.IsSuccessStatusCode)
                {
                    noteDetails = apiResponse.Content.ReadAsAsync<Note>().Result;
                    Console.WriteLine("{0}Note details received:{0}{1}", Environment.NewLine, responseString);
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
                apiHost.BaseAddress = new Uri(destinationConfig.CaseIntegrationAPIBaseURL);

                apiHost.DefaultRequestHeaders.Accept.Clear();
                apiHost.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.ContentTypeFormatJSON));
                apiHost.DefaultRequestHeaders.Add(Constants.HeaderTypeAuthorization, string.Format("{0} {1}", authService.AuthToken.token_type, authService.AuthToken.access_token));

                var apiResponse = apiHost.PutAsJsonAsync<Note>(string.Format("api/v1/clients/{0}/notes", note.ClientId), note).Result;

                var responseString = apiResponse.Content.ReadAsStringAsync().Result;

                if (apiResponse.IsSuccessStatusCode)
                {
                    return true;
                    //Console.WriteLine("Existing note details updated successfully.{0}Response: {1}", Environment.NewLine, responseString);
                }
                else
                {
                    throw new ApplicationException(string.Format("Error occurred while updating existing note details. API Response: {0}", responseString));
                }
            }
        }
    }
}
