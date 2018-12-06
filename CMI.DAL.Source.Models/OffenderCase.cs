using System;

namespace CMI.DAL.Source
{
    public class OffenderCase : Offender
    {
        public string CaseNumber { get; set; }

        public string CaseStatus { get; set; }

        public DateTime? CaseDate { get; set; }

        public DateTime? SupervisionStartDate { get; set; }

        public DateTime? SupervisionEndDate { get; set; }

        public string ClosureReason { get; set; }

        public string OffenseLabel { get; set; }

        public string OffenseStatute { get; set; }

        public string OffenseCategory { get; set; }

        public bool IsPrimary { get; set; }

        public DateTime? OffenseDate { get; set; }
    }
}
