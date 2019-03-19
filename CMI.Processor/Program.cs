using CMI.Nexus.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using CMI.Nexus.Service;
using CMI.Automon.Interface;
using CMI.Automon.Service;
using CMI.Processor.DAL;

namespace CMI.Processor
{
    static class Program
    {
        #region Entry Point
        static void Main(string[] args)
        {
            ProcessorType processorTypeToExecute = ReadProcessorTypeToExecute(args);

            Console.WriteLine(
                (
                    processorTypeToExecute == ProcessorType.Both 
                    ? "Starting execution of Both processors...{0}" 
                    : (
                        processorTypeToExecute == ProcessorType.Inbound 
                        ? "Starting execution of Inbound Processor ...{0}" 
                        : "Starting execution of Outbound Processor ...{0}"
                    )
                ), 
                Environment.NewLine
            );

            // create service collection
            var serviceCollection = new ServiceCollection();
            
            //configure required services
            ConfigureServices(serviceCollection);

            // create service provider
            var serviceProvider = serviceCollection.BuildServiceProvider();

            //check for processor type to execute
            if (processorTypeToExecute == ProcessorType.Both || processorTypeToExecute == ProcessorType.Inbound)
            {
                // entry to run scheduler
                serviceProvider.GetService<InboundProcessor>().Execute();
            }

            if (processorTypeToExecute == ProcessorType.Both || processorTypeToExecute == ProcessorType.Outbound)
            {
                // entry to run scheduler
                serviceProvider.GetService<OutboundProcessor>().Execute();
            }

            Console.WriteLine(
                (
                    processorTypeToExecute == ProcessorType.Both
                    ? "{0}Execution of Both processors completed successfully..."
                    : (
                        processorTypeToExecute == ProcessorType.Inbound 
                        ? "{0}Execution of Inbound processor completed successfully..." 
                        : "{0}Execution of Outbound processor completed successfully..."
                    )
                ),
                Environment.NewLine
            );
        }
        #endregion

        #region Private Helper Methods
        private static void ConfigureServices(IServiceCollection serviceCollection)
        {
            //service configuration for automon
            serviceCollection.AddSingleton<IOffenderService, OffenderService>();
            serviceCollection.AddSingleton<IOffenderAddressService, OffenderAddressService>();
            serviceCollection.AddSingleton<IOffenderPhoneService, OffenderPhoneService>();
            serviceCollection.AddSingleton<IOffenderEmailService, OffenderEmailService>();
            serviceCollection.AddSingleton<IOffenderCaseService, OffenderCaseService>();
            serviceCollection.AddSingleton<IOffenderNoteService, OffenderNoteService>();

            //service configuration for nexus
            serviceCollection.AddSingleton<IClientService, ClientService>();
            serviceCollection.AddSingleton<IAddressService, AddressService>();
            serviceCollection.AddSingleton<IContactService, ContactService>();
            serviceCollection.AddSingleton<ICaseService, CaseService>();
            serviceCollection.AddSingleton<INoteService, NoteService>();
            serviceCollection.AddSingleton<ILookupService, LookupService>();
            serviceCollection.AddSingleton<IAuthService, AuthService>();

            //common services
            serviceCollection.AddSingleton<Common.Logging.ILogger, Common.Logging.DbLogger>();
            serviceCollection.AddSingleton<IProcessorProvider, ProcessorProvider>();
            serviceCollection.AddSingleton<Common.Notification.IEmailNotificationProvider, Common.Notification.EmailNotificationProvider>();

            // add processor as service
            serviceCollection.AddSingleton<InboundProcessor>();
            serviceCollection.AddSingleton<InboundClientProfileProcessor>();
            serviceCollection.AddSingleton<InboundAddressProcessor>();
            serviceCollection.AddSingleton<InboundPhoneContactProcessor>();
            serviceCollection.AddSingleton<InboundEmailContactProcessor>();
            serviceCollection.AddSingleton<InboundCaseProcessor>();
            serviceCollection.AddSingleton<InboundNoteProcessor>();
            serviceCollection.AddSingleton<OutboundProcessor>();
            serviceCollection.AddSingleton<OutboundClientProfileProcessor>();
            serviceCollection.AddSingleton<OutboundNoteProcessor>();
            serviceCollection.AddSingleton<OutboundOfficeVisitProcessor>();
            serviceCollection.AddSingleton<OutboundDrugTestProcessor>();
            serviceCollection.AddSingleton<OutboundFieldVisitProcessor>();


            string enviornmentName = Environment.GetEnvironmentVariable("ENVIORNMENT_NAME");

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
            serviceCollection.Configure<Nexus.Model.NexusConfig>(configuration.GetSection(ConfigKeys.NexusConfig));
            serviceCollection.Configure<Automon.Model.AutomonConfig>(configuration.GetSection(ConfigKeys.AutomonConfig));
            serviceCollection.Configure<ProcessorConfig>(configuration.GetSection(ConfigKeys.ProcessorConfig));
            serviceCollection.Configure<Common.Logging.LogConfig>(configuration.GetSection(ConfigKeys.LogConfig));
            serviceCollection.Configure<Common.Notification.EmailNotificationConfig>(configuration.GetSection(ConfigKeys.EmailNotificationConfig));
            serviceCollection.AddOptions();
        }

        private static ProcessorType ReadProcessorTypeToExecute(string[] args)
        {
            ProcessorType processorTypeToExecute = ProcessorType.Both; 

            if (args.Length > 0)
            {
                if(args[0].Equals("inbound", StringComparison.InvariantCultureIgnoreCase))
                {
                    processorTypeToExecute = ProcessorType.Inbound;
                }
                else if (args[0].Equals("outbound", StringComparison.InvariantCultureIgnoreCase))
                {
                    processorTypeToExecute = ProcessorType.Outbound;
                }
                else
                {
                    processorTypeToExecute = ProcessorType.Both;
                }
            }

            return processorTypeToExecute;
        }
        #endregion
    }
}
