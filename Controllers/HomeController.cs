using System.Web.Mvc;

namespace Devin.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index() => RedirectToAction(null, "devices");

        public ActionResult Cart(string Id)
        {
            if (Id.Contains("device"))
            {
                if (!int.TryParse(Id.Replace("device", ""), out int id)) return Content("Id не является числовым");
                return RedirectToAction("Cart", "Devices", new { Id = id });
            }

            if (Id.Contains("aida"))
            {
                if (!int.TryParse(Id.Replace("aida", ""), out int id)) return Content("Id не является числовым");
                return RedirectToAction("Cart", "Aida", new { Id = id });
            }

            if (Id.Contains("storage"))
            {
                if (!int.TryParse(Id.Replace("storage", ""), out int id)) return Content("Id не является числовым");
                return RedirectToAction("Cart", "Storages", new { Id = id });
            }

            if (Id.Contains("repair"))
            {
                if (!int.TryParse(Id.Replace("repair", ""), out int id)) return Content("Id не является числовым");
                return RedirectToAction("Cart", "Repairs", new { Id = id });
            }

            if (Id.Contains("off"))
            {
                if (!int.TryParse(Id.Replace("off", ""), out int id)) return Content("Id не является числовым");
                return RedirectToAction("Cart", "Writeoffs", new { Id = id });
            }
            
            if (Id.Contains("prn") || Id.Contains("cart"))
            {
                return RedirectToAction("Cart", "Catalog", new { Id });
            }

            return Content("");
        }

        public ActionResult Card(int Id, string type)
        { 
            switch (type)
            {
                case "aida": return RedirectToAction("card", "aida", new { Id });
                default: return HttpNotFound();
            }
        }
    }
}