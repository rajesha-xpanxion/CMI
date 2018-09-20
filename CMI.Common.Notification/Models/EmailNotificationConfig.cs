using System;
using System.Collections.Generic;
using System.Text;

namespace CMI.Common.Notification
{
    public class EmailNotificationConfig
    {
        public bool IsEmailNotificationEnabled { get; set; }
        public string SMTPServerHost { get; set; }
        public string MailServerUserName { get; set; }
        public string MailServerPassword { get; set; }
        public bool IsEnableSsl { get; set; }
        public string FromEmailAddress { get; set; }
        public string EmailAlertTemplatesPath { get; set; }
    }
}
