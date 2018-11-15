using Dapper;
using Devin.Models;
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
            using (var conn = Database.Connection())
            {
                List<Object1C> objects = conn.Query<Object1C>("SELECT * FROM Objects1C ORDER BY Account, Guild, Inventory").AsList();

                if (!string.IsNullOrEmpty(Search))
                {
                    SearchResults = conn.Query<Object1C>($@"SELECT *
                    FROM Objects1C
                    WHERE Inventory   LIKE '%{Search}%'
                       OR Description LIKE '%{Search}%'
                       OR Guild       LIKE '%{Search}%'
                       OR SubDivision LIKE '%{Search}%'
                       OR Mol         LIKE '%{Search}%'
                       OR Account     LIKE '%{Search}%'
                       OR Location    LIKE '%{Search}%'
                    ORDER BY Inventory").AsList();
                }
                else
                {
                    Hided = new Folder { Id = 0, Name = "Скрытые объекты" };
                    Materials = new Folder { Id = 1, Name = "Материалы" };
                    OS = new Folder { Id = 2, Name = "Основные средства" };
                    
                    Materials.SubFolders = conn.Query<Folder>("SELECT (100 + ROW_NUMBER() OVER(ORDER BY Account ASC)) AS Id, Account AS Name FROM Objects1C WHERE Account IS NOT NULL AND IsHide = 0 GROUP BY Account ORDER BY Account").AsList();
                    OS.SubFolders = conn.Query<Folder>("SELECT (200 + ROW_NUMBER() OVER(ORDER BY Guild ASC)) AS Id, Guild AS Name FROM Objects1C WHERE Account IS NULL AND IsHide = 0 GROUP BY Guild ORDER BY Guild").AsList();

                    foreach (var folder in Materials.SubFolders)
                    {
                        folder.Objects = objects.Where(x => x.Account == folder.Name).AsList();
                    }

                    foreach (var folder in OS.SubFolders)
                    {
                        folder.Objects = objects.Where(x => string.IsNullOrEmpty(x.Account) && (x.Guild == folder.Name)).AsList();
                    }

                    Hided.Objects = objects.Where(x => x.IsHide).AsList();
                }
            }
        }
    }
}