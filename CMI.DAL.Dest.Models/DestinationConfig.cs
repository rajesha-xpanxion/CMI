using System;
using System.Collections.Generic;
using System.Text;

namespace CMI.DAL.Dest.Models
{
    public class DestinationConfig
    {
        public string CaseIntegrationAPIBaseURL { get; set; }

        public string CaseIntegrationAPIVersion { get; set; }

        public TokenGeneratorConfig TokenGeneratorConfig { get; set; }
    }

    public class TokenGeneratorConfig
    {
        public string TokenGeneratorAPIBaseURL { get; set; }

        public string GrantType { get; set; }

        public string Scope { get; set; }

        public string ClientId { get; set; }

        public string ClientSecret { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }
    }
}
