using System;
using System.Collections.Generic;

namespace CMI.Processor.DAL
{
    public interface IProcessorProvider
    {
        LastExecutionStatus GetLastExecutionDateTime(ProcessorType processorType = ProcessorType.Inbound);

        void SaveExecutionStatus(ExecutionStatus executionStatus);

        IEnumerable<OutboundMessageDetails> SaveOutboundMessagesToDatabase(IEnumerable<OutboundMessageDetails> outboundMessages);

        void SaveOutboundMessagesToDisk(IEnumerable<OutboundMessageDetails> outboundMessages);

        IEnumerable<OutboundMessageDetails> GetOutboundMessagesFromDisk();
    }
}
