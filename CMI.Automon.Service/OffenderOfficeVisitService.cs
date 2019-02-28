using CMI.Automon.Interface;
using CMI.Automon.Model;
using Microsoft.Extensions.Options;

namespace CMI.Automon.Service
{
    public class OffenderOfficeVisitService : IOffenderOfficeVisitService
    {
        #region Private Member Variables
        private readonly AutomonConfig automonConfig;
        #endregion

        #region Constructor
        public OffenderOfficeVisitService(
            IOptions<AutomonConfig> automonConfig
        )
        {
            this.automonConfig = automonConfig.Value;
        }
        #endregion
    }
}
