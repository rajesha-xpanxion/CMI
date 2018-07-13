using System;
using System.Collections.Generic;
using System.Text;

namespace CMI.DAL.Source
{
    public class OffenderPhone : Offender
    {
        public int Id { get; set; }

        public string PhoneNumberType { get; set; }

        public string Phone { get; set; }

        public bool IsPrimary { get; set; }

        public string Comment { get; set; }
    }
}
