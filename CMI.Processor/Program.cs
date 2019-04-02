using CMI.Nexus.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using CMI.Nexus.Service;
using CMI.Automon.Interface;
using CMI.Automon.Service;
using CMI.Processor.DAL;
using CMI.MessageRetriever.Interface;
using CMI.MessageRetriever.Model;

namespace CMI.Processor
{
    static class Program
    {
        #region Entry Point
        static void Main(string[] args)
        {
            // create service collection
            var serviceCollection = new ServiceCollection();
            
            //configure required services
            var configuration = ConfigureServices(serviceCollection);

            //get processor types to execute
            var processorTypeToExecute = GetProcessorTypesToExecute(configuration);

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

            // create service provider
            var serviceProvider = serviceCollection.BuildServiceProvider();

            //check if to execute inbound processor
            if (processorTypeToExecute == ProcessorType.Both || processorTypeToExecute == ProcessorType.Inbound)
            {
                // entry to run inbound processor
                serviceProvider.GetService<InboundProcessor>().Execute();
            }

            //check if to execute outbound processor
            if (processorTypeToExecute == ProcessorType.Both || processorTypeToExecute == ProcessorType.Outbound)
            {
                // entry to run outbound processor
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
        private static IConfiguration ConfigureServices(IServiceCollection serviceCollection)
        {
            //service configuration for automon
            ConfigureAutomonServices(serviceCollection);

            //service configuration for nexus
            ConfigureNexusServices(serviceCollection);

            //common services
            ConfigureCommonServices(serviceCollection);

            // add processor as service
            ConfigureInboundProcessorServices(serviceCollection);
            ConfigureOutboundProcessorServices(serviceCollection);

            //retrieve enviornment name and load related appsettings json file
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

            //configure message retriever service
            ConfigureMessageRetrieverService(serviceCollection, configuration);

            //configure required configurations in service
            ConfigureOptions(serviceCollection, configuration);

            return configuration;
        }

        private static void ConfigureAutomonServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IOffenderService, OffenderService>();
            serviceCollection.AddSingleton<IOffenderAddressService, OffenderAddressService>();
            serviceCollection.AddSingleton<IOffenderPhoneService, OffenderPhoneService>();
            serviceCollection.AddSingleton<IOffenderEmailService, OffenderEmailService>();
            serviceCollection.AddSingleton<IOffenderCaseService, OffenderCaseService>();
            serviceCollection.AddSingleton<IOffenderNoteService, OffenderNoteService>();
            serviceCollection.AddSingleton<IOffenderDrugTestService, OffenderDrugTestService>();
            serviceCollection.AddSingleton<IOffenderOfficeVisitService, OffenderOfficeVisitService>();
            serviceCollection.AddSingleton<IOffenderFieldVisitService, OffenderFieldVisitService>();
        }

        private static void ConfigureNexusServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IClientService, ClientService>();
            serviceCollection.AddSingleton<IAddressService, AddressService>();
            serviceCollection.AddSingleton<IContactService, ContactService>();
            serviceCollection.AddSingleton<ICaseService, CaseService>();
            serviceCollection.AddSingleton<INoteService, NoteService>();
            serviceCollection.AddSingleton<ILookupService, LookupService>();
            serviceCollection.AddSingleton<IAuthService, AuthService>();
        }

        private static void ConfigureCommonServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<Common.Logging.ILogger, Common.Logging.DbLogger>();
            serviceCollection.AddSingleton<IProcessorProvider, ProcessorProvider>();
            serviceCollection.AddSingleton<Common.Notification.IEmailNotificationProvider, Common.Notification.EmailNotificationProvider>();
        }

        private static void ConfigureInboundProcessorServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<InboundProcessor>();
            serviceCollection.AddSingleton<InboundClientProfileProcessor>();
            serviceCollection.AddSingleton<InboundAddressProcessor>();
            serviceCollection.AddSingleton<InboundPhoneContactProcessor>();
            serviceCollection.AddSingleton<InboundEmailContactProcessor>();
            serviceCollection.AddSingleton<InboundCaseProcessor>();
            serviceCollection.AddSingleton<InboundNoteProcessor>();
        }

        private static void ConfigureOutboundProcessorServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<OutboundProcessor>();
            serviceCollection.AddSingleton<OutboundClientProfileProcessor>();
            serviceCollection.AddSingleton<OutboundNoteProcessor>();
            serviceCollection.AddSingleton<OutboundOfficeVisitProcessor>();
            serviceCollection.AddSingleton<OutboundDrugTestAppointmentProcessor>();
            serviceCollection.AddSingleton<OutboundDrugTestResultProcessor>();
            serviceCollection.AddSingleton<OutboundFieldVisitProcessor>();
        }

        private static void ConfigureMessageRetrieverService(IServiceCollection serviceCollection, IConfiguration configuration)
        {
            if (configuration.GetValue<string>(ConfigKeys.MessageRetrieverTypeToExecute).Equals(MessageRetrieverTypeToExecute.Amqp, StringComparison.InvariantCultureIgnoreCase))
            {
                serviceCollection.AddSingleton<IMessageRetrieverService, MessageRetriever.AMQP.MessageRetrieverService>();
            }
            else
            {
                serviceCollection.AddSingleton<IMessageRetrieverService, MessageRetriever.REST.MessageRetrieverService>();
            }
        }

        private static void ConfigureOptions(IServiceCollection serviceCollection, IConfiguration configuration)
        {
            serviceCollection.Configure<Nexus.Model.NexusConfig>(configuration.GetSection(ConfigKeys.NexusConfig));
            serviceCollection.Configure<Automon.Model.AutomonConfig>(configuration.GetSection(ConfigKeys.AutomonConfig));
            serviceCollection.Configure<ProcessorConfig>(configuration.GetSection(ConfigKeys.ProcessorConfig));
            serviceCollection.Configure<Common.Logging.LogConfig>(configuration.GetSection(ConfigKeys.LogConfig));
            serviceCollection.Configure<Common.Notification.EmailNotificationConfig>(configuration.GetSection(ConfigKeys.EmailNotificationConfig));
            serviceCollection.Configure<MessageRetrieverConfig>(configuration.GetSection(ConfigKeys.MessageRetrieverConfig));
            serviceCollection.AddOptions();
        }

        private static ProcessorType GetProcessorTypesToExecute(IConfiguration configuration)
        {
            ProcessorType processorTypeToExecute = ProcessorType.Both;

            string processorTypesToExecute = configuration.GetValue<string>(ConfigKeys.ProcessorTypesToExecute);

            if (!string.IsNullOrEmpty(processorTypesToExecute))
            {
                if (processorTypesToExecute.Equals("inbound", StringComparison.InvariantCultureIgnoreCase))
                {
                    processorTypeToExecute = ProcessorType.Inbound;
                }
                else if (processorTypesToExecute.Equals("outbound", StringComparison.InvariantCultureIgnoreCase))
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
