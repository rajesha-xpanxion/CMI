using System;

namespace CMI.MessageRetriever.Model
{
    public class ClientProfileDrugTestResultDetailsActivityResponse
    {
        public DateTime TestDateTime { get; set; }
        public string ResultStatus { get; set; }
        public string Location { get; set; }
        public string DrugTestType { get; set; }
        public string DeliberateTamper { get; set; }
        public string Screened { get; set; }
        public string SentToLab { get; set; }
        public string DrugTestId { get; set; }
        public string Dilute { get; set; }
        public string CreatinineLevel { get; set; }
    }
}
