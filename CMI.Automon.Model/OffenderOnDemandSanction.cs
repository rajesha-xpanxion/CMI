using System;
using System.Collections.Generic;

namespace CMI.Automon.Model
{
    public class OffenderOnDemandSanction : Offender
    {
        public DateTime EventDateTime { get; set; }
        public string Magnitude { get; set; }
        public string Response { get; set; }
    }
}
