using LinqToDB.Mapping;

namespace Devin.Models
{
    [Table(Name = "TypesWriteoffs")]
    public class TypeWriteoff
    {
        [Column]
        public string Id { get; set; }

        [Column]
        public string Name { get; set; }

        [Column]
        public string Captions { get; set; }

        [Column, NotNull]
        public int FieldsCount { get; set; }

        [Column]
        public string Template { get; set; }

        [Column]
        public string DefaultData { get; set; }
    }
}