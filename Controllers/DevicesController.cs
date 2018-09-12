using Dapper;
using Devin.Models;
using System.Web.Mvc;

namespace Devin.Controllers
{
    public class DevicesController : Controller
    {
        public ActionResult DefectAct() => View();

        public ActionResult Table() => View();

        public ActionResult Table1C() => View();

        public void HideDevice1C(int Id, bool Hide)
        {
            using (var conn = Database.Connection())
            {
                conn.Execute("UPDATE Devices1C SET IsHide = @Hide WHERE Inventory = @Id", new { Id, Hide });
            }
        }
    }
}