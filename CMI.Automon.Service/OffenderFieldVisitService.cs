using CMI.Automon.Interface;
using CMI.Automon.Model;
using Microsoft.Extensions.Options;

namespace CMI.Automon.Service
{
    public class OffenderFieldVisitService : IOffenderFieldVisitService
    {
        #region Private Member Variables
        private readonly AutomonConfig automonConfig;
        #endregion

        #region Constructor
        public OffenderFieldVisitService(
            IOptions<AutomonConfig> automonConfig
        )
        {
            this.automonConfig = automonConfig.Value;
        }
        #endregion
    }
}
