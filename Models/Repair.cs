using System;

namespace Devin.Models
{
    public class Repair
    {
        [Slapper.AutoMapper.Id]
        public int Id { get; set; }

        public int DeviceId { get; set; }

        public int StorageId { get; set; }

        public int Number { get; set; }

        public DateTime Date { get; set; }

        public bool IsOff { get; set; }

        public bool IsVirtual { get; set; }

        public string Author { get; set; }

        public int WriteoffId { get; set; }

        public int FolderId { get; set; }

        public float StoragePrice { get; set; }

        public Device Device { get; set; }

        public Storage Storage { get; set; }
    }
}
