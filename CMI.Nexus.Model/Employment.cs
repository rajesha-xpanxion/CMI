
namespace CMI.Nexus.Model
{
    public class Employment
    {
        public string ClientId { get; set; }
        public string EmployerId { get; set; }
        public string Employer { get; set; }
        public string Occupation { get; set; }
        public string WorkAddress { get; set; }
        public string WorkPhone { get; set; }
        public string Wage { get; set; }
        public string WageUnit { get; set; }
        public string WorkEnvironment { get; set; }
        public bool IsActive { get; set; }
    }
}
