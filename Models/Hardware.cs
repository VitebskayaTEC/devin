using LinqToDB.Mapping;
using System;

namespace Devin.Models
{
    [Table(Name = "Hardware")]
    public class Hardware
    {
        [Column, NotNull, Identity]
        public int Id { get; set; }

        [Column, NotNull]
        public int FolderId { get; set; }

        [Column, NotNull]
        public string Name { get; set; }

        [Column, NotNull]
        public string Description { get; set; }

        [Column, NotNull]
        public string Interface { get; set; }

        [Column, NotNull]
        public string Files { get; set; }

        [Column]
        public string Location { get; set; }

        [Column, NotNull]
        public DateTime Updated { get; set; }

        [Column, NotNull]
        public string Tags { get; set; }
    }
}