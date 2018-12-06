using System;

namespace CMI.Processor.DAL
{
    public interface IProcessorProvider
    {
        DateTime? GetLastExecutionDateTime();

        void SaveExecutionStatus(ExecutionStatus executionStatus);
    }
}
