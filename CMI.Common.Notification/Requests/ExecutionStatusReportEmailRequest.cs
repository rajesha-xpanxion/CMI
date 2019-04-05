using System.Collections.Generic;

namespace CMI.Common.Notification
{
    public class ExecutionStatusReportEmailRequest : BaseEmailRequest
    {
        public ProcessorType ProcessorType { get; set; }
        public IEnumerable<TaskExecutionStatus> TaskExecutionStatuses { get; set; }
    }
}
