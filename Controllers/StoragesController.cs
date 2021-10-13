using Devin.Models;
using Devin.ViewModels;
using LinqToDB;
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

        public ActionResult Load(string Item, string Search)
        {
            var model = new StoragesViewModel(Search);

            if ((Item ?? "").Contains("folder"))
            {
                int Id = int.Parse(Item.Replace("folder", ""));
                if (Id < 0) return View("Storages", model.Storages.Where(x => x.IsOff()).ToList());
                return View("FolderData", Folder.FindSubFolder(model.Folders, Id));
            }
            else if (!string.IsNullOrEmpty(Search))
            {
                ViewBag.Search = Search;
                return View("Search", model.Storages);
            }
            else
            {
                return View("List", model);
            }
        }

        public ActionResult Analyze() => View();

        public ActionResult Cart(int Id) => View(model: Id);

        public ActionResult History(int Id) => View(model: Id);


        public JsonResult Create()
        {
            using (var db = new DevinContext())
            {
                int id = db.InsertWithInt32Identity(new Storage
                {
                    Inventory = "",
                    Name = "",
                    Nall = 1,
                    Nstorage = 1,
                    Nrepairs = 0,
                    Noff = 0,
                    Date = DateTime.Now,
                    CartridgeId = 0,
                    IsDeleted = false,
                    Cost = 0, 
                    FolderId = 0,
                    Type = ""
                });
                
                db.Log(User, "storages", id, "Создана новая позиция");

                return Json(new { Good = "Создана новая позиция", Id = "storage" + id });
            }
        }

        public JsonResult Update(
            [Bind(Include = "Id,Inventory,Description,Name,Nall,Nstorage,Nrepairs,Noff,CartridgeId,Type,Account,FolderId")] Storage storage, 
            string Date, 
            string Cost
        )
        {
            if (!DateTime.TryParse(Date, out DateTime d))
            {
                return Json(new { Error = "Дата прихода введена в неверном формате" });
            }
            else
            {
                storage.Date = d;
            }

            if (!float.TryParse(Cost, out float f))
            {
                return Json(new { Error = "Стоимость введена в неверном формате" });
            }
            else
            {
                storage.Cost = f;
            }

            if (storage.Cost < 0) return Json(new { Error = "Стоимость не может быть отрицательной" });
            if (storage.Nall < 0) return Json(new { Error = "Количество прихода не может быть отрицательным" });

            using (var db = new DevinContext())
            {
                var old = db.Storages.Where(x => x.Id == storage.Id).FirstOrDefault() ?? new Storage();

                var changes = new List<string>();

                if (old.Inventory != storage.Inventory) changes.Add($"инвентарный номер [{ old.Inventory} => {storage.Inventory}]");
                if (old.Description != storage.Description) changes.Add($"описание [{ old.Description} => {storage.Description}]");
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
                    db.Storages
                        .Where(x => x.Id == storage.Id)
                        .Set(x => x.Inventory, storage.Inventory)
                        .Set(x => x.Description, storage.Description)
                        .Set(x => x.Name, storage.Name)
                        .Set(x => x.Type, storage.Type)
                        .Set(x => x.Cost, storage.Cost)
                        .Set(x => x.Nall, storage.Nall)
                        .Set(x => x.Nstorage, storage.Nstorage)
                        .Set(x => x.Nrepairs, storage.Nrepairs)
                        .Set(x => x.Noff, storage.Noff)
                        .Set(x => x.Date, storage.Date)
                        .Set(x => x.Account, storage.Account)
                        .Set(x => x.CartridgeId, storage.CartridgeId)
                        .Set(x => x.FolderId, storage.FolderId)
                        .Update();

                    db.Log(User, "storages", storage.Id, "Позиция изменена. Изменения: " + changes.ToLog());

                    return Json(new { Good = "Позиция успешно обновлена! Изменены поля: <br />" + changes.ToHtml() });
                }
                else
                {
                    return Json(new { Warning = "Изменений не было" });
                }
            }
        }

        public JsonResult Delete(int Id)
        {
            using (var db = new DevinContext())
            {
                db.Storages.Delete(x => x.Id == Id);
                db.Log(User, "storages", Id, "Позиция удалена");

                return Json(new { Good = "Позиция удалена" });
            }
        }

        public JsonResult Move(string Select, int FolderId)
        {
            var Storages = Select.Replace("storage", "").Split(new [] { ";;" }, StringSplitOptions.RemoveEmptyEntries);

            using (var db = new DevinContext())
            {
                string folder = db.Folders
                    .Where(x => x.Id == FolderId)
                    .Select(x => x.Name)
                    .FirstOrDefault();

                string message = folder == null 
                    ? ("в папку \"" + folder + "\" [folder" + FolderId + "]") 
                    : "отдельно";

                foreach (string storage in Storages)
                {
                    int id = int.TryParse(storage, out int i) ? i : 0;

                    db.Storages
                        .Where(x => x.Id == id)
                        .Set(x => x.FolderId, FolderId)
                        .Update();
                    
                    db.Log(User, "storages", id, "Позиция перемещена " + message);
                }

                return Json(new { Good = "Выбранные позиции перемещены " + message });
            }
        }

        public JsonResult Labels(string Select)
        {
            var selectedIdentifiers = Select.Replace("storage", "").Split(',').Select(x => int.TryParse(x, out int i) ? i : 0).ToList();

            using (var db = new DevinContext())
            {
                var query = from s in db.Storages
                            from c in db.Cartridges.Where(x => x.Id == s.CartridgeId).DefaultIfEmpty(new Cartridge { Name = s.Name })
                            where selectedIdentifiers.Contains(s.Id)
                            orderby s.Date descending
                            select new Storage
                            {
                                Id = s.Id,
                                Nstorage = s.Nstorage,
                                Inventory = s.Inventory,
                                Type = c.Name,
                                Date = s.Date,
                                Name = s.Name
                            };

                var storages = query.ToList();

                if (storages.Count == 0) return Json(new { Warning = "Нечего печатать" });
                if (storages.Select(x => x.Nstorage).Sum() == 0) return Json(new { Warning = "На складе нет выбранных позиций" });

                HSSFWorkbook book;
                using (var fs = new FileStream(Server.MapPath(Url.Action("templates", "content") + "\\storage\\labels.xls"), FileMode.Open, FileAccess.Read))
                {
                    book = new HSSFWorkbook(fs);
                }

                var sheet = book.GetSheetAt(0);

                bool isLeft = true;
                int rowCount = 1;

                for (int i = 0; i < storages.Count; i++)
                {
                    for (int j = 0; j < storages[i].Nstorage; j++)
                    {
                        if (isLeft)
                        {
                            sheet.GetRow(rowCount * 3 - 3).GetCell(0).SetCellValue("№");
                            sheet.GetRow(rowCount * 3 - 2).GetCell(0).SetCellValue("Имя:");
                            sheet.GetRow(rowCount * 3 - 1).GetCell(0).SetCellValue("Приход:");

                            sheet.GetRow(rowCount * 3 - 3).GetCell(1).SetCellValue(storages[i].Inventory);
                            sheet.GetRow(rowCount * 3 - 2).GetCell(1).SetCellValue(storages[i].Name);
                            sheet.GetRow(rowCount * 3 - 1).GetCell(1).SetCellValue(storages[i].Date.ToString("dd.MM.yyyy"));

                            isLeft = false;
                        }
                        else
                        {
                            sheet.GetRow(rowCount * 3 - 3).GetCell(2).SetCellValue("№");
                            sheet.GetRow(rowCount * 3 - 2).GetCell(2).SetCellValue("Имя:");
                            sheet.GetRow(rowCount * 3 - 1).GetCell(2).SetCellValue("Приход:");

                            sheet.GetRow(rowCount * 3 - 3).GetCell(3).SetCellValue(storages[i].Inventory);
                            sheet.GetRow(rowCount * 3 - 2).GetCell(3).SetCellValue(storages[i].Name);
                            sheet.GetRow(rowCount * 3 - 1).GetCell(3).SetCellValue(storages[i].Date.ToString("dd.MM.yyyy"));

                            rowCount++;
                            isLeft = true;
                        }
                    }
                }

                using (var fs = new FileStream(Server.MapPath(Url.Action("excels", "content") + "\\Бирки.xls"), FileMode.OpenOrCreate, FileAccess.Write))
                {
                    book.Write(fs);
                }

                return Json(new { Good = "Создание файла с бирками завершено", Link = Url.Action("excels", "content") + "/Бирки.xls" });
            }
        }

        public JsonResult AnalyzePrint(string Data)
        {
            // Получение исходных данных
            float cost = 0;
            var types = new List<List<Cartridge>>();
            var lastType = new List<Cartridge>();
            string lastName = "";

            var DataSplit = (Data ?? "").Split(new [] { "----" }, StringSplitOptions.RemoveEmptyEntries);
            if (DataSplit.Length < 1) return Json(new { Error = "Не переданы данные" });
            var list = new List<Cartridge>();

            foreach (var data in DataSplit)
            {
                var raw = data.Split(new [] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                if (raw.Length != 5) return Json(new { Error = "В данных по картриджу недостаточно данных: \"" + data + "\"" });

                var cartridge = new Cartridge
                {
                    Name = raw[0],
                    Count = int.Parse(raw[4]),
                    Cost = float.Parse(raw[3])
                };

                switch (raw[2])
                {
                    case "flow": cartridge.Type = "Картридж струйный"; break;
                    case "laser": cartridge.Type = "Тонер-картридж"; break;
                    case "matrix": cartridge.Type = "Матричная лента"; break;
                }

                switch (raw[1])
                { 
                    case "black": cartridge.Color = "черный"; break;
                    case "blue": cartridge.Color = "голубой"; break;
                    case "red": cartridge.Color = "красный"; break;
                    case "yellow": cartridge.Color = "желтый"; break;
                    case "3color": cartridge.Color = "трехцветный"; break;
                    case "5color": cartridge.Color = "многоцветный"; break;
                }

                list.Add(cartridge);
                cost += cartridge.Cost;
            }


            // Открытие шаблона
            HSSFWorkbook book;
            using (var fs = new FileStream(Server.MapPath("../content/templates/") + "storage\\analyze.xls", FileMode.Open, FileAccess.Read))
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

            for (int i = 0; i < list.Count - 1; i++) sheet.CopyRow(17, 18 + i);

            var groups = list
                .GroupBy(x => x.Type)
                .Select(g => new
                {
                    g.Key,
                    Cartridges = g.ToList(),
                })
                .ToList();


            foreach (var group in groups)
            {
                foreach (var cartridge in group.Cartridges)
                {
                    IRow row = sheet.GetRow(endRegion);
                    endRegion++;

                    row.GetCell(2).SetCellValue("шт.");
                    row.GetCell(3).SetCellValue(cartridge.Count);
                    row.GetCell(4).SetCellValue(cartridge.Name + ", " + cartridge.Color);
                    row.GetCell(6).SetCellValue(cartridge.Cost + " BYN за 1 шт.");
                    row.Height = -1;
                }

                sheet.GetRow(startRegion).GetCell(1).SetCellValue(group.Key);
                sheet.AddMergedRegion(new CellRangeAddress(startRegion, endRegion - 1, 1, 1));

                startRegion = endRegion;
            }


            // Сохранение документа
            string name = "Заявка на закупку картриджей " + DateTime.Now.ToString("d MMMM yyyy") + ".xls";
            using (var fs = new FileStream(Server.MapPath("../content/excels/") + name, FileMode.OpenOrCreate, FileAccess.Write))
            {
                book.Write(fs);
            }

            return Json(new { Good = "Создание заявки на закупку завершено", Link = Url.Action("excels", "content") + "/" + name });
        }
    }
}