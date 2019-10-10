using System;

namespace CMI.Automon.Model
{
    public class OffenderGPSViolation : Offender
    {
        public DateTime ViolationDateTime { get; set; }
        public string ViolationStatus { get; set; }
    }
}
