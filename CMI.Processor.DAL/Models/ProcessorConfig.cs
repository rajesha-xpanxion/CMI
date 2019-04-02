using System.Collections.Generic;

namespace CMI.Processor.DAL
{
    public class ProcessorConfig
    {
        public string ApplicationName { get; set; }
        public string CmiDbConnString { get; set; }
        public string ExecutionStatusReportReceiverEmailAddresses { get; set; }
        public string ExecutionStatusReportEmailSubject { get; set; }
        public string ProcessorTypesToExecute { get; set; }
        public InboundProcessorConfig InboundProcessorConfig { get; set; }
        public OutboundProcessorConfig OutboundProcessorConfig { get; set; }
    }

    public class InboundProcessorConfig
    {
        public IEnumerable<string> StagesToProcess { get; set; }
    }

    public class OutboundProcessorConfig
    {
        public string ProcessorTypeToExecute { get; set; }
        public IEnumerable<string> ActivityTypesToProcess { get; set; }
    }
}
