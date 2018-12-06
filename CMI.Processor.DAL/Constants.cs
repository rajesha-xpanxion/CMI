
namespace CMI.Processor.DAL
{
    public static class Constants
    {
        public static string ContactTypeEmailDest { get { return "E-mail"; } }

        public static string EthnicityUnknown { get { return "Unknown"; } }
    }

    public static class SqlParamName
    {
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

    public static class ProcessorStage
    {
        public static string ProcessClientProfiles { get { return "ProcessClientProfiles"; } }
        public static string ProcessAddresses { get { return "ProcessAddresses"; } }
        public static string ProcessPhoneContacts { get { return "ProcessPhoneContacts"; } }
        public static string ProcessEmailContacts { get { return "ProcessEmailContacts"; } }
        public static string ProcessCases { get { return "ProcessCases"; } }
        public static string ProcessNotes { get { return "ProcessNotes"; } }
    }
}
