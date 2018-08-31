using System.Web.Mvc;

namespace Devin.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index() => View();
    }
}