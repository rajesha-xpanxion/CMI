using System;
using System.Collections.Generic;
using System.Text;

namespace CMI.DAL.Source.Models
{
    public class SourceConfig
    {
        public bool IsDevMode { get; set; }

        public string AutoMonDBConnString { get; set; }

        public string TestDataJSONRepoPath { get; set; }
    }
}
