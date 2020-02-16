using LinqToDB;
using System.Collections.Generic;
using System.Linq;

namespace Devin.Models
{
    public class Computer : Device
    {
        public List<Device> Devices { get; set; } = new List<Device>();

        public void Load()
        {
            using (var db = new DevinContext())
            {
				var _devices = from d in db.Devices
							   where !d.IsDeleted && d.ComputerId == Id
							   select new Device
							   {
								   Id = d.Id,
								   Inventory = d.Inventory,
								   Name = d.Name,
								   IsOff = d.IsOff,
								   PublicName = d.PublicName,
								   Mol = d.Mol,
							   };

				Devices = _devices.ToList();
            }
        }
    }
}