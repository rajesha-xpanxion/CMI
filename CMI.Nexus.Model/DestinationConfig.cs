
namespace CMI.Nexus.Model
{
    public class NexusConfig
    {
        public string CaseIntegrationApiBaseUrl { get; set; }

        public string CaseIntegrationApiVersion { get; set; }

        public TokenGeneratorConfig TokenGeneratorConfig { get; set; }
    }

    public class TokenGeneratorConfig
    {
        public string TokenGeneratorApiBaseUrl { get; set; }

        public string GrantType { get; set; }

        public string Scope { get; set; }

        public string ClientId { get; set; }

        public string ClientSecret { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }
    }
}
