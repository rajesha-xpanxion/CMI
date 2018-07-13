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

        public const string MESSAGE = "@Message";

        public const string ERROR_DETAILS = "@ErrorDetails";

    }

    public class SQLQuery
    {
        public const string GET_LAST_EXECUTION_DATE_TIME = @"[dbo].[GetLastExecutionDateTime]";

        public const string SAVE_EXECUTION_STATUS = @"[dbo].[SaveExecutionStatus]";
    }
}
