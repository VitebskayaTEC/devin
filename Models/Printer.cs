using LinqToDB.Mapping;

namespace Devin.Models
{
    [Table(Name = "Printers")]
    public class Printer
    {
        [Column, PrimaryKey, Identity, NotNull]
        public int Id { get; set; }

        [Column]
        public string Name { get; set; }

        [Column]
        public string Description { get; set; }
    }
}