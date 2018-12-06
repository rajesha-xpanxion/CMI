using System.Collections.Generic;

namespace CMI.Common.Notification
{
    public class ExecutionStatusReportEmailRequest : BaseEmailRequest
    {
        public IEnumerable<TaskExecutionStatus> TaskExecutionStatuses { get; set; }
    }
}
