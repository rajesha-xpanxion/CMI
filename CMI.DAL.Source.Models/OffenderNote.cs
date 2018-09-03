using System;
using System.Collections.Generic;
using System.Text;

namespace CMI.DAL.Source
{
    public class OffenderNote : Offender
    {
        public int Id { get; set; }

        public DateTime Date { get; set; }

        public string Text { get; set; }

        public string AuthorEmail { get; set; }

        public string NoteType { get; set; }
    }
}
