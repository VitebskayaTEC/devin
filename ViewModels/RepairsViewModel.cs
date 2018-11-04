using Dapper;
using Devin.Models;
using System;
using System.Collections.Generic;

namespace Devin.ViewModels
{
    public class RepairsViewModel
    {
        public List<Folder> Folders { get; set; } = new List<Folder>();

        public List<Writeoff> Writeoffs { get; set; } = new List<Writeoff>();

        public List<Repair> Repairs { get; set; } = new List<Repair>();

        public RepairsViewModel(string Search = "")
        {
            using (var conn = Database.Connection())
            {
                if (!string.IsNullOrEmpty(Search))
                {
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
                    LEFT OUTER JOIN Devices   ON Repairs.DeviceId  = Devices.Id
                    LEFT OUTER JOIN Storages  ON Repairs.StorageId = Storages.Id
                    WHERE Devices.Name        LIKE '%{Search}%' 
                       OR Devices.Description LIKE '%{Search}%'
                       OR Devices.Inventory   LIKE '%{Search}%'
                       OR Storages.Name       LIKE '%{Search}%'
                       OR Storages.Inventory  LIKE '%{Search}%'
                    ORDER BY Repairs.FolderId, Repairs.WriteoffId, Repairs.Date DESC");

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
                else
                {
                    IEnumerable<dynamic> raw = conn.Query($@"SELECT
                        Repairs.*

                        ,Devices.Id          AS Device_Id
                        ,Devices.Inventory   AS Device_Inventory
                        ,Devices.Name        AS Device_Name
                        ,Devices.PublicName  AS Device_PublicName
                        ,Devices.Type        AS Device_Type

                        ,Storages.Id        AS Storage_Id
                        ,Storages.Inventory AS Storage_Inventory
                        ,Storages.Name      AS Storage_Name
                        ,Storages.Nall      AS Storage_Nall
                        ,Storages.Nstorage  AS Storage_Nstorage
                        ,Storages.Nrepairs  AS Storage_Nrepairs
                        ,Storages.Noff      AS Storage_Noff
                        ,Storages.Cost      AS Storage_Cost
                    FROM Repairs
                    LEFT OUTER JOIN Devices   ON Repairs.DeviceId  = Devices.Id
                    LEFT OUTER JOIN Storages  ON Repairs.StorageId = Storages.Id
                    ORDER BY Repairs.FolderId, Repairs.WriteoffId, Repairs.Date DESC");

                    List<Repair> _repairs = new List<Repair>();

                    foreach (dynamic row in raw)
                    {
                        _repairs.Add(new Repair
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

                    List<Folder> _folders = conn.Query<Folder>(@"SELECT
                        Folders.Id,
                        CASE WHEN Parents.Id IS NULL THEN 0 ELSE Parents.Id END AS FolderId,
                        Folders.Name
                    FROM Folders
                    LEFT OUTER JOIN Folders AS Parents ON Folders.FolderId = Parents.Id
                    WHERE Folders.Type = 'repair'
                    ORDER BY Folders.Name").AsList();

                    List<Writeoff> _writeoffs = conn.Query<Writeoff>(@"SELECT
                        Writeoffs.Id
                        ,Writeoffs.Name
                        ,Writeoffs.Date
                        ,TypesWriteoffs.Name AS [Type]
                        ,Folders.Id          AS [FolderId]
                    FROM Writeoffs
                    LEFT OUTER JOIN TypesWriteoffs ON Writeoffs.Type = TypesWriteoffs.Id
                    LEFT OUTER JOIN Folders ON Writeoffs.FolderId = Folders.Id
                    ORDER BY Writeoffs.Date DESC").AsList();

                    bool found;

                    foreach (Repair repair in _repairs)
                    {
                        found = false;
                        foreach (Writeoff writeoff in _writeoffs)
                        {
                            if (repair.WriteoffId == writeoff.Id)
                            {
                                writeoff.Repairs.Add(repair);
                                found = true;
                                break;
                            }
                        }

                        if (!found)
                        {
                            foreach (Folder folder in _folders)
                            {
                                if (repair.FolderId == folder.Id)
                                {
                                    folder.Repairs.Add(repair);
                                    found = true;
                                    break;
                                }
                            }

                            if (!found)
                            {
                                Repairs.Add(repair);
                            }
                        }
                    }

                    foreach (Writeoff writeoff in _writeoffs)
                    {
                        found = false;
                        foreach (Folder folder in _folders)
                        {
                            if (writeoff.FolderId == folder.Id)
                            {
                                folder.Writeoffs.Add(writeoff);
                                found = true;
                                break;
                            }
                        }

                        if (!found)
                        {
                            Writeoffs.Add(writeoff);
                        }
                    }

                    foreach (Folder folder in _folders)
                    {
                        if (folder.FolderId == 0)
                        {
                            Folders.Add(Folder.Build(folder, _folders));
                        }
                    }
                }
            }
        }
    }
}