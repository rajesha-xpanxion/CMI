using CMI.DAL.Dest.Models;

namespace CMI.DAL.Dest
{
    public interface IAuthService
    {
        AuthTokenResponse AuthToken { get; }
    }
}
