using Dapper;
using Devin.Models;
using System.Web.Mvc;

namespace Devin.Controllers
{
    public class CatalogController : Controller
    {
        public ActionResult Index() => View();

        public ActionResult Cart(string Id)
        {
            if (Id.ToLower().Contains("prn"))
            {
                return View("PrinterCart", model: int.Parse(Id.Replace("prn", "")));
            }
            else
            {
                return View("CartridgeCart", model: int.Parse(Id.Replace("cart", "")));
            }
        }


        public JsonResult CreateCartridge()
        {
            using (var conn = Database.Connection())
            {
                conn.Execute("INSERT INTO Cartridges (Name, Type, Color, Cost) VALUES ('Новый типовой картридж', '', '', null)");
                int Id = conn.QueryFirst<int>("SELECT Max(Id) FROM Cartridges");
                return Json(new { Good = "Создан новый картридж", Id = "cart" + Id });
            }
        }

        public JsonResult UpdateCartridge([Bind(Include = "Id,Name,Type,Color,Description")] Cartridge cartridge)
        {
            using (var conn = Database.Connection())
            {
                string cost = Request.Form.Get("Cost");
                if (!string.IsNullOrEmpty(cost))
                {
                    if (!float.TryParse(cost, out float f)) return Json(new { Error = "Стоимость введена в невалидном формате" }); else cartridge.Cost = f;
                    if (cartridge.Cost < 0) return Json(new { Error = "Стоимость должна быть положительной" });
                }
                else
                {
                    cartridge.Cost = 0;
                }

                conn.Execute("UPDATE Cartridges SET Name = @Name, Cost = @Cost, Type = @Type, Color = @Color, Description = @Description WHERE Id = @Id", cartridge);
                return Json(new { Good = "Картридж сохранен" });
            }
        }

        public JsonResult DeleteCartridge(int Id)
        {
            using (var conn = Database.Connection())
            {
                conn.Execute("DELETE FROM Cartridges WHERE Id = @Id", new { Id });
                return Json(new { Good = "Картридж удален" });
            }
        }


        public JsonResult CreatePrinter()
        {
            using (var conn = Database.Connection())
            {
                conn.Execute("INSERT INTO Printers (Name) VALUES ('Новый типовой принтер')");
                int Id = conn.QueryFirst<int>("SELECT Max(Id) FROM Printers");
                return Json(new { Good = "Создан новый принтер", Id = "prn" + Id });
            }
        }

        public JsonResult UpdatePrinter([Bind(Include = "Id,Name,Description")] Printer printer)
        {
            using (var conn = Database.Connection())
            {
                conn.Execute("UPDATE Printers SET Name = @Name, Description = @Description WHERE Id = @Id", printer);
                return Json(new { Good = "Принтер сохранен" });
            }
        }

        public JsonResult DeletePrinter(int Id)
        {
            using (var conn = Database.Connection())
            {
                conn.Execute("DELETE FROM Printers WHERE Id = @Id", new { Id });
                return Json(new { Good = "Принтер удален" });
            }
        }
        

        public JsonResult CreateCompare(int CartridgeId, int PrinterId)
        {
            using (var conn = Database.Connection())
            {
                conn.Execute("INSERT INTO PrintersCartridges (PrinterId, CartridgeId) VALUES (@PrinterId, @CartridgeId)", new { PrinterId, CartridgeId });
                return Json(new { Good = "Связь создана" });
            }
        }

        public JsonResult DeleteCompare(int CartridgeId, int PrinterId)
        {
            using (var conn = Database.Connection())
            {
                conn.Execute("DELETE FROM PrintersCartridges WHERE CartridgeId = @CartridgeId AND PrinterId = @PrinterId", new { PrinterId, CartridgeId });
                return Json(new { Good = "Связь удалена" });
            }
        }
    }
}