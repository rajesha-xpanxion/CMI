using CMI.MessageRetriever.Interface;
using CMI.MessageRetriever.Model;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace CMI.MessageRetriever.REST
{
    public class MessageRetrieverService : IMessageRetrieverService
    {
        #region Private Member Variables
        private readonly MessageRetrieverConfig messageRetrieverConfig;
        private string token;
        #endregion

        #region Constructor
        public MessageRetrieverService(
            IConfiguration configuration
        )
        {
            this.messageRetrieverConfig = configuration.GetSection(ConfigKeys.MessageRetrieverConfig).Get<MessageRetrieverConfig>();
        }
        #endregion


        private bool readAndDelete = true;

        #region Public Methods
        public async Task<IEnumerable<MessageBodyResponse>> Execute()
        {
            Console.WriteLine("{0} -> Initiating outbound message retrieving process using REST protocol...{1}", DateTime.Now, Environment.NewLine);

            //get token
            if (this.messageRetrieverConfig.UseSas)
            {
                this.token = GetSasToken();
            }
            else
            {
                this.token = await GetAcsToken();
            }

            var messages = await ReceiveMessagesAsync();

            Console.WriteLine("{0}{1} -> Outbound message retrieving process using REST protocol completed successfully...", Environment.NewLine, DateTime.Now);

            return messages;
        }
        #endregion

        #region Private Helper Methods
        private string GetSasToken()
        {
            // Set token lifetime to 20 minutes. 
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            TimeSpan diff = DateTime.Now.ToUniversalTime() - origin;
            uint tokenExpirationTime = Convert.ToUInt32(diff.TotalSeconds) + 20 * 60;

            string stringToSign = $"{HttpUtility.UrlEncode(this.messageRetrieverConfig.ServiceBusNamespace)}\n{tokenExpirationTime}";
            HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(this.messageRetrieverConfig.SharedAccessKey));

            string signature = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(stringToSign)));

            return String.Format(
                CultureInfo.InvariantCulture,
                "SharedAccessSignature sr={0}&sig={1}&se={2}&skn={3}",
                HttpUtility.UrlEncode(this.messageRetrieverConfig.ServiceBusNamespace),
                HttpUtility.UrlEncode(signature),
                tokenExpirationTime,
                this.messageRetrieverConfig.SharedAccessKeyName
            );
        }

        private async Task<string> GetAcsToken()
        {
            using (HttpClient httpClient = new HttpClient())
            {
                var postData = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("wrap_name", this.messageRetrieverConfig.SharedAccessKeyName),
                    new KeyValuePair<string, string>("wrap_password", this.messageRetrieverConfig.SharedAccessKey),
                    new KeyValuePair<string, string>("wrap_scope", $"http://{this.messageRetrieverConfig.ServiceBusNamespace}.servicebus.windows.net/")
                };

                HttpContent postContent = new FormUrlEncodedContent(postData);

                var response = await httpClient.PostAsync($"https://{this.messageRetrieverConfig.ServiceBusNamespace}-sb.accesscontrol.windows.net/WRAPv0.9/", postContent);

                string responseBody = await response.Content.ReadAsStringAsync();

                //check if request successful
                if (response.IsSuccessStatusCode)
                {
                    var responseProperties = responseBody.Split('&');
                    var tokenProperty = responseProperties[0].Split('=');

                    return $"WRAP access_token=\"{Uri.UnescapeDataString(tokenProperty[1])}\"";
                }
                else
                {
                    Console.WriteLine($"GetAcsToken failed. Response: {responseBody}");

                    return null;
                }
            }
        }

        private async Task<List<MessageBodyResponse>> ReceiveMessagesAsync()
        {
            List<MessageBodyResponse> convertedMessages = new List<MessageBodyResponse>();
            ServiceBusHttpMessage retrievedMessage = null;

            do
            {
                // Receive the message
                retrievedMessage = await RetrieveMessage();

                if (retrievedMessage != null)
                {
                    convertedMessages.Add(ProcessMessage(retrievedMessage));
                }
            } while (retrievedMessage != null);

            return convertedMessages;
        }

        private async Task<ServiceBusHttpMessage> RetrieveMessage()
        {

            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add(Constants.HttpRequestHeaderTypeAuthorization, this.token);
                httpClient.DefaultRequestHeaders.Add(Constants.HttpRequestHeaderTypeContentType, Constants.ContentTypeAtomXml);

                string requestUri = $"https://{this.messageRetrieverConfig.ServiceBusNamespace}.servicebus.windows.net/{this.messageRetrieverConfig.TopicName}/Subscriptions/{this.messageRetrieverConfig.SubscriptionName}/messages/head?timeout=30";

                HttpResponseMessage response = null;
                if (readAndDelete)
                {
                    response = await httpClient.DeleteAsync(requestUri);
                }
                else
                {
                    response = await httpClient.PostAsync(requestUri, new ByteArrayContent(new Byte[0]));
                }

                //check if request successful
                if (response.IsSuccessStatusCode)
                {
                    // Check if a message was returned. 
                    HttpResponseHeaders headers = response.Headers;
                    if (headers.Contains(Constants.HttpResponseHeaderTypeBrokerProperties))
                    {
                        // Get message body. 
                        return new ServiceBusHttpMessage
                        {
                            body = await response.Content.ReadAsByteArrayAsync()
                        };
                    }
                }
            }

            return null;
        }

        private MessageBodyResponse ProcessMessage(ServiceBusHttpMessage message)
        {
            // Process the message.
            string messageBody = Encoding.UTF8.GetString(message.body); // Convert byte array into string. 

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"{Environment.NewLine}{DateTime.Now} -> Received message: SequenceNumber:{message.brokerProperties.SequenceNumber} Body:{messageBody}");
            Console.ForegroundColor = ConsoleColor.Gray;

            return JsonConvert.DeserializeObject<MessageBodyResponse>(messageBody);
        }
        #endregion
    }
}
