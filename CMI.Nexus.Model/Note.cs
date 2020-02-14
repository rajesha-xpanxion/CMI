
using System;

namespace CMI.Nexus.Model
{
    public class Note
    {
        #region  Public Properties
        public string ClientId { get; set; }
        public string NoteId { get; set; }
        public string NoteText { get; set; }
        public string NoteAuthor { get; set; }
        public string NoteDatetime { get; set; }
        public string NoteType { get; set; }
        #endregion

        #region Public Methods
        public bool Equals(Note other)
        {
            if (other is null)
                return false;

            //compare ClientId
            if (
                !(
                    (string.IsNullOrEmpty(ClientId) && string.IsNullOrEmpty(other.ClientId))
                    ||
                    string.Equals(ClientId, other.ClientId, StringComparison.InvariantCultureIgnoreCase)
                )
            )
                return false;

            //compare NoteId
            if (
                !(
                    (string.IsNullOrEmpty(NoteId) && string.IsNullOrEmpty(other.NoteId))
                    ||
                    string.Equals(NoteId, other.NoteId, StringComparison.InvariantCultureIgnoreCase)
                )
            )
                return false;

            //compare NoteText
            if (
                !(
                    (string.IsNullOrEmpty(NoteText) && string.IsNullOrEmpty(other.NoteText))
                    ||
                    string.Equals(NoteText, other.NoteText, StringComparison.InvariantCultureIgnoreCase)
                )
            )
                return false;

            //compare NoteAuthor
            if (
                !(
                    (string.IsNullOrEmpty(NoteAuthor) && string.IsNullOrEmpty(other.NoteAuthor))
                    ||
                    string.Equals(NoteAuthor, other.NoteAuthor, StringComparison.InvariantCultureIgnoreCase)
                )
            )
                return false;

            //compare NoteDatetime
            if (
                !(
                    (string.IsNullOrEmpty(NoteDatetime) && string.IsNullOrEmpty(other.NoteDatetime))
                    ||
                    string.Equals(NoteDatetime, other.NoteDatetime, StringComparison.InvariantCultureIgnoreCase)
                )
            )
                return false;

            //compare NoteType
            if (
                !(
                    (string.IsNullOrEmpty(NoteType) && string.IsNullOrEmpty(other.NoteType))
                    ||
                    string.Equals(NoteType, other.NoteType, StringComparison.InvariantCultureIgnoreCase)
                )
            )
                return false;

            return true;
        }
        #endregion

        #region Public Overridden Methods
        public override bool Equals(object obj) => Equals(obj as Note);
        public override int GetHashCode() => (ClientId, NoteId, NoteText, NoteAuthor, NoteDatetime, NoteType).GetHashCode();
        #endregion
    }
}
