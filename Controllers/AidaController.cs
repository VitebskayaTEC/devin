using Devin.Models;
using LinqToDB;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Devin.Controllers
{
    public class AidaController : Controller
    {
        public ActionResult Index() => View();

        public ActionResult Load(string Item, string Search)
        {
            if ((Item ?? "").Contains("aida"))
            {
                int Id = int.TryParse((Item ?? "").Replace("aida", ""), out int i) ? i : 0;
                return View("ComputerData", new Report { ID = Id });
            }
            else if (!string.IsNullOrEmpty(Search))
            {
                ViewBag.Search = Search;
                return View("Search");
            }
            else
            {
                return View("List");
            }
        }

        public ActionResult Cart(int Id) => View(model: Id);

        public ActionResult Data(string Id) => View(model: Id);

        public ActionResult Autorun(string Id) => View(model: Id);

        public ActionResult Devices(string Id) => View(model: Id);

        public ActionResult Programs(string Id) => View(model: Id);


        public JsonResult Delete(string Id)
        {
            using (var db = new DbDevin())
            {
                int id = int.TryParse((Id ?? "").Replace("aida", ""), out int i) ? i : 0;
                if (id == 0) return Json(new { Warning = "Идентификатор не был передан" });

                string name = db.Report.Where(x => x.ID == id).Select(x => x.RHost).FirstOrDefault() ?? "Наименование не определено";

                db.Report.Where(x => x.ID == id).Delete();
                db.Item.Where(x => x.ReportID == id).Delete();
                db.Log(User, "aida", id, "Отчет по компьютеру \"" + name + "\" удален из базы данных");

                return Json(new { Good = "Отчет о компьютере удален из базы" });
            }
        }

        public JsonResult DeleteOldRecords()
        {
            try
            {
                using (var db = new DbDevin())
                {
                    var maxIdentifiers = db.Report
                        .GroupBy(x => x.RHost)
                        .Select(x => x.Select(g => g.ID).Max())
                        .ToList();
                    
                    var oldIdentifiers = db.Report
                        .Where(x => !maxIdentifiers.Contains(x.ID))
                        .Select(x => x.ID)
                        .ToList();

                    if (oldIdentifiers.Count() == 0)
                    {
                        return Json(new { Good = "База не содержит устаревших отчетов" });
                    }

                    int recordsDeleted = db.Item.Where(x => oldIdentifiers.Contains(x.ReportID)).Delete();
                    int itemsDeleted = db.Report.Where(x => oldIdentifiers.Contains(x.ID)).Delete();

                    return Json(new { Good = "Удалено:<br />Отчеты: " + recordsDeleted + "<br />Строки данных: " + itemsDeleted });
                }
            }
            catch (Exception e)
            {
                return Json(new { Error = "Ошибка при выполнении кода на сервере<br />" + e.Message });
            }
        }
    }
}