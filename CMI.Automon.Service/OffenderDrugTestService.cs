using CMI.Automon.Interface;
using CMI.Automon.Model;
using Microsoft.Extensions.Options;

namespace CMI.Automon.Service
{
    public class OffenderDrugTestService : IOffenderDrugTestService
    {
        #region Private Member Variables
        private readonly AutomonConfig automonConfig;
        #endregion

        #region Constructor
        public OffenderDrugTestService(
            IOptions<AutomonConfig> automonConfig
        )
        {
            this.automonConfig = automonConfig.Value;
        }
        #endregion
    }
}
