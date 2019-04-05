
namespace CMI.Common.Notification
{
    public static class Constants
    {
        #region Template File Names
        public static string ProcessorExecutionStatusReportEmailTemplateFileName { get { return "ProcessorExecutionStatusReportEmailTemplate.html"; } }
        #endregion

        #region Template Variable Names
        public static string TemplateVariableApplicationName { get { return "{#application-name#}"; } }
        public static string TemplateVariableInboundExecutionStatusDetails { get { return "{#inbound-execution-status-details#}"; } }
        public static string NoInboundExecutionHtml { get { return "<tr><td colspan='7'>Inbound integration not executed.</td></tr>"; } }
        public static string TemplateVariableOutboundExecutionStatusDetails { get { return "{#outbound-execution-status-details#}"; } }
        public static string NoOutboundExecutionHtml { get { return "<tr><td colspan='7'>Outbound integration not executed.</td></tr>"; } }
        #endregion

        #region Media Types
        public static string MediaTypeHtml { get { return "text/html"; } }
        public static string MediaTypePlain { get { return "text/plain"; } }
        #endregion

        #region Other
        public static string[] EmailAddressSeparators { get { return new string[] { ";", ",", "|" }; } }
        public static string ExecutionStatusReportEmailBodyTableFormat { get { return "<tr><td>{0}</td><td>{1}</td><td>{2}</td><td>{3}</td><td>{4}</td><td>{5}</td><td>{6}</td></tr>"; } }
        #endregion
    }
}
