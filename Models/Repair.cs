namespace Devin.Models
{
    public class Repair
    {
        public int Id { get; set; }

        public string DeviceNumber { get; set; }

        public string DeviceInventory { get; set; }

        public string StorageInventory { get; set; }

        public string StorageName { get; set; }

        public int StorageCount { get; set; }

        public float StoragePrice { get; set; }

        public string StorageList { get; set; }

        public int WriteoffId { get; set; }
    }
}
