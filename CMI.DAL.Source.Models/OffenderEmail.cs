﻿using System;
using System.Collections.Generic;
using System.Text;

namespace CMI.DAL.Source
{
    public class OffenderEmail : Offender
    {
        public int Id { get; set; }

        public string EmailAddress { get; set; }

        public bool IsPrimary { get; set; }

        public bool IsActive { get; set; }
    }
}
