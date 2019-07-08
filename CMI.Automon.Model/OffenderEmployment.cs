using System;
using System.Collections.Generic;
using System.Text;

namespace CMI.Automon.Model
{
    public class OffenderEmployment : Offender
    {
        public int Id { get; set; }
        public string OrganizationName { get; set; }
        public string OrganizationAddress { get; set; }
        public string OrganizationPhone { get; set; }
        public string PayFrequency { get; set; }
        public string PayRate { get; set; }
        public string WorkType { get; set; }
        public string JobTitle { get; set; }
    }
}
