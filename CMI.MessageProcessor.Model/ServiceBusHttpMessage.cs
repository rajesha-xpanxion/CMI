using System.Collections.Specialized;

namespace CMI.MessageProcessor.Model
{
    public class ServiceBusHttpMessage
    {
        public byte[] body { get; set; }

        public string location { get; set; }

        public BrokerProperties brokerProperties { get; set; }
        public NameValueCollection customProperties { get; set; }

        public ServiceBusHttpMessage()
        {
            brokerProperties = new BrokerProperties();
            customProperties = new NameValueCollection();
        }
    }
}