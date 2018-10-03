using System;
using System.Collections.Generic;
using System.Text;

namespace CMI.Processor.DAL
{
    public class Constants
    {
        public const string CONTACT_TYPE_EMAIL_DEST = "E-mail";

        public const string ETHNICITY_UNKNOWN = "Unknown";
    }

    public class SQLParamName
    {
        public const string EXECUTED_ON = "@ExecutedOn";

        public const string IS_SUCCESSFUL = "@IsSuccessful";

        public const string NUM_TASK_PROCESSED = "@NumTaskProcessed";

        public const string NUM_TASK_SUCCEEDED = "@NumTaskSucceeded";

        public const string NUM_TASK_FAILED = "@NumTaskFailed";

        public const string MESSAGE = "@Message";

        public const string ERROR_DETAILS = "@ErrorDetails";

    }

    public class StoredProc
    {
        public const string GET_LAST_EXECUTION_DATE_TIME = @"[dbo].[GetLastExecutionDateTime]";

        public const string SAVE_EXECUTION_STATUS = @"[dbo].[SaveExecutionStatus]";
    }

    public class ProcessorStage
    {
        public const string PROCESS_CLIENT_PROFILES = "ProcessClientProfiles";
        public const string PROCESS_ADDRESSES = "ProcessAddresses";
        public const string PROCESS_PHONE_CONTACTS = "ProcessPhoneContacts";
        public const string PROCESS_EMAIL_CONTACTS = "ProcessEmailContacts";
        public const string PROCESS_CASES = "ProcessCases";
        public const string PROCESS_NOTES = "ProcessNotes";
    }
}
