using LinqToDB.Mapping;
using System;

namespace Devin.Models
{
    [Table(Name = "Activity")]
    public class Activity
    {
        [Column, NotNull, DataType("datetime")]
        public DateTime Date { get; set; }

        [Column, NotNull]
        public string Source { get; set; }

        [Column, NotNull]
        public string Id { get; set; }

        [Column, NotNull]
        public string Text { get; set; }

        [Column, NotNull]
        public string Username { get; set; }
    }
}