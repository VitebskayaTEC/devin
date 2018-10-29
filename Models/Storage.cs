using System;

namespace Devin.Models
{
    public class Storage
    {
        public string Name { get; set; }

        [Slapper.AutoMapper.Id]
        public int Inventory { get; set; }

        public string Type { get; set; }

        public float Cost { get; set; }

        public int Nall { get; set; }

        public int Nstorage { get; set; }

        public int Nrepairs { get; set; }

        public int Noff { get; set; }

        public DateTime Date { get; set; }

        public bool IsDeleted { get; set; }

        public string Account { get; set; }

        public int CartridgeId { get; set; }

        public int FolderId { get; set; }

        public bool IsOff() => (Nall == Noff) && (Nstorage + Nrepairs == 0);

        public string Led()
        {
            if (Nall != (Nstorage + Nrepairs + Noff)) return "warning";
            else if (Nall == Noff) return "off";
            else if (Nstorage == 0 || Nrepairs > 0) return "onwork";
            else return "on";
        }

        public float RealCost()
        {
            var realCost = Cost;

            if (Cost == 0)
            {
                realCost = 24.5F;
            }

            if (Account == "10.5.1")
            {
                realCost = realCost * 1.2F;
            }
            else
            {
                realCost = realCost * 2.4F;
            }

            return realCost;
        }

        public Cartridge Cartridge { get; set; }
    }
}