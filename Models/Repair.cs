using System;

namespace Devin.Models
{
    public class Repair
    {
        [Slapper.AutoMapper.Id]
        public int Id { get; set; }


        public string DeviceNumber { get; set; }

        public string StorageInventory { get; set; }

        public int Units { get; set; }

        public DateTime Date { get; set; }

        public bool IfSpis { get; set; }

        public bool Virtual { get; set; }

        public string Author { get; set; }

        public int WriteoffId { get; set; }

        public int FolderId { get; set; }


        public string DeviceInventory { get; set; }


        public string StorageName { get; set; }

        public float StoragePrice { get; set; }

        public string StorageList { get; set; }

        public int StorageCount { get; set; }


        public Device Device { get; set; }

        public Storage Storage { get; set; }
    }
}
