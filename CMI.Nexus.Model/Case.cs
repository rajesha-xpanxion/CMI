using System;
using System.Collections.Generic;
using System.Linq;

namespace CMI.Nexus.Model
{
    public class Case
    {
        #region  Public Properties
        public string ClientId { get; set; }
        public string CaseNumber { get; set; }
        public string CaseDate { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string EarlyReleaseDate { get; set; }
        public string EndReason { get; set; }
        public string Status { get; set; }
        public List<Offense> Offenses { get; set; }
        #endregion

        #region Public Methods
        public bool Equals(Case other)
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

            //compare CaseNumber
            if (
                !(
                    (string.IsNullOrEmpty(CaseNumber) && string.IsNullOrEmpty(other.CaseNumber))
                    ||
                    string.Equals(CaseNumber, other.CaseNumber, StringComparison.InvariantCultureIgnoreCase)
                )
            )
                return false;

            //compare CaseDate
            if (
                !(
                    (string.IsNullOrEmpty(CaseDate) && string.IsNullOrEmpty(other.CaseDate))
                    ||
                    string.Equals(CaseDate, other.CaseDate, StringComparison.InvariantCultureIgnoreCase)
                )
            )
                return false;

            //compare StartDate
            if (
                !(
                    (string.IsNullOrEmpty(StartDate) && string.IsNullOrEmpty(other.StartDate))
                    ||
                    string.Equals(StartDate, other.StartDate, StringComparison.InvariantCultureIgnoreCase)
                )
            )
                return false;

            //compare EndDate
            if (
                !(
                    (string.IsNullOrEmpty(EndDate) && string.IsNullOrEmpty(other.EndDate))
                    ||
                    string.Equals(EndDate, other.EndDate, StringComparison.InvariantCultureIgnoreCase)
                )
            )
                return false;

            //compare EndReason
            if (
                !(
                    (string.IsNullOrEmpty(EndReason) && string.IsNullOrEmpty(other.EndReason))
                    ||
                    string.Equals(EndReason, other.EndReason, StringComparison.InvariantCultureIgnoreCase)
                )
            )
                return false;

            //compare Status
            if (
                !(
                    (string.IsNullOrEmpty(Status) && string.IsNullOrEmpty(other.Status))
                    ||
                    string.Equals(Status, other.Status, StringComparison.InvariantCultureIgnoreCase)
                )
            )
                return false;

            //compare Offenses
            if (Offenses == null && other.Offenses == null)
            {
                return true;
            }
            else if(Offenses != null && other.Offenses != null)
            {
                if(!Offenses
                    .OrderBy(a => a.Label)
                    .ThenBy(b => b.Date)
                    .ThenBy(c => c.Statute)
                    .ThenBy(d => d.Category)
                    .ThenBy(e => e.IsPrimary)
                    .SequenceEqual(
                        other.Offenses
                            .OrderBy(a => a.Label)
                            .ThenBy(b => b.Date)
                            .ThenBy(c => c.Statute)
                            .ThenBy(d => d.Category)
                            .ThenBy(e => e.IsPrimary)
                    ))
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

            return true;
        }
        #endregion

        #region Public Overridden Methods
        public override bool Equals(object obj) => Equals(obj as Case);
        public override int GetHashCode() => (ClientId, CaseNumber, CaseDate, StartDate, EndDate, EarlyReleaseDate, EndReason, Status, Offenses).GetHashCode();
        #endregion
    }
}
