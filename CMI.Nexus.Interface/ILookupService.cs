using CMI.Nexus.Model;
using System.Collections.Generic;

namespace CMI.Nexus.Interface
{
    public interface ILookupService
    {
        List<string> AddressTypes { get; }

        List<CaseLoad> CaseLoads { get; }

        List<string> ClientTypes { get; }

        List<string> ContactTypes { get; }

        List<string> Ethnicities { get; }

        List<string> Genders { get; }

        List<SupervisingOfficer> SupervisingOfficers { get; }

        List<string> TimeZones { get; }

        List<string> OffenseCategories { get; }

        List<string> StaticRiskRatings { get; }
    }
}
