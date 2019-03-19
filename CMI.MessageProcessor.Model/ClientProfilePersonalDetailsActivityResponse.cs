using System;

namespace CMI.MessageProcessor.Model
{
    public class ClientProfilePersonalDetailsActivityResponse : ActivityResponse
    {
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string TimeZone { get; set; }
        public string Gender { get; set; }
        public string Ethinicity { get; set; }
    }
}
