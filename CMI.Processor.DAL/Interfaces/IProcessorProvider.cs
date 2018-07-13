using System;
using System.Collections.Generic;
using System.Text;

namespace CMI.Processor.DAL
{
    public interface IProcessorProvider
    {
        DateTime GetLastExecutionDateTime();

        void SaveExecutionStatus(ExecutionStatus executionStatus);
    }
}
