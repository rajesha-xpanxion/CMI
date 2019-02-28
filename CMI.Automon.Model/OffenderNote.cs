using System;

namespace CMI.Automon.Model
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
