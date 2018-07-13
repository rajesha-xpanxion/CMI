using System;
using System.Collections.Generic;
using System.Text;

namespace CMI.DAL.Source
{
    public class Offender
    {
        public string Pin { get; set; }

        public string FirstName { get; set; }

        public string MiddleName { get; set; }

        public string LastName { get; set; }

        public DateTime DateOfBirth { get; set; }

        public string ClientType { get; set; }

        public string TimeZone { get; set; }

        public string Gender { get; set; }

        public string Race { get; set; }

        public string CaseloadName { get; set; }

        public string CaseloadType { get; set; }

        public string OfficerLogon { get; set; }

        public string OfficerEmail { get; set; }

        public string OfficerFirstName { get; set; }

        public string OfficerLastName { get; set; }
    }
}
