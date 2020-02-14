
using System;

namespace CMI.Nexus.Model
{
    public class Contact
    {
        #region  Public Properties
        public string ClientId { get; set; }
        public string ContactId { get; set; }
        public string Comment { get; set; }
        public string ContactValue { get; set; }
        public string ContactType { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsActive { get; set; }
        #endregion

        #region Public Methods
        public bool Equals(Contact other)
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

            //compare ContactId
            if (
                !(
                    (string.IsNullOrEmpty(ContactId) && string.IsNullOrEmpty(other.ContactId))
                    ||
                    string.Equals(ContactId, other.ContactId, StringComparison.InvariantCultureIgnoreCase)
                )
            )
                return false;

            //compare Comment
            if (
                !(
                    (string.IsNullOrEmpty(Comment) && string.IsNullOrEmpty(other.Comment))
                    ||
                    string.Equals(Comment, other.Comment, StringComparison.InvariantCultureIgnoreCase)
                )
            )
                return false;

            //compare ContactValue
            if (
                !(
                    (string.IsNullOrEmpty(ContactValue) && string.IsNullOrEmpty(other.ContactValue))
                    ||
                    string.Equals(ContactValue, other.ContactValue, StringComparison.InvariantCultureIgnoreCase)
                )
            )
                return false;

            //compare ContactType
            if (
                !(
                    (string.IsNullOrEmpty(ContactType) && string.IsNullOrEmpty(other.ContactType))
                    ||
                    string.Equals(ContactType, other.ContactType, StringComparison.InvariantCultureIgnoreCase)
                )
            )
                return false;

            //compare IsPrimary
            if (IsPrimary != other.IsPrimary)
                return false;

            return true;
        }
        #endregion

        #region Public Overridden Methods
        public override bool Equals(object obj) => Equals(obj as Contact);
        public override int GetHashCode() => (ClientId, ContactId, Comment, ContactValue, ContactType, IsPrimary, IsActive).GetHashCode();
        #endregion
    }
}
