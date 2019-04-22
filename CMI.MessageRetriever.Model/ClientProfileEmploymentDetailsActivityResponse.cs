namespace CMI.MessageRetriever.Model
{
    public class ClientProfileEmploymentDetailsActivityResponse : DetailsResponse
    {
        public string Employer { get; set; }
        public string Occupation { get; set; }
        public string WorkAddress { get; set; }
        public string WorkPhone { get; set; }
        public string Wage { get; set; }
        public string WageUnit { get; set; }
        public string WorkEnvironment { get; set; }
    }
}
