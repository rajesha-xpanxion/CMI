﻿using System;

namespace CMI.MessageProcessor.Model
{
    public class ActionResponse
    {
        public string Reason { get; set; }

        public DateTime OccurredOn { get; set; }

        public string UpdatedBy { get; set; }
    }
}
