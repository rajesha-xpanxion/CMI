using CMI.Nexus.Model;

namespace CMI.Nexus.Interface
{
    public interface INoteService
    {
        bool AddNewNoteDetails(Note note);

        Note GetNoteDetails(string clientId, string noteId);

        bool UpdateNoteDetails(Note note);

        bool DeleteNoteDetails(string clientId, string noteId);
    }
}
