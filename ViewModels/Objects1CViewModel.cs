using Devin.Models;
using LinqToDB;
using System.Collections.Generic;
using System.Linq;

namespace Devin.ViewModels
{
    public class Objects1CViewModel
    {
        public Folder OS { get; set; }

        public Folder Materials { get; set; }

        public Folder Hided { get; set; }

        public List<Object1C> SearchResults { get; set; } = new List<Object1C>();

        public Objects1CViewModel(string Search = "")
        {
            using (var db = new DevinContext())
            {
                if (!string.IsNullOrEmpty(Search))
                {
                    SearchResults = db.Objects1C
                        .Where(x => x.Inventory.Contains(Search)
                            || x.Description.Contains(Search)
                            || x.Guild.Contains(Search)
                            || x.SubDivision.Contains(Search)
                            || x.Mol.Contains(Search)
                            || x.Account.Contains(Search)
                            || x.Location.Contains(Search)
                        )
                        .OrderBy(x => x.Inventory)
                        .ToList();
                }
                else
                {
                    var objects = db.Objects1C
                        .OrderBy(x => x.Account)
                        .ThenBy(x => x.Guild)
                        .ThenBy(x => x.Inventory)
                        .ToList();

                    Hided = new Folder { Id = 0, Name = "Скрытые объекты" };
                    Materials = new Folder { Id = 1, Name = "Материалы" };
                    OS = new Folder { Id = 2, Name = "Основные средства" };

                    var groups = db.Objects1C
                        .Where(x => x.Account != null && !x.IsHide)
                        .OrderBy(x => x.Account)
                        .GroupBy(x => x.Account)
                        .Select(x => x.Key)
                        .ToArray();

                    Materials.SubFolders = new List<Folder>();

                    for (int i = 0; i < groups.Length; i++)
                    {
                        Materials.SubFolders.Add(new Folder { Id = 100 + i, Name = groups[i] });
                    }

                    groups = db.Objects1C
                        .Where(x => x.Account == null && !x.IsHide)
                        .OrderBy(x => x.Guild)
                        .GroupBy(x => x.Guild)
                        .Select(x => x.Key)
                        .ToArray();

                    OS.SubFolders = new List<Folder>();

                    for (int i = 0; i < groups.Length; i++)
                    {
                        OS.SubFolders.Add(new Folder { Id = 200 + i, Name = groups[i] });
                    }

                    foreach (var folder in Materials.SubFolders)
                    {
                        folder.Objects = objects.Where(x => x.Account == folder.Name).ToList();
                    }

                    foreach (var folder in OS.SubFolders)
                    {
                        folder.Objects = objects.Where(x => string.IsNullOrEmpty(x.Account) && (x.Guild == folder.Name)).ToList();
                    }

                    Hided.Objects = objects.Where(x => x.IsHide).ToList();
                }
            }
        }
    }
}