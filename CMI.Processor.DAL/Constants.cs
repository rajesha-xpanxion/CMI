
namespace CMI.Processor.DAL
{
    public static class Constants
    {
        public static string ContactTypeEmailNexus { get { return "E-mail"; } }

        public static string EthnicityUnknown { get { return "Unknown"; } }
    }

    public static class SqlParamName
    {
        public static string ProcessorTypeId { get { return "@ProcessorTypeId"; } }

        public static string ExecutedOn { get { return "@ExecutedOn"; } }

        public static string IsSuccessful { get { return "@IsSuccessful"; } }

        public static string NumTaskProcessed { get { return "@NumTaskProcessed"; } }

        public static string NumTaskSucceeded { get { return "@NumTaskSucceeded"; } }

        public static string NumTaskFailed { get { return "@NumTaskFailed"; } }

        public static string Message { get { return "@Message"; } }

        public static string ErrorDetails { get { return "@ErrorDetails"; } }

    }

    public static class StoredProc
    {
        public static string GetLastExecutionDateTime { get { return @"[dbo].[GetLastExecutionDateTime]"; } }

        public static string SaveExecutionStatus { get { return @"[dbo].[SaveExecutionStatus]"; } }
    }

    public static class InboundProcessorStage
    {
        public static string ClientProfiles { get { return "ClientProfiles"; } }
        public static string Addresses { get { return "Addresses"; } }
        public static string PhoneContacts { get { return "PhoneContacts"; } }
        public static string EmailContacts { get { return "EmailContacts"; } }
        public static string Cases { get { return "Cases"; } }
        public static string Notes { get { return "Notes"; } }
    }

    public static class ConfigKeys
    {
        public static string ProcessorConfig { get { return "ProcessorConfig"; } }
        public static string NexusConfig { get { return "NexusConfig"; } }
        public static string AutomonConfig { get { return "AutomonConfig"; } }
        public static string LogConfig { get { return "LogConfig"; } }
        public static string EmailNotificationConfig { get { return "NotificationConfig:EmailNotificationConfig"; } }
    }
}
