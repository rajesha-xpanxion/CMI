namespace CMI.MessageProcessor.Model
{
    public class MessageBodyResponse
    {
        public ClientResponse Client { get; set; }
        public dynamic Activity { get; set; }
        public ActionResponse Action { get; set; }
    }
}
