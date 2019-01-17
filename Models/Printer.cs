using LinqToDB.Mapping;

namespace Devin.Models
{
    [Table(Name = "Cartridges")]
    public class Printer
    {
        [Slapper.AutoMapper.Id]
        [Column, PrimaryKey, Identity, NotNull]
        public int Id { get; set; }

        [Column]
        public string Name { get; set; }

        [Column]
        public string Description { get; set; }
    }
}