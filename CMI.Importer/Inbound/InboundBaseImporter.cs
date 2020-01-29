using CMI.Common.Logging;
using CMI.Importer.DAL;
using CMI.Nexus.Interface;
using CMI.Nexus.Service;
using Microsoft.Extensions.Configuration;
using System;

namespace CMI.Importer
{
    public abstract class InboundBaseImporter
    {
        protected readonly IImporterProvider ImporterProvider;
        protected ILogger Logger { get; set; }
        protected IClientService ClientService { get; set; }
        protected ImporterConfig ImporterConfig { get; set; }

        public InboundBaseImporter(
            IServiceProvider serviceProvider,
            IConfiguration configuration
        )
        {
            Logger = (ILogger)serviceProvider.GetService(typeof(ILogger));

            ImporterProvider = (ImporterProvider)serviceProvider.GetService(typeof(IImporterProvider));
            ClientService = (ClientService)serviceProvider.GetService(typeof(IClientService));

            ImporterConfig = configuration.GetSection(ConfigKeys.ImporterConfig).Get<ImporterConfig>();
        }

        public abstract void Execute();
    }
}
