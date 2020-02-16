using LinqToDB.Mapping;
using System;

namespace Devin.Models.Site
{
    [Table(Name = "MetalsCosts")]
    public class MetalsCost
    {
        [Column, NotNull, DataType(LinqToDB.DataType.DateTime)]
        public DateTime Date { get; set; }

        [Column, NotNull]
        public string Name { get; set; }

        [Column, NotNull]
        public float Cost { get; set; }

        [Column, NotNull]
        public float Discount { get; set; }
    }
}