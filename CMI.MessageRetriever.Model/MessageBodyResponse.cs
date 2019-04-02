namespace CMI.MessageRetriever.Model
{
    public class MessageBodyResponse
    {
        public ClientResponse Client { get; set; }
        public ActivityResponse Activity { get; set; }
        public dynamic Details { get; set; }
        public ActionResponse Action { get; set; }
    }
}
