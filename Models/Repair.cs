using LinqToDB.Mapping;
using System;

namespace Devin.Models
{
    [Table(Name = "Repairs")]
    public class Repair
    {
        [Slapper.AutoMapper.Id]
        [Column, PrimaryKey, Identity, NotNull]
        public int Id { get; set; }

        [Column]
        public int DeviceId { get; set; }

        [Column]
        public int StorageId { get; set; }

        [Column]
        public int Number { get; set; }

        [Column, DataType("datetime")]
        public DateTime Date { get; set; }

        [Column]
        public bool IsOff { get; set; }

        [Column, NotNull]
        public bool IsVirtual { get; set; }

        [Column]
        public string Author { get; set; }

        [Column]
        public int WriteoffId { get; set; }

        [Column]
        public int FolderId { get; set; }


        public float StoragePrice { get; set; }

        public Device Device { get; set; }

        public Storage Storage { get; set; }
    }
}
