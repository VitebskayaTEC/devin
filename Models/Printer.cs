namespace Devin.Models
{
    public class Printer
    {
        [Slapper.AutoMapper.Id]
        public int Id { get; set; }

        public string Caption { get; set; }

        public string Description { get; set; }
    }
}