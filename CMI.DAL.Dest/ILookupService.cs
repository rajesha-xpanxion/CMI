using CMI.DAL.Dest.Models;
using System.Collections.Generic;

namespace CMI.DAL.Dest
{
    public interface ILookupService
    {
        List<string> AddressTypes { get; }

        List<CaseLoad> CaseLoads { get; }

        List<string> ClientTypes { get; }

        List<string> ContactTypes { get; }

        List<string> Ethnicities { get; }

        List<string> Genders { get; }

        List<string> Offenses { get; }

        List<SupervisingOfficer> SupervisingOfficers { get; }

        List<string> TimeZones { get; }
    }
}
