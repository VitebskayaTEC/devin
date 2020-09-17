using LinqToDB.Mapping;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Devin.Models
{
    [Table(Name = "Devices")]
    public class Device
    {
        [Column, PrimaryKey, Identity, NotNull]
        public int Id { get; set; }

        [Column]
        public string Inventory { get; set; }

        [Column]
        public string Type { get; set; }

        [Column]
        public string Name { get; set; }

        [Column]
        public string PublicName { get; set; }

        [Column]
        public int ComputerId { get; set; }

        [Column, AllowHtml]
        public string Description { get; set; }

        [Column]
        public string Description1C { get; set; }

        [Column, DataType("datetime")]
        public DateTime DateInstall { get; set; }

        [Column, DataType("datetime")]
        public DateTime? DateLastRepair { get; set; }

        [Column]
        public string SerialNumber { get; set; }

        [Column]
        public string PassportNumber { get; set; }

        [Column]
        public string Location { get; set; }

        [Column]
        public string Mol { get; set; }

        [Column]
        public int PrinterId { get; set; }

        [Column]
        public int FolderId { get; set; }

        [Column]
        public bool IsOff { get; set; }

        [Column, NotNull]
        public bool IsDeleted { get; set; }

        [Column]
        public string ServiceTag { get; set; }

        [Column(Name = "PassportGold")]
        public string Gold { get; set; }

        [Column(Name = "PassportSilver")]
        public string Silver { get; set; }

        [Column(Name = "PassportPlatinum")]
        public string Platinum { get; set; }

        [Column(Name = "PassportMPG")]
        public string MPG { get; set; }

        public string Palladium { get; set; } = "";

        [Column]
        public int PlaceId { get; set; }

        [Column]
        public string NetworkName { get; set; }

        [Column]
        public string Files { get; set; } = "";

        [Column]
        public string Users { get; set; } = "";


        public Printer Printer { get; set; }

        public Object1C Object1C { get; set; }

        public virtual WorkPlace Place { get; set; }

        public IEnumerable<Cartridge> Cartridges { get; set; } = new List<Cartridge>();

        public virtual IEnumerable<Repair> Repairs { get; set; } = new List<Repair>();
    }
}
