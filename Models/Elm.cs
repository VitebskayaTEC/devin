using LinqToDB.Mapping;
using System;

namespace Devin.Models
{
    [Table(Name = "ElmEvents")]
    public class Elm
    {
        [Column, DataType("datetime")]
        public DateTime EDATE { get; set; }

        [Column]
        public string EVGR { get; set; }

        [Column]
        public string EVENTS { get; set; }

        [Column]
        public string CNAME { get; set; }

        [Column]
        public string CUSER { get; set; }
    }
}