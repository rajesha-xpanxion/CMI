using CMI.Common.Logging;
using CMI.Importer.DAL;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;

namespace CMI.Importer
{
    public class InboundImporter : BaseImporter
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IConfiguration configuration;

        public InboundImporter(
            IServiceProvider serviceProvider,
            IConfiguration configuration
        )
            : base(serviceProvider, configuration)
        {
            this.serviceProvider = serviceProvider;
            this.configuration = configuration;
        }

        public override void Execute()
        {
            //log info message for start of importsing
            Logger.LogInfo(new LogRequest
            {
                OperationName = this.GetType().Name,
                MethodName = "Execute",
                Message = "Inbound Importer execution initiated."
            });

            //import client profiles
            if (ImporterConfig.InboundImporterConfig.ExcelSheetsToImport != null && ImporterConfig.InboundImporterConfig.ExcelSheetsToImport.Any(a => a.Equals(InboundImporterStage.ClientProfiles, StringComparison.InvariantCultureIgnoreCase)))
            {
                ((InboundClientProfileImporter)serviceProvider.GetService(typeof(InboundClientProfileImporter))).Execute();
            }

            //import client contacts
            if (ImporterConfig.InboundImporterConfig.ExcelSheetsToImport != null && ImporterConfig.InboundImporterConfig.ExcelSheetsToImport.Any(a => a.Equals(InboundImporterStage.Contacts, StringComparison.InvariantCultureIgnoreCase)))
            {
                ((InboundContactImporter)serviceProvider.GetService(typeof(InboundContactImporter))).Execute();
            }

            //import client addresses
            if (ImporterConfig.InboundImporterConfig.ExcelSheetsToImport != null && ImporterConfig.InboundImporterConfig.ExcelSheetsToImport.Any(a => a.Equals(InboundImporterStage.Addresses, StringComparison.InvariantCultureIgnoreCase)))
            {
                ((InboundAddressImporter)serviceProvider.GetService(typeof(InboundAddressImporter))).Execute();
            }

            //import client cases
            if (ImporterConfig.InboundImporterConfig.ExcelSheetsToImport != null && ImporterConfig.InboundImporterConfig.ExcelSheetsToImport.Any(a => a.Equals(InboundImporterStage.CourtCases, StringComparison.InvariantCultureIgnoreCase)))
            {
                ((InboundCourtCaseImporter)serviceProvider.GetService(typeof(InboundCourtCaseImporter))).Execute();
            }

            //log info message for end of importing
            Logger.LogInfo(new LogRequest
            {
                OperationName = this.GetType().Name,
                MethodName = "Execute",
                Message = "Inbound Importer execution completed."
            });
        }
    }
}
