using CMI.Common.Logging;
using CMI.Importer.DAL;
using Microsoft.Extensions.Configuration;
using System;

namespace CMI.Importer
{
    public abstract class BaseImporter
    {
        protected ILogger Logger { get; set; }
        protected ImporterConfig ImporterConfig { get; set; }

        public BaseImporter(
            IServiceProvider serviceProvider,
            IConfiguration configuration
        )
        {
            Logger = (ILogger)serviceProvider.GetService(typeof(ILogger));
            ImporterConfig = configuration.GetSection(ConfigKeys.ImporterConfig).Get<ImporterConfig>();
        }

        public abstract void Execute();
    }
}
