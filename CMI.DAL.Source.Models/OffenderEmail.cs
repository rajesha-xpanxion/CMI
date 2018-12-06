
namespace CMI.DAL.Source
{
    public class OffenderEmail : Offender
    {
        public int Id { get; set; }

        public string EmailAddress { get; set; }

        public bool IsPrimary { get; set; }

        public bool IsActive { get; set; }
    }
}
