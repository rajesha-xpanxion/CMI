using CMI.Nexus.Model;
using System.Collections.Generic;

namespace CMI.Nexus.Interface
{
    public interface INoteService
    {
        bool AddNewNoteDetails(Note note);

        Note GetNoteDetails(string clientId, string noteId);

        List<Note> GetAllNoteDetails(string clientId);

        bool UpdateNoteDetails(Note note);

        bool DeleteNoteDetails(string clientId, string noteId);
    }
}
