
using System;

namespace CMI.Nexus.Model
{
    public class Employment
    {
        #region  Public Properties
        public string ClientId { get; set; }
        public string EmployerId { get; set; }
        public string Employer { get; set; }
        public string Occupation { get; set; }
        public string WorkAddress { get; set; }
        public string WorkPhone { get; set; }
        public string Wage { get; set; }
        public string WageUnit { get; set; }
        public string WorkEnvironment { get; set; }
        public bool IsActive { get; set; }
        #endregion

        #region Public Methods
        public bool Equals(Employment other)
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

            //compare EmployerId
            if (
                !(
                    (string.IsNullOrEmpty(EmployerId) && string.IsNullOrEmpty(other.EmployerId))
                    ||
                    string.Equals(EmployerId, other.EmployerId, StringComparison.InvariantCultureIgnoreCase)
                )
            )
                return false;

            //compare Employer
            if (
                !(
                    (string.IsNullOrEmpty(Employer) && string.IsNullOrEmpty(other.Employer))
                    ||
                    string.Equals(Employer, other.Employer, StringComparison.InvariantCultureIgnoreCase)
                )
            )
                return false;

            //compare Occupation
            if (
                !(
                    (string.IsNullOrEmpty(Occupation) && string.IsNullOrEmpty(other.Occupation))
                    ||
                    string.Equals(Occupation, other.Occupation, StringComparison.InvariantCultureIgnoreCase)
                )
            )
                return false;

            //compare WorkAddress
            if (
                !(
                    (string.IsNullOrEmpty(WorkAddress) && string.IsNullOrEmpty(other.WorkAddress))
                    ||
                    string.Equals(WorkAddress, other.WorkAddress, StringComparison.InvariantCultureIgnoreCase)
                )
            )
                return false;

            //compare WorkPhone
            if (
                !(
                    (string.IsNullOrEmpty(WorkPhone) && string.IsNullOrEmpty(other.WorkPhone))
                    ||
                    string.Equals(WorkPhone, other.WorkPhone, StringComparison.InvariantCultureIgnoreCase)
                )
            )
                return false;

            //compare Wage
            if (
                !(
                    (string.IsNullOrEmpty(Wage) && string.IsNullOrEmpty(other.Wage))
                    ||
                    string.Equals(Wage, other.Wage, StringComparison.InvariantCultureIgnoreCase)
                )
            )
                return false;

            //compare WageUnit
            if (
                !(
                    (string.IsNullOrEmpty(WageUnit) && string.IsNullOrEmpty(other.WageUnit))
                    ||
                    string.Equals(WageUnit, other.WageUnit, StringComparison.InvariantCultureIgnoreCase)
                )
            )
                return false;

            //compare WorkEnvironment
            if (
                !(
                    (string.IsNullOrEmpty(WorkEnvironment) && string.IsNullOrEmpty(other.WorkEnvironment))
                    ||
                    string.Equals(WorkEnvironment, other.WorkEnvironment, StringComparison.InvariantCultureIgnoreCase)
                )
            )
                return false;

            return true;
        }
        #endregion

        #region Public Overridden Methods
        public override bool Equals(object obj) => Equals(obj as Employment);
        public override int GetHashCode() => (ClientId, EmployerId, Employer, Occupation, WorkAddress, WorkPhone, Wage, WageUnit, WorkEnvironment, IsActive).GetHashCode();
        #endregion
    }
}
