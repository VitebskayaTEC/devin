using Dapper;
using Devin.Models;
using System.Linq;
using System.Web.Mvc;

namespace Devin.Controllers
{
    public class AidaController : Controller
    {
        public ActionResult Index() => View();

        public ActionResult List() => View();
        
        public ActionResult Cart(string Id)
        {
            if (int.TryParse(Id, out int i))
            {
                return View(model: i);
            }
            else
            {
                using (var conn = Database.Connection())
                {
                    int id = conn.Query<int>("SELECT Max(Id) FROM Record WHERE UPPER(RHost) = UPPER(@Id)", new { Id }).FirstOrDefault();
                    return View(model: id);
                }
            }
        }

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