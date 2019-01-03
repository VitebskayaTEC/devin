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
                int Id = conn.QueryFirst<int>("SELECT Max(Id) FROM Folders");

                conn.Log(User, "folders", Id, "Создана папка \"" + Name + "\" для страницы \"" + Type + "\"");

                return Json(new { Good = "Создана новая папка \"" + Name + "\"" });
            }
        }

        public JsonResult CreateInner(string Type, string Name, int FolderId)
        {
            using (var conn = Database.Connection())
            {
                Type = Type.Substring(0, Type.Length - 1);
                conn.Execute("INSERT INTO Folders (Type, Name, FolderId) VALUES (@Type, @Name, @FolderId)", new { Type, Name, FolderId });
                int Id = conn.QueryFirst<int>("SELECT Max(Id) FROM Folders");

                conn.Log(User, "folders", Id, "Создана папка \"" + Name + "\" для страницы \"" + Type + "\"");

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
                conn.Log(User, "folders", Id, "Удалена папка \"" + name + "\"");

                return Json(new { Good = "Папка \"" + name + "\" удалена" });
            }
        }

        public JsonResult Update(string Name, int Id)
        {
            using (var conn = Database.Connection())
            {
                string old = conn.QueryFirst<string>("SELECT Name FROM Folders WHERE Id = @Id", new { Id });
                conn.Execute("UPDATE Folders SET Name = @Name WHERE Id = @Id", new { Name, Id });
                conn.Log(User, "folders", Id, "Папка \"" + old + "\" переименована в \"" + Name + "\"");

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

                conn.Log(User, "folders", Id, "Из папки \"" + name + "\" вынесены все вложенные элементы");
                return Json(new { Good = "Из папки \"" + name + "\" вынесены все вложенные элементы" });
            }
        }

        public JsonResult Move(int FolderId, int Id)
        {
            using (var conn = Database.Connection())
            {
                string name = conn.QueryFirst<string>("SELECT Name FROM Folders WHERE Id = @Id", new { Id });
                conn.Execute("UPDATE Folders SET FolderId = @FolderId WHERE Id = @Id", new { Id, FolderId });

                string text = "";
                if (FolderId == 0)
                {
                    text = "Папка \"" + name + "\" размещена отдельно";
                }
                else
                {
                    string parentName = conn.QueryFirst<string>("SELECT Name FROM Folders WHERE Id = @FolderId", new { FolderId });
                    text = "Папка \"" + name + "\" перенесена в папку \"" + parentName + "\"";
                }

                conn.Log(User, "folders", Id, text);
                return Json(new { Good = text });
            }
        }
    }
}