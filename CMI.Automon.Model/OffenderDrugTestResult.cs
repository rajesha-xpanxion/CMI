using System;
using System.Collections.Generic;
using System.Text;

namespace CMI.Automon.Model
{
    public class OffenderDrugTestResult : Offender
    {
        public DateTime StartDate { get; set; }
        public string Comment { get; set; }
        public DateTime EndDate { get; set; }
        public int Status { get; set; }
        public string DeviceType { get; set; }
        public string TestResult { get; set; }
        public string Validities { get; set; }
    }
}
