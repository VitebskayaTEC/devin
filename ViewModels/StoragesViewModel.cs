using Devin.Models;
using LinqToDB;
using System.Collections.Generic;
using System.Linq;

namespace Devin.ViewModels
{
    public class StoragesViewModel
    {
        public List<Folder> Folders { get; set; } = new List<Folder>();

        public List<Storage> Storages { get; set; } = new List<Storage>();

        public StoragesViewModel(string Search = "")
        {
            using (var db = new DbDevin())
            {
                if (!string.IsNullOrEmpty(Search))
                {
                    Storages = db.Storages
                        .Where(x => !x.IsDeleted && (
                            x.Inventory.Contains(Search)
                            || x.Name.Contains(Search)
                            || x.Account.Contains(Search)
                        ))
                        .OrderBy(x => x.Inventory)
                        .ToList();
                }
                else
                {
                    var foldersQuery = from f in db.Folders
                                       from p in db.Folders.Where(x => x.Id == f.FolderId).DefaultIfEmpty(new Folder { Id = 0 })
                                       where f.Type == "storage"
                                       orderby f.Name
                                       select new Folder
                                       {
                                           Id = f.Id,
                                           Name = f.Name,
                                           FolderId = p.Id,
                                       };

                    var _folders = foldersQuery.ToList();

                    var storages = db.Storages
                        .Where(x => !x.IsDeleted)
                        .OrderBy(x => x.Inventory)
                        .ToList(); ;

                    bool found = false;

                    foreach (var storage in storages)
                    {
                        found = false;
                        foreach (var folder in _folders)
                        {
                            if (storage.FolderId == folder.Id)
                            {
                                folder.Storages.Add(storage);
                                found = true;
                                break;
                            }
                        }

                        if (!found) Storages.Add(storage);
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