using LinqToDB.Mapping;

namespace Devin.Models.Site
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