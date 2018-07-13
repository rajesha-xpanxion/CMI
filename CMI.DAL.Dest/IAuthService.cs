using CMI.DAL.Dest.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace CMI.DAL.Dest
{
    public interface IAuthService
    {
        AuthTokenResponse AuthToken { get; }
    }
}
