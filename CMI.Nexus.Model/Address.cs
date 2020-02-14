
using System;

namespace CMI.Nexus.Model
{
    public class Address
    {
        #region  Public Properties
        public string ClientId { get; set; }
        public string AddressId { get; set; }
        public string Comment { get; set; }
        public string FullAddress { get; set; }
        public string AddressType { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsActive { get; set; }
        #endregion

        #region Public Methods
        public bool Equals(Address other)
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

            //compare AddressId
            if (
                !(
                    (string.IsNullOrEmpty(AddressId) && string.IsNullOrEmpty(other.AddressId))
                    ||
                    string.Equals(AddressId, other.AddressId, StringComparison.InvariantCultureIgnoreCase)
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

            //compare FullAddress
            if (
                !(
                    (string.IsNullOrEmpty(FullAddress) && string.IsNullOrEmpty(other.FullAddress))
                    ||
                    string.Equals(FullAddress, other.FullAddress, StringComparison.InvariantCultureIgnoreCase)
                )
            )
                return false;

            //compare AddressType
            if (
                !(
                    (string.IsNullOrEmpty(AddressType) && string.IsNullOrEmpty(other.AddressType))
                    ||
                    string.Equals(AddressType, other.AddressType, StringComparison.InvariantCultureIgnoreCase)
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
        public override bool Equals(object obj) => Equals(obj as Address);
        public override int GetHashCode() => (ClientId, AddressId, Comment, FullAddress, AddressType, IsPrimary, IsActive).GetHashCode();
        #endregion
    }
}
