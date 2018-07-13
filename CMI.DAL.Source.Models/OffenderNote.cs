using System;
using System.Collections.Generic;
using System.Text;

namespace CMI.DAL.Source
{
    public class OffenderNote : Offender
    {
        public int NoteId { get; set; }

        public DateTime NoteDate { get; set; }

        public string Value { get; set; }

        public string Logon { get; set; }
        public string Email { get; set; }
    }
}
