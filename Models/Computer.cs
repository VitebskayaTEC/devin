using System.Collections.Generic;

namespace Devin.Models
{
    public class Computer : Device
    {
        public List<Device> Devices { get; set; } = new List<Device>();
    }
}
