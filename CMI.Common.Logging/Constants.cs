﻿
namespace CMI.Common.Logging
{
    public static class SqlParamName
    {
        public static string LogLevel { get { return "@LogLevel"; } }

        public static string OperationName { get { return "@OperationName"; } }

        public static string MethodName { get { return "@MethodName"; } }

        public static string ErrorType { get { return "@ErrorType"; } }

        public static string Message { get { return "@Message"; } }

        public static string StackTrace { get { return "@StackTrace"; } }

        public static string CustomParams { get { return "@CustomParams"; } }

        public static string AutomonData { get { return "@AutomonData"; } }

        public static string NexusData { get { return "@NexusData"; } }
    }

    public static class SqlQuery
    {
        public static string SaveLogDetails { get { return @"[dbo].[SaveLogDetails]"; } }
    }
}
