using CMI.DAL.Dest.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace CMI.DAL.Dest
{
    public interface INoteService
    {
        bool AddNewNoteDetails(Note note);

        Note GetNoteDetails(string clientId, string noteId);

        bool UpdateNoteDetails(Note note);
    }
}
