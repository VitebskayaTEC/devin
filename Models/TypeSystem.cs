using LinqToDB.Mapping;

namespace Devin.Models
{
    [Table(Name = "TypesSystems")]
    public class TypeSystem
    {
        [Column]
        public string Id { get; set; }

        [Column]
        public string Name { get; set; }
    }
}