using System;
using System.Collections.Generic;

namespace Devin.Models
{
    public class Aida
    {
        public int ID { get; set; }

        public string RHost { get; set; }

        public string RUser { get; set; }

        public string RLocation { get; set; }

        public DateTime RDateTime { get; set; }
    }

    public class AidaComputer
    {
        public int ID { get; set; }

        public string RHost { get; set; }

        public string RUser { get; set; }

        public string RLocation { get; set; }

        public DateTime RDateTime { get; set; }

        public string IField { get; set; }

        public string IValue { get; set; }

        public List<AidaField> Items { get; set; } = new List<AidaField>();
    }

    public class AidaField
    {
        public int INum { get; set; }

        public string IPage { get; set; }

        public string IDevice { get; set; }

        public string IGroup { get; set; }

        public string IField { get; set; }

        public string IValue { get; set; }
    }
}