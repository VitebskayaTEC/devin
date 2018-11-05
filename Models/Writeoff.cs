using Dapper;
using System;
using System.Collections.Generic;

namespace Devin.Models
{
    public class Writeoff
    {
        public int Id { get; set; }

        public int CostArticle { get; set; }

        public DateTime Date { get; set; }

        public DateTime LastExcelDate { get; set; }

        public string LastExcel { get; set; }

        public string Params { get; set; }

        public string Type { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string DefExcel { get; set; }

        public string Template { get; set; }

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
            using (var conn = Database.Connection())
            {
                Repairs = new List<Repair>();

                IEnumerable<dynamic> raw = conn.Query($@"SELECT
                        Repairs.*
                        ,Devices.Id          AS Device_Id
                        ,Devices.Inventory   AS Device_Inventory
                        ,Devices.Name        AS Device_Name
                        ,Devices.PublicName  AS Device_PublicName
                        ,Devices.Type        AS Device_Type
                        ,Storages.Id         AS Storage_Id
                        ,Storages.Inventory  AS Storage_Inventory
                        ,Storages.Name       AS Storage_Name
                        ,Storages.Nall       AS Storage_Nall
                        ,Storages.Nstorage   AS Storage_Nstorage
                        ,Storages.Nrepairs   AS Storage_Nrepairs
                        ,Storages.Noff       AS Storage_Noff
                        ,Storages.Cost       AS Storage_Cost
                    FROM Repairs
                    LEFT OUTER JOIN Devices  ON Repairs.DeviceId  = Devices.Id
                    LEFT OUTER JOIN Storages ON Repairs.StorageId = Storages.Id
                    WHERE Repairs.WriteoffId = @Id
                    ORDER BY Repairs.FolderId, Repairs.WriteoffId, Repairs.Date DESC", new { Id });

                foreach (dynamic row in raw)
                {
                    Repairs.Add(new Repair
                    {
                        Id = row.Id,
                        DeviceId = Convert.ToInt32(row.DeviceId),
                        StorageId = Convert.ToInt32(row.StorageId),
                        Date = row.Date,
                        Author = row.Author,
                        FolderId = Convert.ToInt32(row.FolderId),
                        WriteoffId = Convert.ToInt32(row.WriteoffId),
                        IsOff = row.IsOff,
                        IsVirtual = row.IsVirtual,
                        Number = Convert.ToInt32(row.Number),
                        Device = new Device
                        {
                            Id = Convert.ToInt32(row.Device_Id),
                            Inventory = row.Device_Inventory,
                            Name = row.Device_Name,
                            PublicName = row.Device_PublicName,
                            Type = row.Device_Type
                        },
                        Storage = new Storage
                        {
                            Id = Convert.ToInt32(row.Storage_Id),
                            Inventory = row.Storage_Inventory,
                            Name = row.Storage_Name,
                            Nall = Convert.ToInt32(row.Storage_Nall),
                            Nstorage = Convert.ToInt32(row.Storage_Nstorage),
                            Nrepairs = Convert.ToInt32(row.Storage_Nrepairs),
                            Noff = Convert.ToInt32(row.Storage_Noff),
                            Cost = Convert.ToSingle(row.Storage_Cost)
                        }
                    });
                }
            }
        }
    }
}