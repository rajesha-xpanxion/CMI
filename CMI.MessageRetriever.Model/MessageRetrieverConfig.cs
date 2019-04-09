

namespace CMI.MessageRetriever.Model
{
    public class MessageRetrieverConfig
    {
        public bool IsDevMode { get; set; }
        public string TestDataJsonRepoPath { get; set; }
        public string RetrieverTypeToExecute { get; set; }
        public string ServiceBusConnectionString { get; set; }
        public string TopicName { get; set; }
        public string SubscriptionName { get; set; }
        public bool ReadAndDelete { get; set; }
        public string ServiceBusNamespace { get; set; }
        public string SharedAccessKeyName { get; set; }
        public string SharedAccessKey { get; set; }
        public bool UseSas { get; set; }
    }
}
