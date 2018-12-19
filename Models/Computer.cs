using Dapper;
using System.Collections.Generic;

namespace Devin.Models
{
    public class Computer : Device
    {
        public List<Device> Devices { get; set; } = new List<Device>();

        public void Load()
        {
            using (var conn = Database.Connection())
            {
                Devices = conn.Query<Device>("SELECT * FROM Devices WHERE IsDeleted = 0 AND ComputerId = @Id", new { Id }).AsList();
            }
        }
    }
}
