using Dapper;
using Devin.Models;
using System.Collections.Generic;
using System.Linq;
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

                conn.Log(User, "cartridges", Id, "Создан новый картридж");

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

                Cartridge old = conn.Query<Cartridge>("SELECT * FROM Cartridge WHERE Id = @Id", cartridge).FirstOrDefault() ?? new Cartridge();
                List<string> changes = new List<string>();

                if (old.Name != cartridge.Name) changes.Add("наименование [\"" + old.Name + "\" => \"" + cartridge.Name + "\"]");
                if (old.Type != cartridge.Type) changes.Add("тип [\"" + old.Type + "\" => \"" + cartridge.Type + "\"]");
                if (old.Color != cartridge.Color) changes.Add("цвет [\"" + old.Color + "\" => \"" + cartridge.Color + "\"]");
                if (old.Description != cartridge.Description) changes.Add("описание [\"" + old.Description + "\" => \"" + cartridge.Description + "\"]");
                if (old.Cost != cartridge.Cost) changes.Add("стоимость [\"" + old.Cost + "\" => \"" + cartridge.Cost + "\"]");

                if (changes.Count > 0)
                {
                    conn.Execute("UPDATE Cartridges SET Name = @Name, Cost = @Cost, Type = @Type, Color = @Color, Description = @Description WHERE Id = @Id", cartridge);
                    conn.Log(User, "cartridges", cartridge.Id, "Картридж изменен. Изменения: " + changes.ToLog());

                    return Json(new { Good = "Картридж изменен. Изменения:<br />" + changes.ToHtml() });
                }
                else
                {
                    return Json(new { Warning = "Изменений не было" });
                }
            }
        }

        public JsonResult DeleteCartridge(int Id)
        {
            using (var conn = Database.Connection())
            {
                conn.Execute("DELETE FROM Cartridges WHERE Id = @Id", new { Id });
                conn.Log(User, "cartridges", Id, "Картридж удален");

                return Json(new { Good = "Картридж удален" });
            }
        }


        public JsonResult CreatePrinter()
        {
            using (var conn = Database.Connection())
            {
                conn.Execute("INSERT INTO Printers (Name) VALUES ('Новый типовой принтер')");
                int Id = conn.QueryFirst<int>("SELECT Max(Id) FROM Printers");

                conn.Log(User, "printers", Id, "Создан новый принтер");

                return Json(new { Good = "Создан новый принтер", Id = "prn" + Id });
            }
        }

        public JsonResult UpdatePrinter([Bind(Include = "Id,Name,Description")] Printer printer)
        {
            using (var conn = Database.Connection())
            {
                Printer old = conn.Query<Printer>("SELECT * FROM Printers WHERE Id = @Id", printer).FirstOrDefault() ?? new Printer();
                List<string> changes = new List<string>();

                if (old.Name != printer.Name) changes.Add("наименование [\"" + old.Name + "\" => \"" + printer.Name + "\"]");
                if (old.Description != printer.Description) changes.Add("описание [\"" + old.Description + "\" => \"" + printer.Description + "\"]");

                if (changes.Count > 0)
                {
                    conn.Execute("UPDATE Printers SET Name = @Name, Description = @Description WHERE Id = @Id", printer);
                    conn.Log(User, "printers", printer.Id, "Картридж изменен. Изменения: " + changes.ToLog());

                    return Json(new { Good = "Картридж изменен. Изменения:<br />" + changes.ToHtml() });
                }
                else
                {
                    return Json(new { Warning = "Изменений не было" });
                }
                
            }
        }

        public JsonResult DeletePrinter(int Id)
        {
            using (var conn = Database.Connection())
            {
                conn.Execute("DELETE FROM Printers WHERE Id = @Id", new { Id });
                conn.Log(User, "printers", Id, "Принтер удален");

                return Json(new { Good = "Принтер удален" });
            }
        }
        

        public JsonResult CreateCompare(int CartridgeId, int PrinterId)
        {
            using (var conn = Database.Connection())
            {
                string cartName = conn.Query<string>("SELECT Name FROM Cartridges WHERE Id = @CartridgeId", new { CartridgeId }).FirstOrDefault() ?? "<не определен>";
                string prnName = conn.Query<string>("SELECT Name FROM Printers WHERE Id = @PrinterId", new { PrinterId }).FirstOrDefault() ?? "<не определен>";

                conn.Execute("INSERT INTO PrintersCartridges (PrinterId, CartridgeId) VALUES (@PrinterId, @CartridgeId)", new { PrinterId, CartridgeId });
                conn.Log(User, "printers", PrinterId, "Создана связь c картриджем \"" + cartName + "\" [cart" + CartridgeId + "]");
                conn.Log(User, "cartridges", CartridgeId, "Создана связь c принтером \"" + prnName + "\" [prn" + PrinterId + "]");

                return Json(new { Good = "Связь создана" });
            }
        }

        public JsonResult DeleteCompare(int CartridgeId, int PrinterId)
        {
            using (var conn = Database.Connection())
            {
                string cartName = conn.Query<string>("SELECT Name FROM Cartridges WHERE Id = @CartridgeId", new { CartridgeId }).FirstOrDefault() ?? "<не определен>";
                string prnName = conn.Query<string>("SELECT Name FROM Printers WHERE Id = @PrinterId", new { PrinterId }).FirstOrDefault() ?? "<не определен>";

                conn.Execute("DELETE FROM PrintersCartridges WHERE CartridgeId = @CartridgeId AND PrinterId = @PrinterId", new { PrinterId, CartridgeId });
                conn.Log(User, "printers", PrinterId, "Удалена связь c картриджем \"" + cartName + "\" [cart" + CartridgeId + "]");
                conn.Log(User, "cartridges", CartridgeId, "Удалена связь c принтером \"" + prnName + "\" [prn" + PrinterId + "]");

                return Json(new { Good = "Связь удалена" });
            }
        }
    }
}