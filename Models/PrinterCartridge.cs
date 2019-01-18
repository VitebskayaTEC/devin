using LinqToDB.Mapping;

namespace Devin.Models
{
    [Table(Name = "PrintersCartridges")]
    public class PrinterCartridge
    {
        [Column]
        public int PrinterId { get; set; }

        [Column]
        public int CartridgeId { get; set; }
    }
}