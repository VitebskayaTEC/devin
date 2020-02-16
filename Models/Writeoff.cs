using LinqToDB.Mapping;
using LinqToDB;
using System.Linq;
using System;
using System.Collections.Generic;

namespace Devin.Models
{
    [Table(Name = "Writeoffs")]
    public class Writeoff
    {
        [Column, PrimaryKey, Identity, NotNull]
        public int Id { get; set; }

        [Column]
        public int CostArticle { get; set; }

        [Column, NotNull, DataType("datetime")]
        public DateTime Date { get; set; }

        [Column, DataType("datetime")]
        public DateTime? LastExcelDate { get; set; }

        [Column]
        public string LastExcel { get; set; }

        [Column]
        public string Params { get; set; }

        [Column]
        public string Type { get; set; }

        [Column]
        public string Name { get; set; }

        [Column]
        public string Description { get; set; }

		[Column]
		public int? BoardsCount { get; set; }

		[Column]
		public float? BoardsWeight { get; set; }

		public string Template { get; set; }

        [Column]
        public string Mark { get; set; }

        public string DefaultData { get; set; }

        [Column]
        public int FolderId { get; set; }

        public List<Repair> Repairs { get; set; } = new List<Repair>();

        public string[] Parameters => (Params ?? "").Split(new string[] { ";;" }, StringSplitOptions.None);

        public float AllCost()
        {
            float cost = 0;
            foreach (var repair in Repairs)
            {
                cost += repair.Number * repair.Storage.Cost;
            }
            return cost;
        }

        public void Load()
        {
            using (var db = new DevinContext())
            {
                var query = from r in db.Repairs
                            from d in db.Devices.Where(x => x.Id == r.DeviceId).DefaultIfEmpty()
                            from s in db.Storages.Where(x => x.Id == r.StorageId).DefaultIfEmpty()
                            where r.WriteoffId == Id
                            orderby r.FolderId, r.WriteoffId, r.Date descending
                            select new Repair
                            {
                                Id = r.Id,
                                DeviceId = r.DeviceId,
                                StorageId = r.StorageId,
                                Number = r.Number,
                                Date = r.Date,
                                Author = r.Author,
                                FolderId = r.FolderId,
                                WriteoffId = r.WriteoffId,
                                IsOff = r.IsOff,
                                IsVirtual = r.IsVirtual,
                                Device = new Device
                                {
                                    Id = d.Id,
                                    Inventory = d.Inventory,
                                    Name = d.Name,
                                    PublicName = d.PublicName,
                                    Type = d.Type
                                },
                                Storage = new Storage
                                {
                                    Id = s.Id,
                                    Inventory = s.Inventory,
                                    Name = s.Name,
                                    Nall = s.Nall,
                                    Nstorage = s.Nstorage,
                                    Nrepairs = s.Nrepairs,
                                    Noff = s.Noff,
                                    Cost = s.Cost
                                }
                            };

                Repairs = query.ToList();
            }
        }
    }
}