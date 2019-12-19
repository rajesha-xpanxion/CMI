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
using Newtonsoft.Json;

namespace CMI.Processor
{
    static class Program
    {
        #region Entry Point
        static void Main(string[] args)
        {
            if(args != null && args.Length > 0)
            {
                if(args[0].Equals("crypto", StringComparison.InvariantCultureIgnoreCase))
                {
                    Console.WriteLine("1. Encrypt");
                    Console.WriteLine("2. Decrypt");
                    Console.WriteLine("3. Exit");
                    Console.WriteLine("Please enter your choice...");
                    int choice = 3;
                    if (Int32.TryParse(Console.ReadLine(), out choice))
                    {
                        string passPhrase = string.Empty, plainText = string.Empty, cipherText = string.Empty;
                        switch (choice)
                        {
                            case 1:
                                Console.WriteLine("Please enter pass phrase:");
                                passPhrase = Console.ReadLine();
                                Console.WriteLine("Please enter plain text:");
                                plainText = Console.ReadLine();
                                Console.WriteLine("Encrypted text: {0}", CryptographyHelper.EncryptString(plainText, passPhrase));
                                break;
                            case 2:
                                Console.WriteLine("Please enter pass phrase:");
                                passPhrase = Console.ReadLine();
                                Console.WriteLine("Please enter cipher text:");
                                cipherText = Console.ReadLine();
                                Console.WriteLine("Decrypted text: {0}", CryptographyHelper.DecryptString(cipherText, passPhrase));
                                break;
                            default:
                                return;
                        }
                    }
                    else
                    {
                        Console.WriteLine("Invalid choice entered!!! Press enter key to exit...");
                    }
                    Console.ReadKey();
                    return;
                }
            }

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

            System.Collections.Generic.List<Common.Notification.TaskExecutionStatus> taskExecutionStatuses = new System.Collections.Generic.List<Common.Notification.TaskExecutionStatus>();

            //check if to execute inbound processor
            if (processorTypeToExecute == ProcessorType.Both || processorTypeToExecute == ProcessorType.Inbound)
            {
                // entry to run inbound processor
                taskExecutionStatuses.AddRange(serviceProvider.GetService<InboundProcessor>().Execute());
            }

            //check if to execute outbound processor
            if (processorTypeToExecute == ProcessorType.Both || processorTypeToExecute == ProcessorType.Outbound)
            {
                // entry to run outbound processor
                taskExecutionStatuses.AddRange(serviceProvider.GetService<OutboundProcessor>().Execute());
            }

            //send execution status report email
            SendExecutionStatusReportEmail(serviceProvider, configuration, taskExecutionStatuses, processorTypeToExecute);

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

            //configure message retriever service
            ConfigureMessageRetrieverService(serviceCollection, configuration);

            //configure required configurations in service
            ConfigureOptions(serviceCollection, configuration);

            return configuration;
        }

        private static void ConfigureAutomonServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IOffenderService, OffenderService>();
            serviceCollection.AddSingleton<IOffenderPersonalDetailsService, OffenderPersonalDetailsService>();
            serviceCollection.AddSingleton<IOffenderAddressService, OffenderAddressService>();
            serviceCollection.AddSingleton<IOffenderPhoneService, OffenderPhoneService>();
            serviceCollection.AddSingleton<IOffenderEmailService, OffenderEmailService>();
            serviceCollection.AddSingleton<IOffenderVehicleService, OffenderVehicleService>();
            serviceCollection.AddSingleton<IOffenderEmploymentService, OffenderEmploymentService>();
            serviceCollection.AddSingleton<IOffenderCaseService, OffenderCaseService>();
            serviceCollection.AddSingleton<IOffenderNoteService, OffenderNoteService>();
            serviceCollection.AddSingleton<IOffenderDrugTestAppointmentService, OffenderDrugTestAppointmentService>();
            serviceCollection.AddSingleton<IOffenderDrugTestResultService, OffenderDrugTestResultService>();
            serviceCollection.AddSingleton<IOffenderOfficeVisitService, OffenderOfficeVisitService>();
            serviceCollection.AddSingleton<IOffenderFieldVisitService, OffenderFieldVisitService>();
            serviceCollection.AddSingleton<IOffenderTreatmentAppointmentService, OffenderTreatmentAppointmentService>();
            serviceCollection.AddSingleton<IOffenderProfilePictureService, OffenderProfilePictureService>();
            serviceCollection.AddSingleton<IOffenderCAMViolationService, OffenderCAMViolationService>();
            serviceCollection.AddSingleton<IOffenderGPSViolationService, OffenderGPSViolationService>();
            serviceCollection.AddSingleton<IOffenderIncentiveService, OffenderIncentiveService>();
            serviceCollection.AddSingleton<IOffenderSanctionService, OffenderSanctionService>();
            serviceCollection.AddSingleton<IOffenderOnDemandSanctionService, OffenderOnDemandSanctionService>();
            serviceCollection.AddSingleton<IOffenderTouchPointCheckInService, OffenderTouchPointCheckInService>();
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
            serviceCollection.AddSingleton<ICommonService, CommonService>();
            serviceCollection.AddSingleton<IVehicleService, VehicleService>();
            serviceCollection.AddSingleton<IEmploymentService, EmploymentService>();
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
            serviceCollection.AddSingleton<InboundVehicleProcessor>();
            serviceCollection.AddSingleton<InboundEmploymentProcessor>();
        }

        private static void ConfigureOutboundProcessorServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<OutboundProcessor>();
            serviceCollection.AddSingleton<OutboundNewClientProfileProcessor>();
            serviceCollection.AddSingleton<OutboundClientProfilePersonalDetailsProcessor>();
            serviceCollection.AddSingleton<OutboundClientProfileEmailProcessor>();
            serviceCollection.AddSingleton<OutboundClientProfileAddressProcessor>();
            serviceCollection.AddSingleton<OutboundClientProfileContactProcessor>();
            serviceCollection.AddSingleton<OutboundClientProfileVehicleProcessor>();
            serviceCollection.AddSingleton<OutboundClientProfileEmploymentProcessor>();
            serviceCollection.AddSingleton<OutboundClientProfileNoteProcessor>();
            serviceCollection.AddSingleton<OutboundClientProfileOfficeVisitProcessor>();
            serviceCollection.AddSingleton<OutboundClientProfileDrugTestAppointmentProcessor>();
            serviceCollection.AddSingleton<OutboundClientProfileDrugTestResultProcessor>();
            serviceCollection.AddSingleton<OutboundClientProfileFieldVisitProcessor>();
            serviceCollection.AddSingleton<OutboundClientProfileTreatmentAppointmentProcessor>();
            serviceCollection.AddSingleton<OutboundClientProfilePictureProcessor>();
            serviceCollection.AddSingleton<OutboundClientProfileCAMAlertProcessor>();
            serviceCollection.AddSingleton<OutboundClientProfileCAMSupervisionProcessor>();
            serviceCollection.AddSingleton<OutboundClientProfileGPSAlertProcessor>();
            serviceCollection.AddSingleton<OutboundClientProfileGPSSupervisionProcessor>();
            serviceCollection.AddSingleton<OutboundClientProfileIncentiveProcessor>();
            serviceCollection.AddSingleton<OutboundClientProfileSanctionProcessor>();
            serviceCollection.AddSingleton<OutboundClientProfileOnDemandSanctionProcessor>();
            serviceCollection.AddSingleton<OutboundClientProfileTouchPointCheckInProcessor>();
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

        private static void SendExecutionStatusReportEmail(
            IServiceProvider serviceProvider,
            IConfiguration configuration,
            System.Collections.Generic.IEnumerable<Common.Notification.TaskExecutionStatus> taskExecutionStatuses,
            ProcessorType processorType
        )
        {
            //retrieve required values from configuration
            string executionStatusReportReceiverEmailAddresses = configuration.GetValue<string>(ConfigKeys.ExecutionStatusReportReceiverEmailAddresses);
            string executionStatusReportEmailSubject = configuration.GetValue<string>(ConfigKeys.ExecutionStatusReportEmailSubject);
            //create request object
            var executionStatusReportEmailRequest = new Common.Notification.ExecutionStatusReportEmailRequest
            {
                ToEmailAddress = executionStatusReportReceiverEmailAddresses,
                Subject = executionStatusReportEmailSubject,
                TaskExecutionStatuses = taskExecutionStatuses,
                ProcessorType = (Common.Notification.ProcessorType)processorType
            };
            //send email notification
            var response = serviceProvider.GetService<Common.Notification.IEmailNotificationProvider>().SendExecutionStatusReportEmail(executionStatusReportEmailRequest);

            //save status of email notification in log
            var logRequest = new Common.Logging.LogRequest
            {
                OperationName = "ExecutionStatusReportEmailNotification",
                MethodName = "SendExecutionStatusReportEmail",
                CustomParams = JsonConvert.SerializeObject(executionStatusReportEmailRequest)
            };

            var logger = serviceProvider.GetService<Common.Logging.ILogger>();

            if (response.IsSuccessful)
            {
                logRequest.Message = "Execution status report email sent successfully.";
                logger.LogInfo(logRequest);
            }
            else
            {
                logRequest.Message = "Error occurred while sending execution status report email.";
                logRequest.Exception = response.Exception;
                logger.LogWarning(logRequest);
            }
        }
        #endregion
    }
}
