using System;

namespace CMI.MessageRetriever.Model
{
    public class ClientProfileCAMAlertDetailsActivityResponse : DetailsResponse
    {
        public string Status { get; set; }
        public string AlertType { get; set; }
        public DateTime AlertDateTime { get; set; }
    }
}
