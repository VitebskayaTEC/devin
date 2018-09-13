namespace Devin.Models
{
    public class Cartridge
    {
        // N
        public int Id { get; set; }

        // Caption
        public string Name { get; set; }

        // Price
        public float Cost { get; set; }

        public string Type { get; set; }

        public string Color { get; set; }

        public string Description { get; set; }


        public virtual int Count { get; set; }
    }
}