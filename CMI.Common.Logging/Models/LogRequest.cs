using System;
using System.Collections.Generic;
using System.Text;

namespace CMI.Common.Logging
{
    public class LogRequest
    {
        public string OperationName { get; set; }

        public string MethodName { get; set; }

        public string Message { get; set; }

        public int? ErrorType { get; set; }

        public Exception Exception { get; set; }

        public string CustomParams { get; set; }
    }

    public enum LogLevel
    {
        Debug = 1,
        Info = 2,
        Warning = 3,
        Error = 4
    }
}
