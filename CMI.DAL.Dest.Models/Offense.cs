using System;
using System.Collections.Generic;
using System.Text;

namespace CMI.DAL.Dest
{
    public class Offense
    {
        public string Label { get; set; }
        public string Date { get; set; }
        public string Statute { get; set; }
        public string Category { get; set; }
        public bool IsPrimary { get; set; }
    }
}
