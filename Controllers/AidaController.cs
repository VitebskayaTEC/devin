using System.Web.Mvc;

namespace Devin.Controllers
{
    public class AidaController : Controller
    {
        public ActionResult Index() => View();

        public ActionResult List() => View();
    }
}