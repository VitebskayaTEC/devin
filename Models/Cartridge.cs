using LinqToDB.Mapping;
using System.Collections.Generic;
using System.Linq;

namespace Devin.Models
{
    [Table(Name = "Cartridges")]
    public class Cartridge
    {
        [Slapper.AutoMapper.Id]
        [Column, PrimaryKey, Identity, NotNull]
        public int Id { get; set; }

        [Column]
        public string Name { get; set; }

        [Column]
        public float Cost { get; set; }

        [Column]
        public string Type { get; set; }

        [Column]
        public string Color { get; set; }

        [Column]
        public string Description { get; set; }


        public virtual int Count { get; set; }

        public virtual int InStorage { get; set; }

        public virtual float DefPrice { get; set; }

        public virtual IEnumerable<Storage> Storages { get; set; } = new List<Storage>();

        public virtual IEnumerable<Repair> Repairs { get; set; } = new List<Repair>();

        public float ApproximateCost()
        {
            float cost = 0;

            if (Storages.Count() > 0)
            { 
                foreach (var storage in Storages)
                {
                    cost += storage.RealCost();
                }

                cost = cost / Storages.Count();
            }

            return cost;
        }
    }
}