using CMI.Automon.Interface;
using CMI.Automon.Model;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace CMI.Automon.Service
{
    public class OffenderVehicleService : IOffenderVehicleService
    {
        #region Private Member Variables
        private readonly AutomonConfig automonConfig;
        #endregion

        #region Constructor
        public OffenderVehicleService(
            IOptions<AutomonConfig> automonConfig
        )
        {
            this.automonConfig = automonConfig.Value;
        }
        #endregion
    }
}
