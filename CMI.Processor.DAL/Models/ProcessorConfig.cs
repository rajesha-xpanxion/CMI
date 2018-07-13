using System;
using System.Collections.Generic;
using System.Text;

namespace CMI.Processor.DAL
{
    public class ProcessorConfig
    {
        public string ApplicationName { get; set; }
        public string CMIDBConnString { get; set; }
        public string ExecutionStatusReportReceiverEmailAddresses { get; set; }
        public string ExecutionStatusReportEmailSubject { get; set; }
    }
}
