using Devin.Models;
using System.Linq;
using System.Web.Mvc;

namespace Devin.Controllers
{
    public class HardwareController : Controller
    {
        public ActionResult Index() => View("Index");

        public ActionResult Cart(string Id) => View("Index", model: Id);
    }
}