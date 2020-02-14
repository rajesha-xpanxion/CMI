
using System;

namespace CMI.Nexus.Model
{
    public class Vehicle
    {
        #region  Public Properties
        public string ClientId { get; set; }
        public string VehicleId { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public string LicensePlate { get; set; }
        public string Color { get; set; }
        public bool IsActive { get; set; }
        #endregion

        #region Public Methods
        public bool Equals(Vehicle other)
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

            //compare VehicleId
            if (
                !(
                    (string.IsNullOrEmpty(VehicleId) && string.IsNullOrEmpty(other.VehicleId))
                    ||
                    string.Equals(VehicleId, other.VehicleId, StringComparison.InvariantCultureIgnoreCase)
                )
            )
                return false;

            //compare Make
            if (
                !(
                    (string.IsNullOrEmpty(Make) && string.IsNullOrEmpty(other.Make))
                    ||
                    string.Equals(Make, other.Make, StringComparison.InvariantCultureIgnoreCase)
                )
            )
                return false;

            //compare Model
            if (
                !(
                    (string.IsNullOrEmpty(Model) && string.IsNullOrEmpty(other.Model))
                    ||
                    string.Equals(Model, other.Model, StringComparison.InvariantCultureIgnoreCase)
                )
            )
                return false;

            //compare Year
            if (Year != other.Year)
                return false;

            //compare LicensePlate
            if (
                !(
                    (string.IsNullOrEmpty(LicensePlate) && string.IsNullOrEmpty(other.LicensePlate))
                    ||
                    string.Equals(LicensePlate, other.LicensePlate, StringComparison.InvariantCultureIgnoreCase)
                )
            )
                return false;

            //compare Color
            if (
                !(
                    (string.IsNullOrEmpty(Color) && string.IsNullOrEmpty(other.Color))
                    ||
                    string.Equals(Color, other.Color, StringComparison.InvariantCultureIgnoreCase)
                )
            )
                return false;

            return true;
        }
        #endregion

        #region Public Overridden Methods
        public override bool Equals(object obj) => Equals(obj as Vehicle);
        public override int GetHashCode() => (ClientId, VehicleId, Make, Model, Year, LicensePlate, Color, IsActive).GetHashCode();
        #endregion
    }
}
