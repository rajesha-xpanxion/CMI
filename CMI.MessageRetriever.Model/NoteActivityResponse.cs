﻿using System;

namespace CMI.MessageRetriever.Model
{
    public class NoteActivityResponse : ActivityResponse
    {
        public string NoteText { get; set; }

        public string NoteAuthor { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}