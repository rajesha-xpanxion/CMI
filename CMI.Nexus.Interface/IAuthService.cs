using CMI.Nexus.Model;

namespace CMI.Nexus.Interface
{
    public interface IAuthService
    {
        AuthTokenResponse AuthToken { get; }
    }
}
