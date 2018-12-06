using CMI.DAL.Dest.Models;

namespace CMI.DAL.Dest
{
    public interface INoteService
    {
        bool AddNewNoteDetails(Note note);

        Note GetNoteDetails(string clientId, string noteId);

        bool UpdateNoteDetails(Note note);

        bool DeleteNoteDetails(string clientId, string noteId);
    }
}
