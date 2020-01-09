using System;

namespace CMI.Automon.Model
{
    public class Offender
    {
        public int Id { get; set; }
        public string Pin { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string ClientType { get; set; }
        public string TimeZone { get; set; }
        public string Gender { get; set; }
        public string Race { get; set; }
        public string RaceDescription { get; set; }
        public string RacePermDesc { get; set; }
        public string CaseloadName { get; set; }
        public string OfficerLogon { get; set; }
        public string OfficerEmail { get; set; }
        public string OfficerFirstName { get; set; }
        public string OfficerLastName { get; set; }
        public string DeptSupLevel { get; set; }
        public string SupervisionStatus { get; set; }
        public string BodyStatus { get; set; }
        public string UpdatedBy { get; set; }
    }

    public class OffenderMugshot : Offender
    {
        public int DocumentId { get; set; }
        public DateTime? DocumentDate { get; set; }
        public byte[] DocumentData { get; set; }
    }
}
