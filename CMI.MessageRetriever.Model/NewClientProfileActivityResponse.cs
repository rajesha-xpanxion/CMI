using System;
using System.Collections.Generic;
using System.Text;

namespace CMI.MessageRetriever.Model
{
    public class NewClientProfileActivityResponse : DetailsResponse
    {
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string TimeZone { get; set; }
        public string Gender { get; set; }
        public string Ethinicity { get; set; }
        public string Email { get; set; }
        public string ClientType { get; set; }
        public string AddressType { get; set; }
        public string Address { get; set; }
        public string ContactType { get; set; }
        public string Contact { get; set; }
    }
}
