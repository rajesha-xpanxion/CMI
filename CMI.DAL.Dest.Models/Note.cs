﻿using System;
using System.Collections.Generic;
using System.Text;

namespace CMI.DAL.Dest.Models
{
    public class Note
    {
        public string ClientId { get; set; }

        public string NoteId { get; set; }

        public string NoteText { get; set; }

        public string NoteAuthor { get; set; }

        public string NoteDatetime { get; set; }
    }
}
