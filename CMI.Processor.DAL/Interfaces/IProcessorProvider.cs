using System;

namespace CMI.Processor.DAL
{
    public interface IProcessorProvider
    {
        DateTime? GetLastExecutionDateTime(ProcessorType processorType = ProcessorType.Inbound);

        void SaveExecutionStatus(ExecutionStatus executionStatus);
    }
}
