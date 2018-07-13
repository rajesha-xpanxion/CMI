using System;
using System.Collections.Generic;
using System.Text;

namespace CMI.Common.Notification
{
    public interface IEmailNotificationProvider
    {
        bool SendExecutionStatusReportEmail(ExecutionStatusReportEmailRequest request);
    }
}
