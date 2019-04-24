using System;
using System.Collections.Generic;

namespace CMI.Processor.DAL
{
    public interface IProcessorProvider
    {
        DateTime? GetLastExecutionDateTime(ProcessorType processorType = ProcessorType.Inbound);

        void SaveExecutionStatus(ExecutionStatus executionStatus);

        IEnumerable<OutboundMessageDetails> SaveOutboundMessages(IEnumerable<OutboundMessageDetails> outboundMessages, DateTime receivedOn);

        IEnumerable<OutboundMessageDetails> GetFailedOutboundMessages();
    }
}
