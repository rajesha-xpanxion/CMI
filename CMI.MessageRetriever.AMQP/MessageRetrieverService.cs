using CMI.MessageRetriever.Interface;
using CMI.MessageRetriever.Model;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace CMI.MessageRetriever.AMQP
{
    public class MessageRetrieverService : IMessageRetrieverService
    {
        #region Private Member Variables
        private readonly IMessageReceiver messageReceiver;
        private readonly MessageRetrieverConfig messageRetrieverConfig;
        #endregion

        #region Constructor
        public MessageRetrieverService(
            IConfiguration configuration
        )
        {
            
            this.messageRetrieverConfig = configuration.GetSection(ConfigKeys.MessageRetrieverConfig).Get<MessageRetrieverConfig>();

            ServiceBusConnectionStringBuilder serviceBusConnectionStringBuilder = new ServiceBusConnectionStringBuilder(this.messageRetrieverConfig.ServiceBusConnectionString);
            ServiceBusConnection serviceBusConnection = new ServiceBusConnection(serviceBusConnectionStringBuilder);

            string entityPath = EntityNameHelper.FormatSubscriptionPath(this.messageRetrieverConfig.TopicName, this.messageRetrieverConfig.SubscriptionName);

            messageReceiver = new MessageReceiver(serviceBusConnection, entityPath);
        }
        #endregion

        #region Public Methods
        public async Task<IEnumerable<MessageBodyResponse>> Execute()
        {
            Console.WriteLine("{0} -> Initiating outbound message retrieving process using AMQP protocol...{1}", DateTime.Now, Environment.NewLine);

            if (this.messageRetrieverConfig.IsDevMode)
            {
                //test data
                string testDataJsonFileName = Path.Combine(messageRetrieverConfig.TestDataJsonFileFullPath, Constants.TestDataJsonFileNameAllOutboundMessages);

                return File.Exists(testDataJsonFileName)
                    ? JsonConvert.DeserializeObject<IEnumerable<MessageBodyResponse>>(File.ReadAllText(testDataJsonFileName))
                    : new List<MessageBodyResponse>();
            }

            var messages = await ReceiveMessagesAsync();

            await messageReceiver.CloseAsync();

            Console.WriteLine("{0}{1} -> Outbound message retrieving process using AMQP protocol completed successfully...", Environment.NewLine, DateTime.Now);

            return messages;
        }
        #endregion

        #region Private Helper Methods
        private async Task<List<MessageBodyResponse>> ReceiveMessagesAsync()
        {
            List<MessageBodyResponse> convertedMessages = new List<MessageBodyResponse>();
            Message retrievedMessage = null;

            do
            {
                // Receive the message
                retrievedMessage = await messageReceiver.ReceiveAsync();

                if (retrievedMessage != null)
                {
                    //process received message and transform into required format
                    convertedMessages.Add(ProcessMessage(retrievedMessage));

                    // Complete the message so that it is not received again.
                    // This can be done only if the MessageReceiver is created in ReceiveMode.PeekLock mode (which is default).
                    if (this.messageRetrieverConfig.ReadAndDelete)
                    {
                        await messageReceiver.CompleteAsync(retrievedMessage.SystemProperties.LockToken);
                    }
                }
            } while (retrievedMessage != null);


            return convertedMessages;
        }

        private MessageBodyResponse ProcessMessage(Message message)
        {
            // Process the message.
            string messageBody = Encoding.UTF8.GetString(message.Body); // Convert byte array into string. 

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"{Environment.NewLine}{DateTime.Now} -> Received message: SequenceNumber:{message.SystemProperties.SequenceNumber} Body:{messageBody}");
            Console.ForegroundColor = ConsoleColor.Gray;

            return JsonConvert.DeserializeObject<MessageBodyResponse>(messageBody);
        }
        #endregion
    }
}
