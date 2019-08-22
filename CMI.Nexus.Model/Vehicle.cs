
namespace CMI.Nexus.Model
{
    public class Vehicle
    {
        public string ClientId { get; set; }
        public string VehicleId { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public string LicensePlate { get; set; }
        public string Color { get; set; }
        public bool IsActive { get; set; }
    }
}
