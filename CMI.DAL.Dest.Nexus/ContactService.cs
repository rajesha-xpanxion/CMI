﻿using CMI.DAL.Dest.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace CMI.DAL.Dest.Nexus
{
    public class ContactService : IContactService
    {
        DestinationConfig destinationConfig;
        IAuthService authService;

        public ContactService(Microsoft.Extensions.Options.IOptions<DestinationConfig> destinationConfig, IAuthService authService)
        {
            this.destinationConfig = destinationConfig.Value;
            this.authService = authService;
        }

        public bool AddNewContactDetails(Contact contact)
        {
            using (HttpClient apiHost = new HttpClient())
            {
                apiHost.BaseAddress = new Uri(destinationConfig.CaseIntegrationAPIBaseURL);

                apiHost.DefaultRequestHeaders.Accept.Clear();
                apiHost.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.ContentTypeFormatJSON));
                apiHost.DefaultRequestHeaders.Add(Constants.HeaderTypeAuthorization, string.Format("{0} {1}", authService.AuthToken.token_type, authService.AuthToken.access_token));

                var apiResponse = apiHost.PostAsJsonAsync<Contact>(string.Format("api/{0}/clients/{1}/contacts", destinationConfig.CaseIntegrationAPIVersion, contact.ClientId), contact).Result;
                var responseString = apiResponse.Content.ReadAsStringAsync().Result;

                if (apiResponse.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    throw new ApplicationException(string.Format("Error occurred while adding new client contact details. API Response: {0}", responseString));
                }
            }
        }

        public Contact GetContactDetails(string clientId, string contactId)
        {
            Contact contactDetails = null;

            using (HttpClient apiHost = new HttpClient())
            {
                apiHost.BaseAddress = new Uri(destinationConfig.CaseIntegrationAPIBaseURL);

                apiHost.DefaultRequestHeaders.Accept.Clear();
                apiHost.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.ContentTypeFormatJSON));
                apiHost.DefaultRequestHeaders.Add(Constants.HeaderTypeAuthorization, string.Format("{0} {1}", authService.AuthToken.token_type, authService.AuthToken.access_token));

                var apiResponse = apiHost.GetAsync(string.Format("api/{0}/clients/{1}/contacts/{2}", destinationConfig.CaseIntegrationAPIVersion, clientId, contactId)).Result;

                var responseString = apiResponse.Content.ReadAsStringAsync().Result;

                if (apiResponse.IsSuccessStatusCode)
                {
                    contactDetails = apiResponse.Content.ReadAsAsync<Contact>().Result;
                    Console.WriteLine("{0}Client contact details received:{0}{1}", Environment.NewLine, responseString);
                }
                else
                {
                    contactDetails = null;
                }
            }


            return contactDetails;
        }

        public bool UpdateContactDetails(Contact contact)
        {
            using (HttpClient apiHost = new HttpClient())
            {
                apiHost.BaseAddress = new Uri(destinationConfig.CaseIntegrationAPIBaseURL);

                apiHost.DefaultRequestHeaders.Accept.Clear();
                apiHost.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.ContentTypeFormatJSON));
                apiHost.DefaultRequestHeaders.Add(Constants.HeaderTypeAuthorization, string.Format("{0} {1}", authService.AuthToken.token_type, authService.AuthToken.access_token));

                var apiResponse = apiHost.PutAsJsonAsync<Contact>(string.Format("api/{0}/clients/{1}/contacts", destinationConfig.CaseIntegrationAPIVersion, contact.ClientId), contact).Result;

                var responseString = apiResponse.Content.ReadAsStringAsync().Result;

                if (apiResponse.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    throw new ApplicationException(string.Format("Error occurred while updating existing client contact details. API Response: {0}", responseString));
                }
            }
        }

        public bool DeleteContactDetails(string clientId, string contactId)
        {
            using (HttpClient apiHost = new HttpClient())
            {
                apiHost.BaseAddress = new Uri(destinationConfig.CaseIntegrationAPIBaseURL);

                apiHost.DefaultRequestHeaders.Accept.Clear();
                apiHost.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.ContentTypeFormatJSON));
                apiHost.DefaultRequestHeaders.Add(Constants.HeaderTypeAuthorization, string.Format("{0} {1}", authService.AuthToken.token_type, authService.AuthToken.access_token));

                var apiResponse = apiHost.DeleteAsync(string.Format("api/{0}/clients/{1}/contacts/{2}", destinationConfig.CaseIntegrationAPIVersion, clientId, contactId)).Result;

                var responseString = apiResponse.Content.ReadAsStringAsync().Result;

                if (apiResponse.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    throw new ApplicationException(string.Format("Error occurred while deleting existing client contact details. API Response: {0}", responseString));
                }
            }
        }
    }
}