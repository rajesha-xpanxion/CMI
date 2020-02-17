
using System;

namespace CMI.Nexus.Model
{
    public class Client
    {
        #region  Public Properties
        public string IntegrationId { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string ClientType { get; set; }
        public string TimeZone { get; set; }
        public string Gender { get; set; }
        public string Ethnicity { get; set; }
        public string DateOfBirth { get; set; }
        public string SupervisingOfficerEmailId { get; set; }
        public string CaseloadId { get; set; }
        public string StaticRiskRating { get; set; }
        public string NeedsClassification { get; set; }
        public string CmsStatus { get; set; }
        #endregion

        #region Public Methods
        public bool Equals(Client other)
        {
            if (other is null)
                return false;

            //compare IntegrationId
            if (
                !(
                    (string.IsNullOrEmpty(IntegrationId) && string.IsNullOrEmpty(other.IntegrationId))
                    ||
                    string.Equals(IntegrationId, other.IntegrationId, StringComparison.InvariantCultureIgnoreCase)
                )
            )
                return false;

            //compare FirstName
            if (
                !(
                    (string.IsNullOrEmpty(FirstName) && string.IsNullOrEmpty(other.FirstName))
                    ||
                    string.Equals(FirstName, other.FirstName, StringComparison.InvariantCultureIgnoreCase)
                )
            )
                return false;

            //compare MiddleName
            if (
                !(
                    (string.IsNullOrEmpty(MiddleName) && string.IsNullOrEmpty(other.MiddleName))
                    ||
                    string.Equals(MiddleName, other.MiddleName, StringComparison.InvariantCultureIgnoreCase)
                )
            )
                return false;

            //compare LastName
            if (
                !(
                    (string.IsNullOrEmpty(LastName) && string.IsNullOrEmpty(other.LastName))
                    ||
                    string.Equals(LastName, other.LastName, StringComparison.InvariantCultureIgnoreCase)
                )
            )
                return false;

            //compare ClientType
            if (
                !(
                    (string.IsNullOrEmpty(ClientType) && string.IsNullOrEmpty(other.ClientType))
                    ||
                    string.Equals(ClientType, other.ClientType, StringComparison.InvariantCultureIgnoreCase)
                )
            )
                return false;

            //compare TimeZone
            if (
                !(
                    (string.IsNullOrEmpty(TimeZone) && string.IsNullOrEmpty(other.TimeZone))
                    ||
                    string.Equals(TimeZone, other.TimeZone, StringComparison.InvariantCultureIgnoreCase)
                )
            )
                return false;

            //compare Gender
            if (
                !(
                    (string.IsNullOrEmpty(Gender) && string.IsNullOrEmpty(other.Gender))
                    ||
                    string.Equals(Gender, other.Gender, StringComparison.InvariantCultureIgnoreCase)
                )
            )
                return false;

            //compare Ethnicity
            if (
                !(
                    (string.IsNullOrEmpty(Ethnicity) && string.IsNullOrEmpty(other.Ethnicity))
                    ||
                    string.Equals(Ethnicity, other.Ethnicity, StringComparison.InvariantCultureIgnoreCase)
                )
            )
                return false;

            //compare DateOfBirth
            if (
                !(
                    (string.IsNullOrEmpty(DateOfBirth) && string.IsNullOrEmpty(other.DateOfBirth))
                    ||
                    string.Equals(DateOfBirth, other.DateOfBirth, StringComparison.InvariantCultureIgnoreCase)
                )
            )
                return false;

            //compare SupervisingOfficerEmailId
            if (
                !(
                    (string.IsNullOrEmpty(SupervisingOfficerEmailId) && string.IsNullOrEmpty(other.SupervisingOfficerEmailId))
                    ||
                    string.Equals(SupervisingOfficerEmailId, other.SupervisingOfficerEmailId, StringComparison.InvariantCultureIgnoreCase)
                )
            )
                return false;

            //compare CaseloadId
            if (
                !(
                    (string.IsNullOrEmpty(CaseloadId) && string.IsNullOrEmpty(other.CaseloadId))
                    ||
                    string.Equals(CaseloadId, other.CaseloadId, StringComparison.InvariantCultureIgnoreCase)
                )
            )
                return false;

            //compare StaticRiskRating
            if (
                !(
                    (string.IsNullOrEmpty(StaticRiskRating) && string.IsNullOrEmpty(other.StaticRiskRating))
                    ||
                    string.Equals(StaticRiskRating, other.StaticRiskRating, StringComparison.InvariantCultureIgnoreCase)
                )
            )
                return false;

            //compare NeedsClassification
            if (
                !(
                    (string.IsNullOrEmpty(NeedsClassification) && string.IsNullOrEmpty(other.NeedsClassification))
                    ||
                    string.Equals(NeedsClassification, other.NeedsClassification, StringComparison.InvariantCultureIgnoreCase)
                )
            )
                return false;

            return true;
        }
        #endregion

        #region Public Overridden Methods
        public override bool Equals(object obj) => Equals(obj as Client);
        public override int GetHashCode() => 
            (IntegrationId, FirstName, MiddleName, LastName, ClientType, TimeZone, Gender, Ethnicity, DateOfBirth, SupervisingOfficerEmailId, CaseloadId, StaticRiskRating, NeedsClassification, CmsStatus)
            .GetHashCode();
        #endregion
    }

    public class ClientProfilePicture : Client
    {
        public string ImageBase64String { get; set; }
    }
}
