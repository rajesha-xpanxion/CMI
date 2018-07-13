using System;
using System.Collections.Generic;
using System.Text;

namespace CMI.Processor.DAL
{
    public class ExecutionStatus
    {
        public DateTime ExecutedOn { get; set; }
        public bool IsSuccessful { get; set; }
        public string ExecutionStatusMessage { get; set; }
        public string ErrorDetails { get; set; }
    }
}
