using System.Web.Mvc;

namespace Devin.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index() => RedirectToAction(null, "devices");

        public ActionResult Cart(string Id)
        {
            if (Id.Contains("device")) return RedirectToAction("Cart", "Devices", new { Id = int.Parse(Id.Replace("device", "")) });
            if (Id.Contains("storage")) return RedirectToAction("Cart", "Storages", new { Id = int.Parse(Id.Replace("storage", "")) });
            if (Id.Contains("repair")) return RedirectToAction("Cart", "Repairs", new { Id = int.Parse(Id.Replace("repair", "")) });
            if (Id.Contains("off")) return RedirectToAction("Cart", "Writeoffs", new { Id = int.Parse(Id.Replace("off", "")) });
            if (Id.Contains("prn")) return RedirectToAction("Cart", "Catalog", new { Id });
            if (Id.Contains("cart")) return RedirectToAction("Cart", "Catalog", new { Id });
            return HttpNotFound();
        }
    }
}