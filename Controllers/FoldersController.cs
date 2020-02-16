using Devin.Models;
using LinqToDB;
using System.Linq;
using System.Web.Mvc;

namespace Devin.Controllers
{
    public class FoldersController : Controller
    {
        public JsonResult Create(string type, string name)
        {
            using (var db = new DevinContext())
            {
                if (type != "aida") type = type.Substring(0, type.Length - 1);

                var id = db.Folders
                    .Value(x => x.Type, type)
                    .Value(x => x.Name, name)
                    .Value(x => x.FolderId, 0)
                    .InsertWithInt32Identity();

                if (!id.HasValue) return Json(new { Error = "Ошибка при создании записи в БД" });

                db.Log(User, "folders", id, "Создана папка \"" + name + "\" для страницы \"" + type + "\"");
                return Json(new { Good = "Создана новая папка \"" + name + "\"" });
            }
        }

        public JsonResult CreateInner(string type, string Name, int FolderId)
        {
            using (var db = new DevinContext())
            {
                if (type != "aida") type = type.Substring(0, type.Length - 1);

                int Id = db.InsertWithInt32Identity(new Folder { Type = type, Name = Name, FolderId = FolderId });
                db.Log(User, "folders", Id, "Создана папка \"" + Name + "\" для страницы \"" + type + "\"");

                return Json(new { Good = "Создана новая папка \"" + Name + "\"" });
            }
        }

        public JsonResult Delete(string Type, int Id)
        {
            using (var db = new DevinContext())
            {
                string name = db.Folders.Where(x => x.Id == Id).Select(x => x.Name).FirstOrDefault();
                switch (Type)
                {
                    case "devices": db.Devices.Where(x => x.FolderId == Id).Set(x => x.FolderId, 0).Update(); break;
                    case "storages": db.Storages.Where(x => x.FolderId == Id).Set(x => x.FolderId, 0).Update(); break;
                    case "repairs": db.Writeoffs.Where(x => x.FolderId == Id).Set(x => x.FolderId, 0).Update(); break;
                    case "aida": db.Report.Where(x => x.PlaceId == Id).Set(x => x.PlaceId, 0).Update(); break;
                }

                db.Folders.Where(x => x.Id == Id).Delete();
                db.Log(User, "folders", Id, "Удалена папка \"" + name + "\"");

                return Json(new { Good = "Папка \"" + name + "\" удалена" });
            }
        }

        public JsonResult Update(string Name, int Id)
        {
            using (var db = new DevinContext())
            {
                string oldName = db.Folders
                    .Where(x => x.Id == Id)
                    .Select(x => x.Name)
                    .FirstOrDefault();

                db.Folders
                    .Where(x => x.Id == Id)
                    .Set(x => x.Name, Name)
                    .Update();

                db.Log(User, "folders", Id, "Папка \"" + oldName + "\" переименована в \"" + Name + "\"");

                return Json(new { Good = "Папка \"" + oldName + "\" переименована в \"" + Name + "\"" });
            }
        }

        public JsonResult Clear(string Type, int Id)
        {
            using (var db = new DevinContext())
            {
                string name = db.Folders.Where(x => x.Id == Id).Select(x => x.Name).FirstOrDefault();
                switch (Type)
                {
                    case "devices": db.Devices.Where(x => x.FolderId == Id).Set(x => x.FolderId, 0).Update(); break;
                    case "storages": db.Storages.Where(x => x.FolderId == Id).Set(x => x.FolderId, 0).Update(); break;
                    case "repairs": db.Writeoffs.Where(x => x.FolderId == Id).Set(x => x.FolderId, 0).Update(); break;
                    case "aida": db.Report.Where(x => x.PlaceId == Id).Set(x => x.PlaceId, 0).Update(); break;
                }

                db.Log(User, "folders", Id, "Из папки \"" + name + "\" вынесены все вложенные элементы");

                return Json(new { Good = "Из папки \"" + name + "\" вынесены все вложенные элементы" });
            }
        }

        public JsonResult Move(int FolderId, int Id)
        {
            using (var db = new DevinContext())
            {
                string name = db.Folders.Where(x => x.Id == Id).Select(x => x.Name).FirstOrDefault();
                db.Folders.Where(x => x.Id == Id).Set(x => x.FolderId, FolderId).Update();

                string text = "";
                if (FolderId == 0)
                {
                    text = "Папка \"" + name + "\" размещена отдельно";
                }
                else
                {
                    string parentName = db.Folders.Where(x => x.Id == FolderId).Select(x => x.Name).FirstOrDefault();
                    text = "Папка \"" + name + "\" перенесена в папку \"" + parentName + "\"";
                }

                db.Log(User, "folders", Id, text);

                return Json(new { Good = text });
            }
        }
    }
}