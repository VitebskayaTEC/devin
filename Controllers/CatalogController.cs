using Devin.Models;
using LinqToDB;
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
            using (var db = new DevinContext())
            {
                int Id = db.InsertWithInt32Identity(new Cartridge
                {
                    Name = "Новый типовой картридж",
                    Type = "",
                    Color = "",
                    Cost = 0
                });

                db.Log(User, "cartridges", Id, "Создан новый картридж");

                return Json(new { Good = "Создан новый картридж", Id = "cart" + Id });
            }
        }

        public JsonResult UpdateCartridge([Bind(Include = "Id,Name,Type,Color,Description")] Cartridge cartridge, string Cost)
        {
            if (!string.IsNullOrEmpty(Cost))
            {
                if (!float.TryParse(Cost, out float f)) return Json(new { Error = "Стоимость введена в невалидном формате" }); else cartridge.Cost = f;
                if (cartridge.Cost < 0) return Json(new { Error = "Стоимость должна быть положительной" });
            }
            else
            {
                cartridge.Cost = 0;
            }

            using (var db = new DevinContext())
            {
                var old = db.Cartridges.FirstOrDefault(x => x.Id == cartridge.Id);
                if (old == null) return Json(new { Error = "Запись, которую нужно обновить, не найдена в базе данных" });

                var log = new List<string>();
                if (old.Name != cartridge.Name) log.Add("наименование [\"" + old.Name + "\" => \"" + cartridge.Name + "\"]");
                if (old.Type != cartridge.Type) log.Add("тип [\"" + old.Type + "\" => \"" + cartridge.Type + "\"]");
                if (old.Color != cartridge.Color) log.Add("цвет [\"" + old.Color + "\" => \"" + cartridge.Color + "\"]");
                if (old.Description != cartridge.Description) log.Add("описание [\"" + old.Description + "\" => \"" + cartridge.Description + "\"]");
                if (old.Cost != cartridge.Cost) log.Add("стоимость [\"" + old.Cost + "\" => \"" + cartridge.Cost + "\"]");

                if (log.Count > 0)
                {
                    db.Cartridges
                        .Where(x => x.Id == cartridge.Id)
                        .Set(x => x.Name, cartridge.Name)
                        .Set(x => x.Type, cartridge.Type)
                        .Set(x => x.Cost, cartridge.Cost)
                        .Set(x => x.Description, cartridge.Description)
                        .Update();
                        
                    db.Log(User, "cartridges", cartridge.Id, "Картридж изменен. Изменения: " + log.ToLog());

                    return Json(new { Good = "Картридж изменен. Изменения:<br />" + log.ToHtml() });
                }
                else
                {
                    return Json(new { Warning = "Изменений не было" });
                }
            }
        }

        public JsonResult DeleteCartridge(int Id)
        {
            using (var db = new DevinContext())
            {
                db.Cartridges.Where(x => x.Id == Id).Delete();
                db.Log(User, "cartridges", Id, "Картридж удален");

                return Json(new { Good = "Картридж удален" });
            }
        }


        public JsonResult CreatePrinter()
        {
            using (var db = new DevinContext())
            {
                int Id = db.InsertWithInt32Identity(new Printer { Name = "Новый типовой принтер" });
                db.Log(User, "printers", Id, "Создан новый принтер");

                return Json(new { Good = "Создан новый принтер", Id = "prn" + Id });
            }
        }

        public JsonResult UpdatePrinter([Bind(Include = "Id,Name,Description")] Printer printer)
        {
            using (var db = new DevinContext())
            {
                var old = db.Printers.FirstOrDefault(x => x.Id == printer.Id);
                if (old == null) return Json(new { Error = "Запись, которую необходимо обновить, не найдена в базе данных" });

                var log = new List<string>();

                if (old.Name != printer.Name) log.Add("наименование [\"" + old.Name + "\" => \"" + printer.Name + "\"]");
                if (old.Description != printer.Description) log.Add("описание [\"" + old.Description + "\" => \"" + printer.Description + "\"]");

                if (log.Count > 0)
                {
                    db.Printers
                        .Where(x => x.Id == printer.Id)
                        .Set(x => x.Name, printer.Name)
                        .Set(x => x.Description, printer.Description)
                        .Update();
                    
                    db.Log(User, "printers", printer.Id, "Картридж изменен. Изменения: " + log.ToLog());

                    return Json(new { Good = "Картридж изменен. Изменения:<br />" + log.ToHtml() });
                }
                else
                {
                    return Json(new { Warning = "Изменений не было" });
                }
            }
        }

        public JsonResult DeletePrinter(int Id)
        {
            using (var db = new DevinContext())
            {
                db.Printers.Where(x => x.Id == Id).Delete();
                db.Log(User, "printers", Id, "Принтер удален");

                return Json(new { Good = "Принтер удален" });
            }
        }
        

        public JsonResult CreateCompare(int CartridgeId, int PrinterId)
        {
            using (var db = new DevinContext())
            {
                string cartName = db.Cartridges.First(x => x.Id == CartridgeId)?.Name ?? "";
                string prnName = db.Printers.First(x => x.Id == PrinterId)?.Name ?? "";

                db.Insert(new PrinterCartridge
                {
                    PrinterId = PrinterId,
                    CartridgeId = CartridgeId
                });

                db.Log(User, "printers", PrinterId, "Создана связь c картриджем \"" + cartName + "\" [cart" + CartridgeId + "]");
                db.Log(User, "cartridges", CartridgeId, "Создана связь c принтером \"" + prnName + "\" [prn" + PrinterId + "]");

                return Json(new { Good = "Связь создана" });
            }
        }

        public JsonResult DeleteCompare(int CartridgeId, int PrinterId)
        {
            using (var db = new DevinContext())
            {
                string cartName = db.Cartridges.First(x => x.Id == CartridgeId)?.Name ?? "";
                string prnName = db.Printers.First(x => x.Id == PrinterId)?.Name ?? "";

                db._PrinterCartridge.Where(x => x.CartridgeId == CartridgeId && x.PrinterId == PrinterId).Delete();

                db.Log(User, "printers", PrinterId, "Удалена связь c картриджем \"" + cartName + "\" [cart" + CartridgeId + "]");
                db.Log(User, "cartridges", CartridgeId, "Удалена связь c принтером \"" + prnName + "\" [prn" + PrinterId + "]");

                return Json(new { Good = "Связь удалена" });
            }
        }
    }
}