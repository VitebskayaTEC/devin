using Dapper;
using Devin.Forms;
using Devin.Models;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace Devin.Controllers
{
    public class StoragesController : Controller
    {
        public ActionResult Index() => View();

        public ActionResult List() => View();

        public ActionResult Analyze() => View();

        public ActionResult Cart(string Id) => View(model: Id);

        public ActionResult Repairs() => View();

        public ActionResult Import()
        {
            var model = new StorageCompare();

            var file = Request.Files[Request.Files.AllKeys[0]];
            var book = new XSSFWorkbook(file.InputStream);
            var sheet = book.GetSheetAt(0);

            string title = "";

            for (int i = 10; i < sheet.LastRowNum; i++)
            {
                var row = sheet.GetRow(i);
                if (row.GetCell(32).StringCellValue != "")
                {
                    row.GetCell(28).SetCellType(CellType.Numeric);

                    string date = row.GetCell(15).StringCellValue;
                    DateTime.TryParse(date.Substring(0, date.Length - 2) + "20" + date.Substring(date.Length - 2), out DateTime d);

                    model.Excels.Add(new Storage
                    {
                        Name = row.GetCell(10).StringCellValue,
                        Ncard = row.GetCell(11).NumericCellValue.ToString(),
                        Price = (float)row.GetCell(12).NumericCellValue,
                        Date =  d,
                        Nadd = (int)row.GetCell(28).NumericCellValue,
                        Uchet = row.GetCell(32).StringCellValue
                    });

                    if (!title.EndsWith(row.GetCell(32).StringCellValue)) title += " " + row.GetCell(32).StringCellValue;
                }
            }

            using (var conn = Database.Connection())
            {
                model.Storages = conn.Query<Storage>("SELECT * FROM Sklad ORDER BY Ncard").AsList();
            }

            ViewBag.Title = title;
            return View(model);
        }


        public string Create(string Id)
        {
            using (var conn = Database.Connection())
            {
                conn.Execute("INSERT INTO Sklad (Ncard, Name, Nadd, Nis, Nuse, Nbreak, Date, Id_Cart, delit) VALUES (@Id, '', 1, 1, 0, 0, @Date, 0, 1)", new { Id, DateTime.Now.Date });

                conn.Execute("INSERT INTO Activity (Date, Source, Id, Text, Username) VALUES (@Date, @Source, @Id, @Text, @Username)", new Activity
                {
                    Date = DateTime.Now,
                    Source = "storages",
                    Id = Id,
                    Text = "Создана позиция. Инвертарный номер [" + Id + "]",
                    Username = User.Identity.Name
                });

                return Id.ToString();
            }
        }

        public string Update(string Id, [Bind(Include = "Ncard,Name,Nadd,Nis,Nuse,Nbreak,Id_Cart,Class_Name,Uchet,G_Id")] Storage storage)
        {
            // Валидация 
            if (!DateTime.TryParse(Request.Form.Get("Date"), out DateTime d)) return "error:Дата прихода введена в неверном формате"; else storage.Date = d;
            if (!float.TryParse(Request.Form.Get("Price"), out float f)) return "error:Стоимость введена в неверном формате"; else storage.Price = f;
            if (storage.Price < 0) return "error:Стоимость не может быть отрицательной";
            if (storage.Nadd < 0) return "error:Количество прихода не может быть отрицательным";

            using (var conn = Database.Connection())
            {
                string newNcard = "";

                if (storage.Ncard != Id)
                {
                    if (conn.Query("SELECT Ncard FROM Sklad WHERE Ncard = @Ncard", new { storage.Ncard }).Count() != 0) return "error:Введенный инвертарный номер не является уникальным";

                    // Обновление всех логов по инвертарному, чтобы не потерять их, когда поменяется инвентарный номер позиции
                    conn.Execute("UPDATE Activity SET Id = @Id WHERE Source = 'storages' AND Id = @OldId", new { Id = storage.Ncard, OldId = Id });
                    newNcard = "<div class='hide' id>" + storage.Ncard + "</div>";
                }

                // Логирование изменений
                var _old = conn.Query<Storage>("SELECT Ncard, Name, Class_Name, Price, Nadd, Nis, Nuse, Nbreak, Date, Uchet, Id_Cart, G_Id, Delit FROM Sklad WHERE Ncard = @Id", new { Id }).FirstOrDefault() ?? new Storage();

                var changes = new List<string>();
                if (_old.Ncard != storage.Ncard) changes.Add($"инвентарный номер [{ _old.Ncard} => {storage.Ncard}]");
                if ((_old.Name ?? "") != (storage.Name ?? "")) changes.Add($"наименование [\"{ _old.Name}\" => \"{storage.Name}\"]");
                if (_old.Nadd != storage.Nadd) changes.Add($"кол-во прихода [{ _old.Nadd} => {storage.Nadd}]");
                if (_old.Nis != storage.Nis) changes.Add($"кол-во на складе [{ _old.Nis} => {storage.Nis}]");
                if (_old.Nuse != storage.Nuse) changes.Add($"кол-во используемых [{ _old.Nuse} => {storage.Nuse}]");
                if (_old.Nbreak != storage.Nbreak) changes.Add($"кол-во списанных [{ _old.Nbreak} => {storage.Nbreak}]");
                if (_old.Id_Cart != storage.Id_Cart) changes.Add($"типовой картридж [{ _old.Id_Cart} => {storage.Id_Cart}]");
                if (_old.Class_Name != storage.Class_Name) changes.Add($"тип позиции [{ _old.Class_Name} => {storage.Class_Name}]");
                if (_old.Uchet != storage.Uchet) changes.Add($"счет учета [{ _old.Uchet} => {storage.Uchet}]");
                if (_old.G_Id != storage.G_Id) changes.Add($"папка [{ _old.G_Id} => {storage.G_Id}]");
                if (_old.Date != storage.Date) changes.Add($"дата прихода [{ _old.Date} => {storage.Date}]");
                if (_old.Price != storage.Price) changes.Add($"стоимость [{ _old.Price} => {storage.Price}]");

                if (changes.Count > 0)
                {
                    // Сохранение в базе
                    conn.Execute(@"UPDATE Sklad SET
                        Ncard      = @Ncard,
                        Name       = @Name,
                        Class_Name = @Class_Name,
                        Price      = @Price,
                        Nadd       = @Nadd,
                        Nis        = @Nis,
                        Nuse       = @Nuse,
                        Nbreak     = @Nbreak,
                        Date       = @Date,
                        Uchet      = @Uchet,
                        Id_Cart    = @Id_Cart,
                        G_Id       = @G_Id
                    WHERE Ncard = '" + Id + "'", storage);

                    conn.Execute("INSERT INTO Activity (Date, Source, Id, Text, Username) VALUES (@Date, @Source, @Id, @Text, @Username)", new Activity
                    {
                        Date = DateTime.Now,
                        Source = "storages",
                        Id = storage.Ncard,
                        Text = "Позиция изменена. Изменения: " + string.Join(",\n", changes.ToArray()),
                        Username = User.Identity.Name
                    });

                    return "Позиция успешно обновлена! Изменены поля: <br />" + string.Join(",<br />", changes.ToArray()) + newNcard;
                }
                else
                {
                    return "Изменений не было";
                }
            }
        }

        public void Delete(string Id)
        {
            using (var conn = Database.Connection())
            {
                conn.Execute("DELETE FROM Sklad WHERE Ncard = @Id", new { Id });

                conn.Execute("INSERT INTO Activity (Date, Source, Id, Text, Username) VALUES (@Date, @Source, @Id, @Text, @Username)", new Activity
                {
                    Date = DateTime.Now,
                    Source = "storages",
                    Id = Id,
                    Text = "Позиция удалена",
                    Username = User.Identity.Name
                });
            }
        }

        public void AddExcelToStorage([Bind(Include = "Ncard,Name,Date,Uchet,Nadd,Price")] Storage storage)
        {
            string name = storage.Name.ToLower();

            // Картриджи
            if (name.Contains("картридж") || name.Contains("тонер") || name.Contains("чернильница") || name.Contains("катридж")) storage.Class_Name = "PRN";

            // Мониторы
            else if (name.Contains("монитор")) storage.Class_Name = "DIS";

            // Комплектующие
            else if (name.Contains("блок") 
                || name.Contains("диск") 
                || name.Contains("накопитель") 
                || name.Contains("клави") 
                || name.Contains("мыш") 
                || name.Contains("памят") 
                || name.Contains("озу") 
                || name.Contains("плата") 
                || name.Contains("процессор ")
                || name.Contains("видеокарта")
                || name.Contains("видеоплата")
                || name.Contains("привод")) storage.Class_Name = "CMP";

            // Периферия
            else if (name.Contains("клави") || name.Contains("мыш")) storage.Class_Name = "INP";

            // Коммутаторы
            else if (name.Contains("коммутатор")) storage.Class_Name = "SWT";

            // Периферия
            else if (name.Contains("батарея") || name.Contains("ибп") || name.Contains("элемент питания")) storage.Class_Name = "UPS";

            // Другое
            else storage.Class_Name = "RR";

            using (var conn = Database.Connection())
            {
                conn.Execute("INSERT INTO Sklad (Ncard, Name, Date, Uchet, Nadd, Nis, Nuse, Nbreak, delit, class_name, Price) VALUES (@Ncard, @Name, @Date, @Uchet, @Nadd, @Nadd, 0, 0, 1, @Class_Name, @Price)", storage);
            }
        }

        public string Labels()
        {
            string sql = "SELECT SKLAD.Nis, SKLAD.Ncard, CARTRIDGE.Caption, SKLAD.Date, SKLAD.Name FROM SKLAD LEFT OUTER JOIN CARTRIDGE ON SKLAD.ID_cart = CARTRIDGE.N WHERE Ncard = '" + (Request.Form.Get("select") ?? "1").Replace(";", "' OR Ncard = '") + "' ORDER BY Date DESC";

            using (var conn = Database.Connection())
            {
                var model = conn.Query(sql).AsList();

                if (model.Count == 0) return "Нечего печатать";

                IWorkbook book;
                using (var fs = new FileStream(Server.MapPath("../content/exl/") + "labels.xls", FileMode.Open, FileAccess.Read))
                {
                    book = new HSSFWorkbook(fs);
                }
                var sheet = book.GetSheetAt(0);

                bool isLeft = true;
                int rowCount = 1;

                for (int i = 0; i < model.Count; i++)
                {
                    for (int j = 0; j < model[i].Nis; j++)
                    {
                        if (isLeft)
                        {
                            sheet.GetRow(rowCount * 3 - 3).GetCell(0).SetCellValue("№");
                            sheet.GetRow(rowCount * 3 - 2).GetCell(0).SetCellValue("Тип:");
                            sheet.GetRow(rowCount * 3 - 1).GetCell(0).SetCellValue("Приход:");

                            sheet.GetRow(rowCount * 3 - 3).GetCell(1).SetCellValue(model[i].Ncard);
                            sheet.GetRow(rowCount * 3 - 2).GetCell(1).SetCellValue(model[i].Caption ?? model[i].Name);
                            sheet.GetRow(rowCount * 3 - 1).GetCell(1).SetCellValue(model[i].Date.ToString("dd.MM.yyyy"));

                            isLeft = false;
                        }
                        else
                        {
                            sheet.GetRow(rowCount * 3 - 3).GetCell(2).SetCellValue("№");
                            sheet.GetRow(rowCount * 3 - 2).GetCell(2).SetCellValue("Тип:");
                            sheet.GetRow(rowCount * 3 - 1).GetCell(2).SetCellValue("Приход:");

                            sheet.GetRow(rowCount * 3 - 3).GetCell(3).SetCellValue(model[i].Ncard);
                            sheet.GetRow(rowCount * 3 - 2).GetCell(3).SetCellValue(model[i].Caption ?? model[i].Name);
                            sheet.GetRow(rowCount * 3 - 1).GetCell(3).SetCellValue(model[i].Date.ToString("dd.MM.yyyy"));


                            rowCount = rowCount + 1;
                            isLeft = true;
                        }
                    }
                }
                
                using (var fs = new FileStream(Server.MapPath("../content/excels/") + "Бирки.xls", FileMode.OpenOrCreate, FileAccess.Write))
                {
                    book.Write(fs);
                }

                return Url.Action("excels", "content") + "/Бирки.xls";
            }
        }

        public string AnalyzePrint()
        {
            // Получение исходных данных
            float cost = 0;
            int count = 0;
            var types = new List<List<Cartridge>>();
            var lastType = new List<Cartridge>();
            string lastName = "";

            string[] Data = (Request.Form.Get("data") ?? "").Split(new string[] { "----" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string data in Data)
            {
                string[] _ = data.Split(new string[] { "__" }, StringSplitOptions.RemoveEmptyEntries);

                var cartridge = new Cartridge
                {
                    Name = _[0],
                    Count = int.Parse(_[3]),
                    Cost = float.Parse(_[1])
                };

                switch (_[2]) {
                    case "flow": cartridge.Type = "Картридж струйный"; break;
                    case "laser": cartridge.Type = "Тонер-картридж"; break;
                    case "matrix": cartridge.Type = "Матричная лента"; break;
                }

                switch (_[4])
                { 
                    case "black": cartridge.Color = "черный"; break;
                    case "blue": cartridge.Color = "голубой"; break;
                    case "red": cartridge.Color = "красный"; break;
                    case "yellow": cartridge.Color = "желтый"; break;
                    case "3color": cartridge.Color = "трехцветный"; break;
                    case "5color": cartridge.Color = "многоцветный"; break;
                }

                if (lastName != cartridge.Type)
                {
                    types.Add(lastType = new List<Cartridge>());
                    lastName = cartridge.Type;
                }

                lastType.Add(cartridge);

                cost += cartridge.Cost;
                count++;
            }


            // Открытие шаблона
            IWorkbook book;
            using (var fs = new FileStream(Server.MapPath("/devin/content/exl/") + "analyze.xls", FileMode.Open, FileAccess.Read))
            {
                book = new HSSFWorkbook(fs);
            }
            ISheet sheet = book.GetSheetAt(0);


            // Заполнение полей
            int month = DateTime.Now.Month;
            string quarter = "";

            if (month > 8) quarter = "в IV квартале";
            else if (month > 5) quarter = "в III квартале";
            else if (month > 2) quarter = "во II квартале";
            else quarter = "в II квартале";

            sheet.GetRow(7).GetCell(1).SetCellValue(DateTime.Now.ToString("dd.MM.yyyy"));
            sheet.GetRow(12).GetCell(1).SetCellValue(
                sheet.GetRow(12).GetCell(1).StringCellValue.Replace("@quarter", quarter).Replace("@year", DateTime.Now.Year.ToString()));
            sheet.GetRow(18).GetCell(7).SetCellValue(cost.ToString() + " BYN");


            // Заполнение таблицы
            int startRegion = 17;
            int endRegion = 17;

            for (int i = 0; i < count - 1; i++) sheet.CopyRow(17, 18 + i);

            foreach (var type in types)
            {
                foreach (var cartridge in type)
                {
                    IRow row = sheet.GetRow(endRegion);
                    endRegion++;

                    row.GetCell(3).SetCellValue("шт.");
                    row.GetCell(4).SetCellValue(cartridge.Count);
                    row.GetCell(5).SetCellValue(cartridge.Name + ", " + cartridge.Color);
                    row.GetCell(7).SetCellValue(cartridge.Cost + " BYN за 1 шт.");
                    row.Height = -1;
                }

                sheet.GetRow(startRegion).GetCell(1).SetCellValue(type[0].Type);
                sheet.AddMergedRegion(new CellRangeAddress(startRegion, endRegion - 1, 1, 2));

                startRegion = endRegion;
            }

           
            // Сохранение документа
            using (var fs = new FileStream(Server.MapPath("/devin/content/excels/") + "analyze.xls", FileMode.OpenOrCreate, FileAccess.Write))
            {
                book.Write(fs);
            }

            return "<a href='/devin/content/excels/analyze.xls'>Сохранить файл</a>";
        }
    }
}