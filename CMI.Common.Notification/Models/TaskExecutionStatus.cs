using System;
using System.Collections.Generic;
using System.Text;

namespace CMI.Common.Notification
{
    public class TaskExecutionStatus
    {
        public bool IsSuccessful { get; set; }
        public string TaskName { get; set; }
        public int SourceReceivedRecordCount { get; set; }
        public int DestAddRecordCount { get; set; }
        public int DestUpdateRecordCount { get; set; }
        public int DestDeleteRecordCount { get; set; }
        public int DestFailureRecordCount { get; set; }
    }
}
