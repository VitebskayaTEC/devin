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
                Devices = db.Devices.Where(x => !x.IsDeleted && x.ComputerId == Id).ToList();
            }
        }
    }
}