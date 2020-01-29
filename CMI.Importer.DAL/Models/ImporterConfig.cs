using System.Collections.Generic;

namespace CMI.Importer.DAL
{
    public class ImporterConfig
    {
        public string ApplicationName { get; set; }
        public string CmiDbConnString { get; set; }
        public string ImporterTypesToExecute { get; set; }
        public InboundImporterConfig InboundImporterConfig { get; set; }
        public OutboundImporterConfig OutboundImporterConfig { get; set; }
    }

    public class InboundImporterConfig
    {
        public string SourceDataFileFullPath { get; set; }
        public IEnumerable<string> ExcelSheetsToImport { get; set; }
    }

    public class OutboundImporterConfig
    {
    }
}
