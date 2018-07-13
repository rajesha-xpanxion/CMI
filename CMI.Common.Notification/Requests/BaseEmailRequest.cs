using System;
using System.Collections.Generic;
using System.Text;

namespace CMI.Common.Notification
{
    public class BaseEmailRequest
    {
        public string Subject { get; set; }

        public string ToEmailAddress { get; set; }
    }
}
