using System.Collections.Generic;
using System.Linq;

namespace Devin.Models
{
    public class Cartridge
    {
        // N
        [Slapper.AutoMapper.Id]
        public int Id { get; set; }

        // Caption
        public string Name { get; set; }

        public string Caption { get; set; }

        // Price
        public float Cost { get; set; }

        public string Type { get; set; }

        public string Color { get; set; }

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