using System;
using System.Collections.Generic;
using System.Text;

namespace CMI.Automon.Model
{
    public class OffenderVehicle : Offender
    {
        public int VehicleYear { get; set; }
        public string Make { get; set; }
        public string BodyStyle { get; set; }
        public string Color { get; set; }
        public string LicensePlate { get; set; }
        public bool IsActive { get; set; }
    }
}
