﻿using CMI.DAL.Dest;
using CMI.DAL.Dest.Nexus;
using CMI.DAL.Source;
using CMI.DAL.Source.AutoMon;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;

namespace CMI.Processor
{
    static class Program
    {
        #region Entry Point
        static void Main(string[] args)
        {

            Console.WriteLine("Starting Processor execution...{0}", Environment.NewLine);

            // create service collection
            var serviceCollection = new ServiceCollection();
            
            //configure required services
            ConfigureServices(serviceCollection);

            // create service provider
            var serviceProvider = serviceCollection.BuildServiceProvider();

            // entry to run scheduler
            serviceProvider.GetService<Processor>().Execute();


            Console.WriteLine("{0}Processor execution completed successfully...", Environment.NewLine);
        }
        #endregion

        #region Private Helper Methods
        private static void ConfigureServices(IServiceCollection serviceCollection)
        {
            //service configuration for source
            serviceCollection.AddSingleton<IOffenderService, OffenderService>();
            serviceCollection.AddSingleton<IOffenderAddressService, OffenderAddressService>();
            serviceCollection.AddSingleton<IOffenderPhoneService, OffenderPhoneService>();
            serviceCollection.AddSingleton<IOffenderEmailService, OffenderEmailService>();
            serviceCollection.AddSingleton<IOffenderCaseService, OffenderCaseService>();
            serviceCollection.AddSingleton<IOffenderNoteService, OffenderNoteService>();

            //service configuration for destination
            serviceCollection.AddSingleton<IClientService, ClientService>();
            serviceCollection.AddSingleton<IAddressService, AddressService>();
            serviceCollection.AddSingleton<IContactService, ContactService>();
            serviceCollection.AddSingleton<ICaseService, CaseService>();
            serviceCollection.AddSingleton<INoteService, NoteService>();
            serviceCollection.AddSingleton<ILookupService, LookupService>();
            serviceCollection.AddSingleton<IAuthService, AuthService>();

            //common services
            serviceCollection.AddSingleton<Common.Logging.ILogger, Common.Logging.DbLogger>();
            serviceCollection.AddSingleton<DAL.IProcessorProvider, DAL.ProcessorProvider>();
            serviceCollection.AddSingleton<Common.Notification.IEmailNotificationProvider, Common.Notification.EmailNotificationProvider>();

            // add processor as service
            serviceCollection.AddTransient<Processor>();

            //read configuration from appsettings.json
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("AppSettings.json", false)
                .Build();

            //configure required configurations in service
            serviceCollection.Configure<CMI.DAL.Dest.Models.DestinationConfig>(configuration.GetSection("DestinationConfig"));
            serviceCollection.Configure<CMI.DAL.Source.Models.SourceConfig>(configuration.GetSection("SourceConfig"));
            serviceCollection.Configure<DAL.ProcessorConfig>(configuration.GetSection("ProcessorConfig"));
            serviceCollection.Configure<Common.Logging.LogConfig>(configuration.GetSection("LogConfig"));
            serviceCollection.Configure<Common.Notification.EmailNotificationConfig>(configuration.GetSection("NotificationConfig:EmailNotificationConfig"));
            serviceCollection.AddOptions();
        }
        #endregion
    }
}
