using Dapper;
using Devin.Models;
using System.Web.Mvc;

namespace Devin.Controllers
{
    public class AidaController : Controller
    {
        public ActionResult Index() => View();

        public ActionResult List() => View();

        public ActionResult Cart(int Id) => View(model: Id);

        public void Delete(int Id)
        {
            using (var conn = Database.Connection())
            {
                conn.Execute("DELETE FROM Report WHERE ID = @Id", new { Id });
                conn.Execute("DELETE FROM Item WHERE ReportID = @Id", new { Id });
            }
        }
    }
}