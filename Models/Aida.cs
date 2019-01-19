using LinqToDB.Mapping;
using System;
using System.Collections.Generic;

namespace Devin.Models
{
    [Table(Name = "Report")]
    public class Report
    {
        [Column, PrimaryKey, NotNull]
        public int ID { get; set; }

        [Column]
        public string RHost { get; set; }

        [Column]
        public string RUser { get; set; }

        [Column]
        public string RLocation { get; set; }

        [Column, DataType("datetime")]
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

        public List<Item> Items { get; set; } = new List<Item>();
    }

    [Table(Name = "Item")]
    public class Item
    {
        [Column]
        public int INum { get; set; }

        [Column]
        public string IPage { get; set; }

        [Column]
        public string IDevice { get; set; }

        [Column]
        public string IGroup { get; set; }

        [Column]
        public string IField { get; set; }

        [Column]
        public string IValue { get; set; }

        [Column]
        public int IIcon { get; set; }

        [Column]
        public int IID { get; set; }

        [Column, NotNull]
        public int ReportID { get; set; }
    }
}