﻿
namespace CMI.Common.Notification
{
    public class TaskExecutionStatus
    {
        public ProcessorType ProcessorType { get; set; }
        public bool IsSuccessful { get; set; }
        public string TaskName { get; set; }


        public int AutomonReceivedRecordCount { get; set; }
        public int NexusAddRecordCount { get; set; }
        public int NexusUpdateRecordCount { get; set; }
        public int NexusDeleteRecordCount { get; set; }
        public int NexusFailureRecordCount { get; set; }


        public int NexusReceivedMessageCount { get; set; }
        public int AutomonAddMessageCount { get; set; }
        public int AutomonUpdateMessageCount { get; set; }
        public int AutomonDeleteMessageCount { get; set; }
        public int AutomonFailureMessageCount { get; set; }
    }
}
