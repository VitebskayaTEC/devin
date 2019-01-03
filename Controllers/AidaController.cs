using Dapper;
using Devin.Models;
using System.Linq;
using System.Web.Mvc;

namespace Devin.Controllers
{
    public class AidaController : Controller
    {
        public ActionResult Index() => View();

        public ActionResult Load() => View("List");

        public ActionResult Cart(int Id) => View(model: Id);

        public ActionResult Data(string Id) => View(model: Id);

        public ActionResult Autorun(string Id) => View(model: Id);

        public ActionResult Devices(string Id) => View(model: Id);

        public ActionResult Programs(string Id) => View(model: Id);

        public JsonResult Delete(int Id)
        {
            using (var conn = Database.Connection())
            {
                string name = conn.Query<string>("SELECT RHost FROM Report WHERE ID = @Id", new { Id }).FirstOrDefault() ?? "";

                conn.Execute("DELETE FROM Report WHERE ID = @Id", new { Id });
                conn.Execute("DELETE FROM Item WHERE ReportID = @Id", new { Id });
                conn.Log(User, "aida", Id, "Отчет по компьютеру \"" + name + "\" удален из базы данных");

                return Json(new { Good = "Отчет о компьютере удален из базы" });
            }
        }
    }
}