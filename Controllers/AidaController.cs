using Dapper;
using Devin.Models;
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
            using (var conn = Database.Connection())
            {
                int id = int.TryParse((Id ?? "").Replace("aida", ""), out int i) ? i : 0;
                if (id == 0) return Json(new { Warning = "Идентификатор не был передан" });

                string name = conn.Query<string>("SELECT RHost FROM Report WHERE ID = @Id", new { Id = id }).FirstOrDefault() ?? "";

                conn.Execute("DELETE FROM Report WHERE ID = @Id", new { Id = id });
                conn.Execute("DELETE FROM Item WHERE ReportID = @Id", new { Id = id });
                conn.Log(User, "aida", id, "Отчет по компьютеру \"" + name + "\" удален из базы данных");

                return Json(new { Good = "Отчет о компьютере удален из базы" });
            }
        }

        public JsonResult DeleteOldRecords()
        {
            try
            {
                using (var conn = Database.Connection())
                {
                    var oldRecordsCount = conn.QueryFirst<int>("SELECT Count(ID) FROM Report WHERE ID NOT IN (SELECT Max(ID) FROM Report GROUP BY RHost)");

                    if (oldRecordsCount == 0)
                    {
                        return Json(new { Good = "База не содержит устаревших отчетов" });
                    }

                    int oldRowsCount = conn.QueryFirst<int>("SELECT Count(*) FROM Item WHERE ReportID IN (SELECT ID FROM Report WHERE ID NOT IN (SELECT Max(ID) FROM Report GROUP BY RHost))");

                    conn.Execute("DELETE FROM Item WHERE ReportID IN (SELECT ID FROM Report WHERE ID NOT IN (SELECT Max(ID) FROM Report GROUP BY RHost))");
                    conn.Execute("DELETE FROM Report WHERE ID NOT IN (SELECT Max(ID) FROM Report GROUP BY RHost)");

                    return Json(new { Good = "База обновлена. Удалено устаревших:<br />отчетов => " + oldRecordsCount + "<br />строк данных => " + oldRowsCount });
                }
            }
            catch (Exception e)
            {
                return Json(new { Error = "Ошибка при выполнении кода на сервере<br />" + e.Message });
            }
        }
    }
}