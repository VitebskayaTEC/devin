using Devin.Models;
using LinqToDB;
using System.Collections.Generic;
using System.Linq;

namespace Devin.ViewModels
{
    public class RepairsViewModel
    {
        public List<Folder> Folders { get; set; } = new List<Folder>();

        public List<Writeoff> Writeoffs { get; set; } = new List<Writeoff>();

        public List<Repair> Repairs { get; set; } = new List<Repair>();

        public RepairsViewModel(string Search = "")
        {
            using (var db = new DevinContext())
            {
                if (!string.IsNullOrEmpty(Search))
                {
                    var query = from r in db.Repairs
                                from d in db.Devices.Where(x => x.Id == r.DeviceId).DefaultIfEmpty()
                                from s in db.Storages.Where(x => x.Id == r.StorageId).DefaultIfEmpty()
                                where d.Name.Contains(Search)
                                    || d.Description.Contains(Search)
                                    || d.Inventory.Contains(Search)
                                    || s.Name.Contains(Search)
                                    || s.Inventory.Contains(Search)
                                orderby r.FolderId ascending, r.WriteoffId ascending, r.Date descending
                                select new Repair
                                {
                                    Id = r.Id,
                                    FolderId = r.FolderId,
                                    WriteoffId = r.WriteoffId,
                                    Date = r.Date,
                                    Author = r.Author,
                                    DeviceId = r.DeviceId,
                                    StorageId = r.StorageId,
                                    Number = r.Number,
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
                else
                {
                    var repairsQuery = from r in db.Repairs
                                       from d in db.Devices.Where(x => x.Id == r.DeviceId).DefaultIfEmpty()
                                       from s in db.Storages.Where(x => x.Id == r.StorageId).DefaultIfEmpty()
                                       orderby r.FolderId ascending, r.WriteoffId ascending, r.Date descending
                                       select new Repair
                                       {
                                           Id = r.Id,
                                           FolderId = r.FolderId,
                                           WriteoffId = r.WriteoffId,
                                           Date = r.Date,
                                           Author = r.Author,
                                           DeviceId = r.DeviceId,
                                           StorageId = r.StorageId,
                                           Number = r.Number,
                                           IsOff = r.IsOff,
                                           IsVirtual = r.IsVirtual,
                                           Device = new Device
                                           {
                                               Id = d.Id,
                                               Inventory = d.Inventory,
                                               Name = d.Name,
                                               PublicName = d.PublicName,
                                               Type = d.Type,
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
                                               Cost = s.Cost,
                                           },
                                       };

                    var _repairs = repairsQuery.ToList();

                    var foldersQuery = from f in db.Folders
                                       from p in db.Folders.Where(x => x.Id == f.FolderId).DefaultIfEmpty(new Folder { Id = 0 })
                                       where f.Type == "repair"
                                       orderby f.Name
                                       select new Folder
                                       {
                                           Id = f.Id,
                                           Name = f.Name,
                                           FolderId = p.Id,
                                       };

                    var _folders = foldersQuery.ToList();

                    var writeoffsQuery = from w in db.Writeoffs
                                         from t in db._WriteoffTypes.Where(x => x.Id == w.Type).DefaultIfEmpty()
                                         from f in db.Folders.Where(x => x.Id == w.FolderId).DefaultIfEmpty()
                                         orderby w.Date descending
                                         select new Writeoff
                                         {
                                             Id = w.Id,
                                             Name = w.Name,
                                             Date = w.Date,
                                             Type = t.Name,
                                             FolderId = f.Id,
                                             Mark = w.Mark,
                                         };

                    var _writeoffs = writeoffsQuery.ToList();

                    bool found;

                    foreach (var repair in _repairs)
                    {
                        found = false;
                        foreach (var writeoff in _writeoffs)
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
                            foreach (var folder in _folders)
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

                    foreach (var writeoff in _writeoffs)
                    {
                        found = false;
                        foreach (var folder in _folders)
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

                    foreach (var folder in _folders)
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