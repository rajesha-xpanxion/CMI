using System;
using System.Collections.Generic;
using System.Text;

namespace CMI.Automon.Model
{
    public class OffenderFieldVisit : Offender
    {
        public int Id { get; set; }
        public DateTime StartDate { get; set; }
        public string Comment { get; set; }
        public DateTime EndDate { get; set; }
        public int Status { get; set; }
        public bool IsOffenderPresent { get; set; }
        public bool IsSearchConducted { get; set; }
        public string SearchLocations { get; set; }
        public string SearchResults { get; set; }
    }
}
