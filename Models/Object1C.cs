using LinqToDB.Mapping;
using System;

namespace Devin.Models
{
    [Table(Name = "Objects1C")]
    public class Object1C
    {
        [Column, NotNull]
        public string Inventory { get; set; }

        [Column]
        public string Description { get; set; } = "";

        [Column]
        public string Mol { get; set; } = "";

        [Column]
        public string Guild { get; set; } = "";

        [Column]
        public string SubDivision { get; set; } = "";

        [Column]
        public string Account { get; set; } = "";

        [Column]
        public string Location { get; set; } = "";

        [Column]
        public string Unit { get; set; } = "";
        
        [Column, DataType("datetime")]
        public DateTime? Date { get; set; }

        [Column]
        public int Rest { get; set; } = 0;

        [Column]
        public float BalanceCost { get; set; } = 0;

        [Column]
        public float DepreciationCost { get; set; } = 0;

        [Column]
        public float RestCost { get; set; } = 0;

        [Column]
        public float Gold { get; set; } = 0;

        [Column]
        public float Silver { get; set; } = 0;

        [Column]
        public float Platinum { get; set; } = 0;

        [Column]
        public float Palladium { get; set; } = 0;

        [Column]
        public float Mpg { get; set; } = 0;

        [Column]
        public bool IsHide { get; set; }

        [Column]
        public bool IsChecked { get; set; }
    }
}