using System;

namespace CMI.Processor.DAL
{
    public class LastExecutionStatus
    {
        public DateTime? LastIncrementalModeExecutionDateTime { get; set; }
        public DateTime? LastNonIncrementalModeExecutionDateTime { get; set; }
    }
}
