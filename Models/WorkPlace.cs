using LinqToDB.Mapping;
using System.Collections.Generic;

namespace Devin.Models
{
    [Table(Name = "WorkPlaces")]
    public class WorkPlace
    {
        [Column, Identity]
        public int Id { get; set; }

        [Column, NotNull]
        public string Location { get; set; }

        [Column]
        public string Guild { get; set; }

        public virtual IEnumerable<Device> Devices { get; set; } = new List<Device>();
    }
}