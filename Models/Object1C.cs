namespace Devin.Models
{
    public class Object1C
    {
        [Slapper.AutoMapper.Id]
        public string Inventory { get; set; }

        public string InventoryNew { get; set; }

        public string Description { get; set; } = "";

        public string Mol { get; set; } = "";

        public string Guild { get; set; } = "";

        public string SubDivision { get; set; } = "";

        public string Account { get; set; } = "";

        public string Location { get; set; } = "";

        public float Gold { get; set; } = 0;

        public float Silver { get; set; } = 0;

        public float Platinum { get; set; } = 0;

        public float Palladium { get; set; } = 0;

        public float Mpg { get; set; } = 0;

        public float BalanceCost { get; set; } = 0;

        public float DepreciationCost { get; set; } = 0;

        public float RestCost { get; set; } = 0;

        public float Rest { get; set; } = 0;
    }
}