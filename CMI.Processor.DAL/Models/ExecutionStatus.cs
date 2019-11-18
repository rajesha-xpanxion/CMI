using System;

namespace CMI.Processor.DAL
{
    public class ExecutionStatus
    {
        public ProcessorType ProcessorType { get; set; }
        public DateTime ExecutedOn { get; set; }
        public bool IsSuccessful { get; set; }
        public int NumTaskProcessed { get; set; }
        public int NumTaskSucceeded { get; set; }
        public int NumTaskFailed { get; set; }
        public string ExecutionStatusMessage { get; set; }
        public string ErrorDetails { get; set; }
        public bool IsExecutedInIncrementalMode { get; set; }
    }
}
