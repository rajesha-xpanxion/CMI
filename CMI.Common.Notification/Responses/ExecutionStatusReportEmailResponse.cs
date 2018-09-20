using System;
using System.Collections.Generic;
using System.Text;

namespace CMI.Common.Notification
{
    public class ExecutionStatusReportEmailResponse
    {
        public bool IsSuccessful { get; set; }

        public Exception Exception { get; set; }
    }
}
