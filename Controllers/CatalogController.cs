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

        public void CreateCompare()
        {
            using (var conn = Database.Connection())
            {
                string mode = Request.Form.Get("active");
                int Id = int.Parse(Request.Form.Get("id").Replace("prn", "").Replace("cart", ""));
                int Compare = int.Parse(Request.Form.Get("compare"));

                switch (mode)
                {
                    case "printer":
                        conn.Execute("INSERT INTO PrintersCartridges (PrinterId, CartridgeId) VALUES (@Id, @Compare)", new { Id, Compare });
                        break;
                    case "cartridge":
                        conn.Execute("INSERT INTO PrintersCartridges (PrinterId, CartridgeId) VALUES (@Compare, @Id)", new { Id, Compare });
                        break;
                }
            }
        }

        public int CreateCartridge()
        {
            using (var conn = Database.Connection())
            {
                conn.Execute("INSERT INTO Cartridges (Name, Type, Color, Cost) VALUES ('Новый типовой картридж', '', '', null)");
                return conn.QueryFirst<int>("SELECT Max(Id) FROM Cartridges");
            }
        }

        public int CreatePrinter()
        {
            using (var conn = Database.Connection())
            {
                conn.Execute("INSERT INTO Printers (Name) VALUES ('Новый типовой принтер')");
                return conn.QueryFirst<int>("SELECT Max(Id) FROM Name");
            }
        }

        public void DeleteCartridge(int Id)
        {
            using (var conn = Database.Connection())
            {
                conn.Execute("DELETE FROM Cartridges WHERE Id = @Id", new { Id });
            }
        }

        public void DeletePrinter(int Id)
        {
            using (var conn = Database.Connection())
            {
                conn.Execute("DELETE FROM Printers WHERE Id = @Id", new { Id });
            }
        }

        public void DeleteCompare()
        {
            using (var conn = Database.Connection())
            {
                string mode = Request.Form.Get("active");
                int Id = int.Parse(Request.Form.Get("id").Replace("prn", "").Replace("cart", ""));
                int Compare = int.Parse(Request.Form.Get("compare"));

                switch (mode)
                {
                    case "printer":
                        conn.Execute("DELETE FROM PrintersCartridges WHERE PrinterId = @Id AND CartridgeId = @Compare", new { Id, Compare });
                        break;
                    case "cartridge":
                        conn.Execute("DELETE FROM PrintersCartridges WHERE CartridgeId = @Id AND PrinterId = @Compare", new { Id, Compare });
                        break;
                }
            }
        }

        public string SaveCartridge([Bind(Include = "Id,Name,Type,Color,Description")] Cartridge cartridge)
        {
            using (var conn = Database.Connection())
            {
                string cost = Request.Form.Get("Cost");
                if (!string.IsNullOrEmpty(cost))
                {
                    if (!float.TryParse(cost, out float f)) return "error Стоимость введена в невалидном формате"; else cartridge.Cost = f;
                    if (cartridge.Cost < 0) return "error Стоимость должна быть положительной";
                }
                else
                {
                    cartridge.Cost = 0;
                }
                

                conn.Execute("UPDATE Cartridges SET Name = @Namen, Cost = @Cost, Type = @Type, Color = @Color, Description = @Description WHERE Id = @Id", cartridge);
                return "Карточка успешно сохранена";
            }
        }

        public string SavePrinter([Bind(Include = "Id,Name,Description")] Printer printer)
        {
            using (var conn = Database.Connection())
            {
                conn.Execute("UPDATE Printers SET Name = @Name, Description = @Description WHERE Id = @Id", printer);
                return "Карточка успешно сохранена";
            }
        }
    }
}