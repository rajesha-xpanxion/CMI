using System;

namespace CMI.Processor.DAL
{
    public class OutboundMessageDetails
    {
        public int Id { get; set; }
        public string ActivityTypeName { get; set; }
        public string ActivitySubTypeName { get; set; }
        public string ActionReasonName { get; set; }
        public string ClientIntegrationId { get; set; }
        public string ActivityIdentifier { get; set; }
        public DateTime? ActionOccurredOn { get; set; }
        public string ActionUpdatedBy { get; set; }
        public string Details { get; set; }
        public bool IsSuccessful { get; set; }
        public string ErrorDetails { get; set; }
        public string RawData { get; set; }
        public bool IsProcessed { get; set; }
    }
}
