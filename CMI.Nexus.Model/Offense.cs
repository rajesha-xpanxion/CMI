
using System;

namespace CMI.Nexus.Model
{
    public class Offense : IEquatable<Offense>
    {
        #region  Public Properties
        public string Label { get; set; }
        public string Date { get; set; }
        public string Statute { get; set; }
        public string Category { get; set; }
        public bool IsPrimary { get; set; }
        #endregion

        #region Public Methods
        public bool Equals(Offense other)
        {
            if (other is null)
                return false;

            //compare Label
            if (
                !(
                    (string.IsNullOrEmpty(Label) && string.IsNullOrEmpty(other.Label))
                    ||
                    string.Equals(Label, other.Label, StringComparison.InvariantCultureIgnoreCase)
                )
            )
                return false;

            //compare Date
            if (
                !(
                    (string.IsNullOrEmpty(Date) && string.IsNullOrEmpty(other.Date))
                    ||
                    string.Equals(Date, other.Date, StringComparison.InvariantCultureIgnoreCase)
                )
            )
                return false;

            //compare Statute
            if (
                !(
                    (string.IsNullOrEmpty(Statute) && string.IsNullOrEmpty(other.Statute))
                    ||
                    string.Equals(Statute, other.Statute, StringComparison.InvariantCultureIgnoreCase)
                )
            )
                return false;

            //compare Category
            if (
                !(
                    (string.IsNullOrEmpty(Category) && string.IsNullOrEmpty(other.Category))
                    ||
                    string.Equals(Category, other.Category, StringComparison.InvariantCultureIgnoreCase)
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
        public override bool Equals(object obj) => Equals(obj as Offense);
        public override int GetHashCode() => (Label, Date, Statute, Category, IsPrimary).GetHashCode();
        #endregion
    }
}
