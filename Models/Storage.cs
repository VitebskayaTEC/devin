using LinqToDB.Mapping;
using System;

namespace Devin.Models
{
    [Table(Name = "Storages")]
    public class Storage
    {
        [Slapper.AutoMapper.Id]
        [Column, PrimaryKey, Identity, NotNull]
        public int Id { get; set; }

        [Column]
        public string Name { get; set; }

        [Column, NotNull]
        public string Inventory { get; set; }

        [Column]
        public string Type { get; set; }

        [Column, NotNull]
        public float Cost { get; set; }

        [Column, NotNull]
        public int Nall { get; set; }

        [Column, NotNull]
        public int Nstorage { get; set; }

        [Column, NotNull]
        public int Nrepairs { get; set; }

        [Column, NotNull]
        public int Noff { get; set; }

        [Column, NotNull, DataType("datetime")]
        public DateTime Date { get; set; }

        [Column, NotNull]
        public bool IsDeleted { get; set; }

        [Column]
        public string Account { get; set; }

        [Column]
        public int CartridgeId { get; set; }

        [Column]
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