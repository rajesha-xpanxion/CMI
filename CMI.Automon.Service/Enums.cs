using System;
using System.Collections.Generic;
using System.Text;

namespace CMI.Automon.Service
{
    public enum EventStatus
    {
        Pending = 0,

        Missed = 16,

        Cancelled = 10,

        Complete = 2
    }
}
