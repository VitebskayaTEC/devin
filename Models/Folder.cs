﻿using System.Collections.Generic;

namespace Devin.Models
{
    public class Folder
    {
        public int Id { get; set; }

        public int FolderId { get; set; }

        public string Title { get; set; }

        public List<Folder> SubFolders { get; set; } = new List<Folder>();

        public List<Device> Devices { get; set; } = new List<Device>();

        public List<Computer> Computers { get; set; } = new List<Computer>();

        public List<Storage> Storages { get; set; } = new List<Storage>();

        public List<Writeoff> Writeoffs { get; set; } = new List<Writeoff>();

        public List<Repair> Repairs { get; set; } = new List<Repair>();

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
    }
}