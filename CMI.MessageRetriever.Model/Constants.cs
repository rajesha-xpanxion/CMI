namespace CMI.MessageRetriever
{
    public static class Constants
    {
        /// <summary>
        /// Http request header type of "Authorization"
        /// </summary>
        public static string HttpRequestHeaderTypeAuthorization { get { return "Authorization"; } }

        /// <summary>
        /// Http request header type of "ContentType"
        /// </summary>
        public static string HttpRequestHeaderTypeContentType { get { return "ContentType"; } }

        /// <summary>
        /// Represents content type - "application/atom+xml;type=entry;charset=utf-8"
        /// </summary>
        public static string ContentTypeAtomXml { get { return "application/atom+xml;type=entry;charset=utf-8"; } }

        /// <summary>
        /// Http response header type of "BrokerProperties"
        /// </summary>
        public static string HttpResponseHeaderTypeBrokerProperties { get { return "BrokerProperties"; } }
    }

    public static class ConfigKeys
    {
        public static string MessageRetrieverConfig { get { return "ProcessorConfig:OutboundProcessorConfig:MessageRetrieverConfig"; } }
    }
}
