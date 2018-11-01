using Dapper;
using Devin.Models;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using System;
using System.Collections.Generic;
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

        public ActionResult Cart(int Id) => View(model: Id);

        public ActionResult History(int Id) => View(model: Id);

        public ActionResult Import()
        {
            var excels = new List<Storage>();

            var file = Request.Files[Request.Files.AllKeys[0]];
            var book = new HSSFWorkbook(file.InputStream);
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

                    excels.Add(new Storage
                    {
                        Name = row.GetCell(10).StringCellValue,
                        Inventory = row.GetCell(11).StringCellValue,
                        Cost = (float)row.GetCell(12).NumericCellValue,
                        Date =  d,
                        Nall = (int)row.GetCell(28).NumericCellValue,
                        Account = row.GetCell(32).StringCellValue
                    });

                    if (!title.EndsWith(row.GetCell(32).StringCellValue)) title += " " + row.GetCell(32).StringCellValue;
                }
            }

            ViewBag.Title = title;
            return View(excels);
        }

        
        public JsonResult Create()
        {
            using (var conn = Database.Connection())
            {
                conn.Execute("INSERT INTO Storages (Inventory, Name, Nall, Nstorage, Nrepairs, Noff, Date, CartridgeId, IsDeleted) VALUES ('', '', 1, 1, 0, 0, @Date, 0, 0)", new { DateTime.Now.Date });
                int Id = conn.QueryFirst<int>("SELECT Max(Id) FROM Storages");

                conn.Execute("INSERT INTO Activity (Date, Source, Id, Text, Username) VALUES (GetDate(), 'storages', @Id, @Text, @Username)", new Activity
                {
                    Id = Id.ToString(),
                    Text = "Создана новая позиция",
                    Username = User.Identity.Name
                });

                return Json(new { Good = "Создана новая позиция", Id });
            }
        }

        public JsonResult Update(int Id, [Bind(Include = "Id,Inventory,Name,Nall,Nstorage,Nrepairs,Noff,CartridgeId,Type,Account,FolderId")] Storage storage)
        {
            // Валидация 
            if (!DateTime.TryParse(Request.Form.Get("Date"), out DateTime d)) return Json(new { Error = "Дата прихода введена в неверном формате" }); else storage.Date = d;
            if (!float.TryParse(Request.Form.Get("Cost"), out float f)) return Json(new { Error = "Стоимость введена в неверном формате" }); else storage.Cost = f;
            if (storage.Cost < 0) return Json(new { Error = "Стоимость не может быть отрицательной" });
            if (storage.Nall < 0) return Json(new { Error = "Количество прихода не может быть отрицательным" });

            using (var conn = Database.Connection())
            {
                // Логирование изменений
                var old = conn.Query<Storage>("SELECT * FROM Storages WHERE Id = @Id", new { Id }).FirstOrDefault() ?? new Storage();

                var changes = new List<string>();
                if (old.Inventory != storage.Inventory) changes.Add($"инвентарный номер [{ old.Inventory} => {storage.Inventory}]");
                if ((old.Name ?? "") != (storage.Name ?? "")) changes.Add($"наименование [\"{ old.Name}\" => \"{storage.Name}\"]");
                if (old.Nall != storage.Nall) changes.Add($"кол-во прихода [{ old.Nall} => {storage.Nall}]");
                if (old.Nstorage != storage.Nstorage) changes.Add($"кол-во на складе [{ old.Nstorage} => {storage.Nstorage}]");
                if (old.Nrepairs != storage.Nrepairs) changes.Add($"кол-во используемых [{ old.Nrepairs} => {storage.Nrepairs}]");
                if (old.Noff != storage.Noff) changes.Add($"кол-во списанных [{ old.Noff} => {storage.Noff}]");
                if (old.CartridgeId != storage.CartridgeId) changes.Add($"типовой картридж [{ old.CartridgeId} => {storage.CartridgeId}]");
                if (old.Type != storage.Type) changes.Add($"тип позиции [{ old.Type} => {storage.Type}]");
                if (old.Account != storage.Account) changes.Add($"счет учета [{ old.Account} => {storage.Account}]");
                if (old.FolderId != storage.FolderId) changes.Add($"папка [{ old.FolderId} => {storage.FolderId}]");
                if (old.Date != storage.Date) changes.Add($"дата прихода [{ old.Date} => {storage.Date}]");
                if (old.Cost != storage.Cost) changes.Add($"стоимость [{ old.Cost} => {storage.Cost}]");

                if (changes.Count > 0)
                {
                    // Сохранение в базе
                    conn.Execute(@"UPDATE Storages SET
                        Inventory   = @Inventory,
                        Name        = @Name,
                        Type        = @Type,
                        Cost        = @Cost,
                        Nall        = @Nall,
                        Nstorage    = @Nstorage,
                        Nrepairs    = @Nrepairs,
                        Noff        = @Noff,
                        Date        = @Date,
                        Account     = @Account,
                        CartridgeId = @CartridgeId,
                        FolderId    = @FolderId
                    WHERE Id = @Id", storage);

                    conn.Execute("INSERT INTO Activity (Date, Source, Id, Text, Username) VALUES (GetDate(), 'storages', @Id, @Text, @Username)", new Activity
                    {
                        Id = storage.Id.ToString(),
                        Text = "Позиция изменена. Изменения: " + string.Join(",\n", changes.ToArray()),
                        Username = User.Identity.Name
                    });

                    return Json(new { Good = "Позиция успешно обновлена! Изменены поля: <br />" + string.Join(",<br />", changes.ToArray()) });
                }
                else
                {
                    return Json(new { Warning = "Изменений не было" });
                }
            }
        }

        public JsonResult Delete(int Id)
        {
            using (var conn = Database.Connection())
            {
                conn.Execute("DELETE FROM Storages WHERE Id = @Id", new { Id });
                conn.Execute("INSERT INTO Activity (Date, Source, Id, Text, Username) VALUES (GetDate(), 'storages', @Id, @Text, @Username)", new Activity
                {
                    Id = Id.ToString(),
                    Text = "Позиция удалена",
                    Username = User.Identity.Name
                });

                return Json(new { Good = "Позиция удалена" });
            }
        }

        public JsonResult Move(string Select, int FolderId)
        {
            string[] Storages = Select.Split(new string[] { ";;" }, StringSplitOptions.RemoveEmptyEntries);

            using (var conn = Database.Connection())
            {
                foreach (string storage in Storages)
                {
                    conn.Execute("UPDATE Storages SET FolderId = @FolderId WHERE Id = @Id", new { FolderId, Id = int.TryParse(storage, out int i) ? i : 0 });
                }

                string name = conn.Query<string>("SELECT Name FROM Folders WHERE Id = @FolderId", new { FolderId }).FirstOrDefault();
                if (string.IsNullOrEmpty(name))
                {
                    return Json(new { Good = "Выбранные позиции размещены отдельно" });
                }
                else
                {
                    
                    return Json(new { Good = "Выбранные позиции перемещены в папку \"" + name + "\"" });
                }
            }
        }

        public JsonResult AddExcelToStorage([Bind(Include = "Inventory,Name,Date,Account,Nall,Cost")] Storage storage)
        {
            string name = storage.Name.ToLower();

            // Картриджи
            if (name.Contains("картридж") || name.Contains("тонер") || name.Contains("чернильница") || name.Contains("катридж")) storage.Type = "PRN";

            // Мониторы
            else if (name.Contains("монитор")) storage.Type = "DIS";

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
                || name.Contains("привод")) storage.Type = "CMP";

            // Периферия
            else if (name.Contains("клави") || name.Contains("мыш")) storage.Type = "INP";

            // Коммутаторы
            else if (name.Contains("коммутатор")) storage.Type = "SWT";

            // Периферия
            else if (name.Contains("батарея") || name.Contains("ибп") || name.Contains("элемент питания")) storage.Type = "UPS";

            // Другое
            else storage.Type = "RR";

            using (var conn = Database.Connection())
            {
                conn.Execute("INSERT INTO Storages (Inventory, Name, Date, Account, Nall, Nstorage, Nrepairs, Noff, IsDeleted, Type, Cost) VALUES (@Inventory, @Name, @Date, @Account, @Nall, @Nall, 0, 0, 1, @Type, @Cost)", storage);
                int Id = conn.QueryFirst<int>("SELECT Max(Id) FROM Storages");

                return Json(new { Good = "Позиция \"" + storage.Name + "\" с инв. номером \"" + storage.Inventory +"\" добавлена на склад", Id });
            }
        }

        public JsonResult Labels(string Select)
        {
            List<Storage> Storages;

            using (var conn = Database.Connection())
            {
                Storages = conn.Query<Storage>(@"SELECT
                    Storages.Id,
                    Storages.Nstorage,
                    Storages.Inventory,
                    Cartridges.Name AS [Type],
                    Storages.Date,
                    Storages.Name
                FROM Storages
                LEFT OUTER JOIN Cartridges ON Storages.CartridgeId = Cartridges.Id
                WHERE Storages.Id IN (" + Select + ") ORDER BY Storages.Date DESC").AsList();
            }

            if (Storages.Count == 0) return Json(new { Warning = "Нечего печатать" });
            if (Storages.Select(x => x.Nstorage).Sum() == 0) return Json(new { Warning = "На складе нет выбранных позиций" });

            HSSFWorkbook book;
            using (var fs = new FileStream(Server.MapPath("../content/exl/") + "labels.xls", FileMode.Open, FileAccess.Read))
            {
                book = new HSSFWorkbook(fs);
            }

            var sheet = book.GetSheetAt(0);

            bool isLeft = true;
            int rowCount = 1;

            for (int i = 0; i < Storages.Count; i++)
            {
                for (int j = 0; j < Storages[i].Nstorage; j++)
                {
                    if (isLeft)
                    {
                        sheet.GetRow(rowCount * 3 - 3).GetCell(0).SetCellValue("№");
                        sheet.GetRow(rowCount * 3 - 2).GetCell(0).SetCellValue("Тип:");
                        sheet.GetRow(rowCount * 3 - 1).GetCell(0).SetCellValue("Приход:");

                        sheet.GetRow(rowCount * 3 - 3).GetCell(1).SetCellValue(Storages[i].Inventory);
                        sheet.GetRow(rowCount * 3 - 2).GetCell(1).SetCellValue(Storages[i].Type ?? Storages[i].Name);
                        sheet.GetRow(rowCount * 3 - 1).GetCell(1).SetCellValue(Storages[i].Date.ToString("dd.MM.yyyy"));

                        isLeft = false;
                    }
                    else
                    {
                        sheet.GetRow(rowCount * 3 - 3).GetCell(2).SetCellValue("№");
                        sheet.GetRow(rowCount * 3 - 2).GetCell(2).SetCellValue("Тип:");
                        sheet.GetRow(rowCount * 3 - 1).GetCell(2).SetCellValue("Приход:");

                        sheet.GetRow(rowCount * 3 - 3).GetCell(3).SetCellValue(Storages[i].Inventory);
                        sheet.GetRow(rowCount * 3 - 2).GetCell(3).SetCellValue(Storages[i].Type ?? Storages[i].Name);
                        sheet.GetRow(rowCount * 3 - 1).GetCell(3).SetCellValue(Storages[i].Date.ToString("dd.MM.yyyy"));
                        
                        rowCount = rowCount + 1;
                        isLeft = true;
                    }
                }
            }

            using (var fs = new FileStream(Server.MapPath("../content/excels/") + "Бирки.xls", FileMode.OpenOrCreate, FileAccess.Write))
            {
                book.Write(fs);
            }

            return Json(new { Good = "Создание файла с бирками завершено", Link = Url.Action("excels", "content") + "/Бирки.xls" });
        }

        public JsonResult AnalyzePrint(string Data)
        {
            // Получение исходных данных
            float cost = 0;
            int count = 0;
            var types = new List<List<Cartridge>>();
            var lastType = new List<Cartridge>();
            string lastName = "";

            string[] DataSplit = (Data ?? "").Split(new string[] { "----" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string data in DataSplit)
            {
                string[] raw = data.Split(new string[] { "__" }, StringSplitOptions.RemoveEmptyEntries);

                var cartridge = new Cartridge
                {
                    Name = raw[0],
                    Count = int.Parse(raw[3]),
                    Cost = float.Parse(raw[1])
                };

                switch (raw[2]) {
                    case "flow": cartridge.Type = "Картридж струйный"; break;
                    case "laser": cartridge.Type = "Тонер-картридж"; break;
                    case "matrix": cartridge.Type = "Матричная лента"; break;
                }

                switch (raw[4])
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
            HSSFWorkbook book;
            using (var fs = new FileStream(Server.MapPath("../content/exl/") + "analyze.xls", FileMode.Open, FileAccess.Read))
            {
                book = new HSSFWorkbook(fs);
            }
            var sheet = book.GetSheetAt(0);


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
            string name = "Заявка на закупку картриджей " + DateTime.Now.ToString("d MMMM yyyy") + ".xls";
            using (var fs = new FileStream(Server.MapPath("../content/excels/") + name, FileMode.OpenOrCreate, FileAccess.Write))
            {
                book.Write(fs);
            }

            return Json(new { Good = "Создание заявки на закупку завершено", Link = Url.Action("excels", "content") + name });
        }
    }
}