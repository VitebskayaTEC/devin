using LinqToDB.Mapping;

namespace Devin.Models
{
    [Table(Name = "Constants")]
    public class Constant
    {
        [Column]
        public string Keyword { get; set; }

        [Column]
        public string Value { get; set; }
    }
}