using System;
using System.Collections.Generic;

namespace CMI.Processor.DAL
{
    public interface IProcessorProvider
    {
        DateTime? GetLastExecutionDateTime(ProcessorType processorType = ProcessorType.Inbound);

        void SaveExecutionStatus(ExecutionStatus executionStatus);

        void SaveOutboundMessages(IEnumerable<OutboundMessageDetails> outboundMessages);
    }
}
