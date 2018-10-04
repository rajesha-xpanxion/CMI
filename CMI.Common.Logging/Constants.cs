using System;
using System.Collections.Generic;
using System.Text;

namespace CMI.Common.Logging
{
    public class Constants
    {
    }

    public class SQLParamName
    {
        public const string LOG_LEVEL = "@LogLevel";

        public const string OPERATION_NAME = "@OperationName";

        public const string METHOD_NAME = "@MethodName";

        public const string ERROR_TYPE = "@ErrorType";

        public const string MESSAGE = "@Message";

        public const string STACK_TRACE = "@StackTrace";

        public const string CUSTOM_PARAMS = "@CustomParams";

        public const string SOURCE_DATA = "@SourceData";

        public const string DEST_DATA = "@DestData";
    }

    public class SQLQuery
    {
        public const string SAVE_LOG_DETAILS = @"[dbo].[SaveLogDetails]";
    }
}
