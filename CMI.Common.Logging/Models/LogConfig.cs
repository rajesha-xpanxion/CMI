using System;
using System.Collections.Generic;
using System.Text;

namespace CMI.Common.Logging
{
    public class LogConfig
    {
        public bool IsEnabled { get; set; }

        public LogLevel LogLevel { get; set; }

        public string DBConnString { get; set; }
    }
}
