using Dapper;
using Devin.Models;
using System.Collections.Generic;

namespace Devin.ViewModels
{
    public class StoragesViewModel
    {
        public List<Folder> Folders { get; set; } = new List<Folder>();

        public List<Storage> Storages { get; set; } = new List<Storage>();

        public StoragesViewModel(string Search = "")
        {
            using (var conn = Database.Connection())
            {
                if (!string.IsNullOrEmpty(Search))
                {
                    Storages = conn.Query<Storage>($@"SELECT * FROM Storages WHERE IsDeleted <> 1 AND (
                        Inventory LIKE '%{Search}%'
                        OR Name LIKE '%{Search}%'
                        OR Date LIKE '%{Search}%'
                        OR Account LIKE '%{Search}%'
                    ) ORDER BY Inventory").AsList();
                }
                else
                {
                    List<Folder> _folders = conn.Query<Folder>(@"SELECT 
	                    Folders.Id,
	                    CASE WHEN Parents.Id IS NULL THEN 0 ELSE Parents.Id END AS FolderId,
	                    Folders.Name
                    FROM Folders
	                LEFT OUTER JOIN Folders AS Parents ON Folders.FolderId = Parents.Id
                    WHERE Folders.Type = 'storage'
                    ORDER BY Folders.Name").AsList();

                    List<Storage> storages = conn.Query<Storage>("SELECT * FROM Storages WHERE IsDeleted <> 1 ORDER BY Date DESC").AsList();

                    bool found = false;

                    foreach (Storage storage in storages)
                    {
                        found = false;
                        foreach (Folder folder in _folders)
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