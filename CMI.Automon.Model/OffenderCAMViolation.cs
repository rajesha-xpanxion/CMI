using System;

namespace CMI.Automon.Model
{
    public class OffenderCAMViolation : Offender
    {
        public DateTime ViolationDateTime { get; set; }
        public string ViolationStatus { get; set; }
    }
}
