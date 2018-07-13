using System;
using System.Collections.Generic;
using System.Text;

namespace CMI.Common.Notification
{
    public class ExecutionStatusReportEmailRequest : BaseEmailRequest
    {
        public IEnumerable<TaskExecutionStatus> TaskExecutionStatuses { get; set; }
    }
}
