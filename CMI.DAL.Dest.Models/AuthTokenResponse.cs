using System;
using System.Collections.Generic;
using System.Text;

namespace CMI.DAL.Dest.Models
{
    public class AuthTokenResponse
    {
        public string access_token { get; set; }

        public string expires_in { get; set; }

        public string token_type { get; set; }
    }
}
