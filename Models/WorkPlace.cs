using System.Collections.Generic;

namespace Devin.Models
{
    public class WorkPlace
    {
        [Slapper.AutoMapper.Id]
        public int Id { get; set; }

        public string Location { get; set; }

        public string Guild { get; set; }

        public virtual IEnumerable<Device> Devices { get; set; } = new List<Device>();
    }
}