using System.Collections.Generic;

namespace CMI.Processor.DAL
{
    public class ProcessorConfig
    {
        public string ApplicationName { get; set; }
        public string CmiDbConnString { get; set; }
        public string ExecutionStatusReportReceiverEmailAddresses { get; set; }
        public string ExecutionStatusReportEmailSubject { get; set; }
        public IEnumerable<string> InboundProcessorStagesToProcess { get; set; }
    }
}
