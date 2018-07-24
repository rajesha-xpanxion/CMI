using System;
using System.Collections.Generic;
using System.Text;

namespace CMI.DAL.Dest
{
    public class Contact
    {
        public string ClientId { get; set; }
        public string ContactId { get; set; }
        public string Comment { get; set; }
        public string ContactValue { get; set; }
        public string ContactType { get; set; }
        public bool IsPrimary { get; set; }
        public string Status { get; set; }
    }
}
