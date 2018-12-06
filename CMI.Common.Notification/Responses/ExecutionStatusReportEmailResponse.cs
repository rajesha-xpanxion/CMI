using System;

namespace CMI.Common.Notification
{
    public class ExecutionStatusReportEmailResponse
    {
        public bool IsSuccessful { get; set; }

        public Exception Exception { get; set; }
    }
}
