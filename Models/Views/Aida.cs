using System;
using System.Collections.Generic;

namespace Devin.Models.Views
{
    public class Aida
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public string User { get; set; }
        public string UserName { get; set; }
        public DateTime LastReport { get; set; }
        public decimal TotalScore { get; set; }

        public string OS { get; set; }
        public decimal OSScore { get; set; }

        public string Cpu { get; set; }
        public int CpuCore { get; set; }
        public int CpuFrequency { get; set; }
        public decimal CpuScore { get; set; }

        public string Motherboard { get; set; }
        public int RamType { get; set; }
        public decimal RamValue { get; set; }
        public decimal RamScore { get; set; }

        public string Disk { get; set; }
        public string DiskType { get; set; }
        public decimal DiskCapacity { get; set; }
        public decimal DiskScore { get; set; }
        public string DiskValue { get; set; }

        public List<decimal> DisplaysSize { get; set; } = new List<decimal>();
        public List<string> DisplaysCaptions { get; set; } = new List<string>();
        public decimal DisplayScore { get; set; }
    }
}