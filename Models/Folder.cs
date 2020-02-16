using LinqToDB.Mapping;
using System.Collections.Generic;

namespace Devin.Models
{
    [Table(Name = "Folders")]
    public class Folder
    {
        [Column, PrimaryKey, Identity, NotNull]
        public int Id { get; set; }

        [Column, NotNull]
        public int FolderId { get; set; }

        [Column]
        public string Name { get; set; }

        [Column, NotNull]
        public string Type { get; set; }

        public List<Folder> SubFolders { get; set; } = new List<Folder>();

        public List<Device> Devices { get; set; } = new List<Device>();

        public List<Computer> Computers { get; set; } = new List<Computer>();

        public List<Storage> Storages { get; set; } = new List<Storage>();

        public List<Writeoff> Writeoffs { get; set; } = new List<Writeoff>();

        public List<Repair> Repairs { get; set; } = new List<Repair>();

        public List<Object1C> Objects { get; set; } = new List<Object1C>();

        public List<Report> Reports { get; set; } = new List<Report>();

        public static Folder Build(Folder folder, List<Folder> folders)
        {
            foreach (var f in folders)
            {
                if (f.FolderId == folder.Id)
                {
                    folder.SubFolders.Add(Build(f, folders));
                }
            }

            return folder;
        }

        public static Folder FindSubFolder(List<Folder> folders, int id)
        {
            foreach (Folder sub in folders)
            {
                if (sub.Id == id) return sub;
                var found = FindSubFolder(sub.SubFolders, id);
                if (found != null) return found;
            }
            return null;
        }
    }
}