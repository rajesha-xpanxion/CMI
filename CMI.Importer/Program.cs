using CMI.Nexus.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using CMI.Nexus.Service;
using CMI.Importer.DAL;

namespace CMI.Importer
{
    static class Program
    {
        static void Main(string[] args)
        {
            // create service collection
            var serviceCollection = new ServiceCollection();

            //configure required services
            var configuration = ConfigureServices(serviceCollection);

            //get importer types to execute
            var importerTypeToExecute = GetImporterTypesToExecute(configuration);

            Console.WriteLine(
                (
                    importerTypeToExecute == ImporterType.Both
                    ? "Starting execution of Both importers...{0}"
                    : (
                        importerTypeToExecute == ImporterType.Inbound
                        ? "Starting execution of Inbound Importer ...{0}"
                        : "Starting execution of Outbound Importer ...{0}"
                    )
                ),
                Environment.NewLine
            );

            // create service provider
            var serviceProvider = serviceCollection.BuildServiceProvider();

            //check if to execute inbound importer
            if (importerTypeToExecute == ImporterType.Both || importerTypeToExecute == ImporterType.Inbound)
            {
                // entry to run inbound importer
                serviceProvider.GetService<InboundImporter>().Execute();
            }

            Console.WriteLine(
                (
                    importerTypeToExecute == ImporterType.Both
                    ? "{0}Execution of Both importers completed successfully..."
                    : (
                        importerTypeToExecute == ImporterType.Inbound
                        ? "{0}Execution of Inbound importer completed successfully..."
                        : "{0}Execution of Outbound importer completed successfully..."
                    )
                ),
                Environment.NewLine
            );
        }

        #region Private Helper Methods
        private static IConfiguration ConfigureServices(IServiceCollection serviceCollection)
        {
            //service configuration for nexus
            ConfigureNexusServices(serviceCollection);

            //common services
            ConfigureCommonServices(serviceCollection);

            // add importer as service
            ConfigureInboundImporterServices(serviceCollection);

            //retrieve enviornment name and load related appsettings json file
            string enviornmentName = Environment.GetEnvironmentVariable("ENVIORNMENT_NAME");

            Console.WriteLine($"ENVIORNMENT_NAME: {enviornmentName}");

            //read configuration from appsettings.json
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(
                    string.IsNullOrEmpty(enviornmentName)
                    ? $"AppSettings.json"
                    : $"AppSettings.{enviornmentName}.json",
                    optional: false,
                    reloadOnChange: false
                )
                .Build();

            serviceCollection.AddSingleton<IConfiguration>(configuration);

            //configure required configurations in service
            ConfigureOptions(serviceCollection, configuration);

            return configuration;
        }

        private static void ConfigureNexusServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IClientService, ClientService>();
            serviceCollection.AddSingleton<IAddressService, AddressService>();
            serviceCollection.AddSingleton<IContactService, ContactService>();
            serviceCollection.AddSingleton<ICaseService, CaseService>();
            serviceCollection.AddSingleton<ILookupService, LookupService>();
            serviceCollection.AddSingleton<IAuthService, AuthService>();
            serviceCollection.AddSingleton<ICommonService, CommonService>();
        }

        private static void ConfigureCommonServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<Common.Logging.ILogger, Common.Logging.DbLogger>();
            serviceCollection.AddSingleton<IImporterProvider, ImporterProvider>();
        }

        private static void ConfigureInboundImporterServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<InboundImporter>();
            serviceCollection.AddSingleton<InboundClientProfileImporter>();
            serviceCollection.AddSingleton<InboundCourtCaseImporter>();
            serviceCollection.AddSingleton<InboundAddressImporter>();
            serviceCollection.AddSingleton<InboundContactImporter>();
        }

        private static void ConfigureOptions(IServiceCollection serviceCollection, IConfiguration configuration)
        {
            serviceCollection.Configure<Nexus.Model.NexusConfig>(configuration.GetSection(ConfigKeys.NexusConfig));
            serviceCollection.Configure<ImporterConfig>(configuration.GetSection(ConfigKeys.ImporterConfig));
            serviceCollection.Configure<Common.Logging.LogConfig>(configuration.GetSection(ConfigKeys.LogConfig));
            serviceCollection.AddOptions();
        }

        private static ImporterType GetImporterTypesToExecute(IConfiguration configuration)
        {
            ImporterType importerTypeToExecute = ImporterType.Both;

            string importerTypesToExecute = configuration.GetValue<string>(ConfigKeys.ImporterTypesToExecute);

            if (!string.IsNullOrEmpty(importerTypesToExecute))
            {
                if (importerTypesToExecute.Equals("inbound", StringComparison.InvariantCultureIgnoreCase))
                {
                    importerTypeToExecute = ImporterType.Inbound;
                }
                else if (importerTypesToExecute.Equals("outbound", StringComparison.InvariantCultureIgnoreCase))
                {
                    importerTypeToExecute = ImporterType.Outbound;
                }
                else
                {
                    importerTypeToExecute = ImporterType.Both;
                }
            }

            return importerTypeToExecute;
        }
        #endregion
    }
}
