using System;
using System.Collections.Generic;
using System.Text;

namespace CMI.Common.Notification
{
    public class Constants
    {
        #region Template File Names
        public const string ProcessorExecutionStatusReportEmailTemplateFileName = "ProcessorExecutionStatusReportEmailTemplate.html";
        #endregion

        #region Template Variable Names
        public const string TemplateVariableApplicationName = "{#application-name#}";
        public const string TemplateVariableExecutionStatusDetails = "{#execution-status-details#}";
        #endregion
    }
}
