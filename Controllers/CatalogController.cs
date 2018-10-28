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
                        conn.Execute("INSERT INTO Office (Printer, Cartridge) VALUES (@Id, @Compare)", new { Id, Compare });
                        break;
                    case "cartridge":
                        conn.Execute("INSERT INTO Office (Printer, Cartridge) VALUES (@Compare, @Id)", new { Id, Compare });
                        break;
                }
            }
        }

        public int CreateCartridge()
        {
            using (var conn = Database.Connection())
            {
                conn.Execute("INSERT INTO Cartridge (Caption, Type, Color, Price) VALUES ('Новый типовой картридж', '', '', null)");
                return conn.QueryFirst<int>("SELECT Max(N) FROM Cartridge");
            }
        }

        public int CreatePrinter()
        {
            using (var conn = Database.Connection())
            {
                conn.Execute("INSERT INTO Printer (Caption) VALUES ('Новый типовой принтер')");
                return conn.QueryFirst<int>("SELECT Max(N) FROM Cartridge");
            }
        }

        public void DeleteCartridge(int Id)
        {
            using (var conn = Database.Connection())
            {
                conn.Execute("DELETE FROM Cartridge WHERE N = @Id", new { Id });
            }
        }

        public void DeletePrinter(int Id)
        {
            using (var conn = Database.Connection())
            {
                conn.Execute("DELETE FROM Printer WHERE N = @Id", new { Id });
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
                        conn.Execute("DELETE FROM Office WHERE Printer = @Id AND Cartridge = @Compare", new { Id, Compare });
                        break;
                    case "cartridge":
                        conn.Execute("DELETE FROM Office WHERE Cartridge = @Id AND Printer = @Compare", new { Id, Compare });
                        break;
                }
            }
        }

        public string SaveCartridge([Bind(Include = "Id,Caption,Type,Color,Description")] Cartridge cartridge)
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
                

                conn.Execute("UPDATE Cartridge SET Caption = @Caption, Price = @Cost, Type = @Type, Color = @Color, Description = @Description WHERE N = @Id", cartridge);
                return "Карточка успешно сохранена";
            }
        }

        public string SavePrinter([Bind(Include = "Id,Caption,Description")] Printer printer)
        {
            using (var conn = Database.Connection())
            {
                conn.Execute("UPDATE Printer SET Caption = @Caption, Description = @Description WHERE N = @Id", printer);
                return "Карточка успешно сохранена";
            }
        }
    }
}