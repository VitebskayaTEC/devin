using Dapper;
using Devin.Models;
using System.Web.Mvc;

namespace Devin.Controllers
{
    public class FoldersController : Controller
    {
        public JsonResult Create(string Type, string Name)
        {
            using (var conn = Database.Connection())
            {
                Type = Type.Substring(0, Type.Length - 1);
                conn.Execute("INSERT INTO Folders (Type, Name, FolderId) VALUES (@Type, @Name, 0)", new { Type, Name });
                return Json(new { Good = "Создана новая папка \"" + Name + "\"" });
            }
        }

        public JsonResult CreateInner(string Type, string Name, int FolderId)
        {
            using (var conn = Database.Connection())
            {
                Type = Type.Substring(0, Type.Length - 1);
                conn.Execute("INSERT INTO Folders (Type, Name, FolderId) VALUES (@Type, @Name, @FolderId)", new { Type, Name, FolderId });
                return Json(new { Good = "Создана новая папка \"" + Name + "\"" });
            }
        }

        public JsonResult Delete(string Type, int Id)
        {
            using (var conn = Database.Connection())
            {
                string name = conn.QueryFirst<string>("SELECT Name FROM Folders WHERE Id = @Id", new { Id });
                switch (Type)
                {
                    case "devices": conn.Execute("UPDATE Devices SET FolderId = 0 WHERE FolderId = @Id", new { Id }); break;
                    case "storages": conn.Execute("UPDATE Storages SET FolderId = 0 WHERE FolderId = @Id", new {Id }); break;
                    case "repairs": conn.Execute("UPDATE Writeoffs SET FolderId = 0 WHERE FolderId = @Id", new { Id }); break;
                }
                conn.Execute("DELETE FROM Folders WHERE Id = @Id", new { Id });
                return Json(new { Good = "Папка \"" + name + "\" удалена" });
            }
        }

        public JsonResult Update(string Name, int Id)
        {
            using (var conn = Database.Connection())
            {
                string old = conn.QueryFirst<string>("SELECT Name FROM Folders WHERE Id = @Id", new { Id });
                conn.Execute("UPDATE Folders SET Name = @Name WHERE Id = @Id", new { Name, Id });
                return Json(new { Good = "Папка \"" + old + "\" переименована в \"" + Name + "\"" });
            }
        }

        public JsonResult Clear(string Type, int Id)
        {
            using (var conn = Database.Connection())
            {
                string name = conn.QueryFirst<string>("SELECT Name FROM Folders WHERE Id = @Id", new { Id });
                switch (Type)
                {
                    case "devices": conn.Execute("UPDATE Devices SET FolderId = 0 WHERE FolderId = @Id", new { Id }); break;
                    case "storages": conn.Execute("UPDATE Storages SET FolderId = 0 WHERE FolderId = @Id", new { Id }); break;
                    case "repairs": conn.Execute("UPDATE Writeoffs SET FolderId = 0 WHERE FolderId = @Id", new { Id }); break;
                }
                return Json(new { Good = "Из папки \"" + name + "\" вынесены все вложенные элементы" });
            }
        }

        public JsonResult Move(int FolderId, int Id)
        {
            using (var conn = Database.Connection())
            {
                string name = conn.QueryFirst<string>("SELECT Name FROM Folders WHERE Id = @Id", new { Id });
                conn.Execute("UPDATE Folders SET FolderId = @FolderId WHERE Id = @Id", new { Id, FolderId });
                
                if (FolderId == 0)
                {
                    return Json(new { Good = "Папка \"" + name + "\" размещена отдельно" });
                }
                else
                {
                    string parentName = conn.QueryFirst<string>("SELECT Name FROM Folders WHERE Id = @FolderId", new { FolderId });
                    return Json(new { Good = "Папка \"" + name + "\" перенесена в папку \"" + parentName + "\"" });
                }
            }
        }
    }
}