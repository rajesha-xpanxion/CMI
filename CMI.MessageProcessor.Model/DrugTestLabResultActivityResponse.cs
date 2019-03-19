using System;

namespace CMI.MessageProcessor.Model
{
    public class DrugTestLabResultActivityResponse : ActivityResponse
    {
        public DateTime TestDateTime { get; set; }
        public string ResultStatus { get; set; }
        public string Location { get; set; }
        public DrugTestLabResultDetails Details { get; set; }
    }

    public class DrugTestLabResultDetails
    {
        public string DrugTestType { get; set; }
        public string DeliberateTamper { get; set; }
        public string Screened { get; set; }
        public string SentToLab { get; set; }
        public string DrugTestId { get; set; }
        public string Dilute { get; set; }
        public string CreatinineLevel { get; set; }
    }
}
