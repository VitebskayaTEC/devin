using LinqToDB.Mapping;

namespace Devin.Models
{
    [Table(Name = "TypesCartridges")]
    public class TypeCartridge
    {
        [Column]
        public string Id { get; set; }

        [Column]
        public string Name { get; set; }

        [Column]
        public string Type { get; set; }
    }
}