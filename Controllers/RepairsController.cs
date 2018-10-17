using System.Web.Mvc;

namespace Devin.Controllers
{
    public class RepairsController : Controller
    {
        public ActionResult Index() => View();

        public ActionResult List() => View();

        public ActionResult YearReport() => View();

        public ActionResult CartridgesUsage() => View();
    }
}