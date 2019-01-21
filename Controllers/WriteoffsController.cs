using Devin.Models;
using LinqToDB;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace Devin.Controllers
{
    public class WriteoffsController : Controller
    {
        public ActionResult Cart(int Id) => View(model: Id);

        public ActionResult History(string Id) => View(model: Id);


        public JsonResult Create()
        {
            using (var db = new DevinContext())
            {
                int id = db.InsertWithInt32Identity(new Writeoff
                {
                    Name = "Новое списание",
                    Date = DateTime.Now,
                    Type = "expl",
                    FolderId = 0,
                    CostArticle = 0
                });

                db.Writeoffs.Where(x => x.Id == id).Set(x => x.Name, x => x.Name + " #" + id).Update();
                db.Log(User, "writeoffs", id, "Создано списание");

                return Json(new { Id = "off" + id, Good = "Создано новое списание" });
            }
        }

        public JsonResult Update(
            [Bind(Include = "Id,Name,Type,Description,CostArticle,FolderId")] Writeoff writeoff, 
            string[] Params, 
            string Date
        )
        {
            writeoff.Params = string.Join(";;", Params);

            if (!DateTime.TryParse(Date, out DateTime d))
            {
                return Json(new { Error = "Дата введена в неверном формате. Ожидается формат \"дд.ММ.гггг чч:мм\"" });
            }
            else
            {
                writeoff.Date = d;
            }

            using (var db = new DevinContext())
            {
                var old = db.Writeoffs.Where(x => x.Id == writeoff.Id).FirstOrDefault();

                var changes = new List<string>();
                if (writeoff.Name != old.Name) changes.Add("наименование [\"" + old.Name + "\" => \"" + writeoff.Name + "\"]");
                if (writeoff.Type != old.Type) changes.Add("тип [\"" + old.Type + "\" => \"" + writeoff.Type + "\"]");
                if ((writeoff.Description ?? "") != (old.Description ?? "")) changes.Add("описание [\"" + old.Description + "\" => \"" + writeoff.Description + "\"]");
                if (writeoff.CostArticle != old.CostArticle) changes.Add("статья расходов [\"" + old.CostArticle + "\" => \"" + writeoff.CostArticle + "\"]");
                if (writeoff.FolderId != old.FolderId) changes.Add("папка [\"" + (old.FolderId == 0 ? "отдельно" : ("folder" + old.FolderId)) + "\" => \"" + (writeoff.FolderId == 0 ? "отдельно" : ("folder" + writeoff.FolderId)) + "\"]");
                if (writeoff.Params != old.Params) changes.Add("параметры экспорта [\"" + old.Params + "\" => \"" + writeoff.Params + "\"]");
                if (writeoff.Date != old.Date) changes.Add("дата создания [\"" + old.Date + "\" => \"" + writeoff.Date + "\"]");

                if (changes.Count > 0)
                {
                    db.Writeoffs
                        .Where(x => x.Id == writeoff.Id)
                        .Set(x => x.Name, writeoff.Name)
                        .Set(x => x.Type, writeoff.Type)
                        .Set(x => x.Description, writeoff.Description)
                        .Set(x => x.CostArticle, writeoff.CostArticle)
                        .Set(x => x.FolderId, writeoff.FolderId)
                        .Set(x => x.Params, writeoff.Params)
                        .Set(x => x.Date, writeoff.Date)
                        .Update();

                    db.Log(User, "writeoffs", writeoff.Id, "Списание обновлено. Изменения: " + changes.ToLog());

                    return Json(new { Good = "Списание обновлено. Изменения:<br />" + changes.ToHtml() });
                }
                else
                {
                    return Json(new { Warning = "Изменений не было" });
                }
            }
        }

        public JsonResult Move(int Id, int FolderId)
        {
            using (var db = new DevinContext())
            {
                string folder = db.Folders.Where(x => x.Id == FolderId).Select(x => x.Name).FirstOrDefault() ?? "<не определено>";

                db.Writeoffs
                    .Where(x => x.Id == Id)
                    .Set(x => x.FolderId, FolderId)
                    .Update();
                db.Log(User, "writeoffs", Id, "Списание перемещено в папку \"" + folder + "\" [folder" + FolderId + "]");

                return Json(new { Good = "Списание перемещено" });
            }
        }

        public JsonResult Delete(string Id)
        {
            int id = int.TryParse(Id.Replace("off", ""), out int i) ? i : 0;

            using (var db = new DevinContext())
            {
                db.Writeoffs.Delete(x => x.Id == id);
                db.Log(User, "writeoffs", id, "Списание удалено без отмены вложенных ремонтов");
            }

            return Json(new { Good = "Списание удалено без отмены вложенных ремонтов" });
        }

        public JsonResult Print(int Id)
        {
            var months = new [] { "январе", "феврале", "марте", "апреле", "мае", "июне", "июле", "августе", "сентябре", "октябре", "ноябре", "декабре" };

            using (var db = new DevinContext())
            {
                var query = from w in db.Writeoffs
                            from t in db._WriteoffTypes.Where(x => x.Id == w.Type).DefaultIfEmpty()
                            where w.Id == Id
                            select new
                            {
                                w.Name,
                                w.Date,
                                w.Params,
                                w.Type,
                                w.CostArticle,
                                t.Template,
                                t.DefaultData
                            };

                var writeoff = query.FirstOrDefault();

                string template = Server.MapPath(Url.Action("exl", "content")) + "\\" + writeoff.Type + ".xls";
                string output = writeoff.Name + " " + DateTime.Now.ToLongDateString() + ".xls";

                if (!System.IO.File.Exists(template)) return Json(new { Error = "Файла шаблона не существует либо путь к нему неправильно прописан в исходниках<br />Путь: " + template });

                IWorkbook book;
                ISheet sheet;

                int step = 0;

                if (string.IsNullOrEmpty(writeoff.Params)) return Json(new { Error = "В списании не были заданы параметры" });

                var props = writeoff.Params.Split(new [] { ";;" }, StringSplitOptions.None);

                using (var fs = new FileStream(template, FileMode.Open, FileAccess.Read))
                {
                    book = new HSSFWorkbook(fs);
                }

                IEnumerable<Repair> repairs;
                float sum = 0;

                /* Эксплуатационные расходы */
                if (writeoff.Type == "expl")
                {

                    if (props.Length != 2) return Json(new { Error = "Недостаточное количество параметров для экспорта. Получено " + props.Length + ", ожидается 2." });

                    sheet = book.GetSheetAt(0);
                    sheet.GetRow(13).GetCell(0).SetCellValue("      Комиссия,  назначенная приказом №108 от 28.08.2012г. произвела подсчет и списание товарно-материальных ценностей, израсходованных в " + months[writeoff.Date.Month - 1] + " " + writeoff.Date.Year + " г. Были использованы  следующие материалы:");

                    try
                    {
                        var repairsQuery = from r in db.Repairs
                                           from d in db.Devices.Where(x => x.Id == r.DeviceId).DefaultIfEmpty()
                                           from s in db.Storages.Where(x => x.Id == r.StorageId).DefaultIfEmpty()
                                           where r.WriteoffId == Id
                                           select new Repair
                                           {
                                               Id = r.Id,
                                               Number = r.Number,
                                               Device = new Device
                                               {
                                                   Id = d.Id,
                                                   Inventory = d.Inventory
                                               },
                                               Storage = new Storage
                                               {
                                                   Id = s.Id,
                                                   Inventory = s.Inventory,
                                                   Name = s.Name,
                                                   Cost = s.Cost,
                                                   Account = s.Account
                                               }
                                           };

                        repairs = repairsQuery.ToList();
                    }
                    catch (Exception)
                    {
                        return Json(new { Error = "Хуйня с запросом" });
                    }

                    foreach (Repair r in repairs)
                    {
                        if (r.Device.Inventory.Contains("***") || r.Device.Inventory.Contains("xxx"))
                        {
                            r.Device.Inventory = "Эксплуатационные нужды";
                        }
                        else
                        {
                            r.Device.Inventory = "Установлен в: инв. № " + r.Device.Inventory;
                        }

                        IRow row = sheet.CopyRow(20, 21 + step);
                        row.GetCell(0).SetCellValue(step + 1);
                        row.GetCell(1).SetCellValue(r.Storage.Inventory);
                        row.GetCell(2).SetCellValue(r.Storage.Name + "; счет " + r.Storage.Account);
                        row.GetCell(3).SetCellValue("шт.");
                        row.GetCell(4).SetCellValue(r.Number);
                        row.GetCell(5).SetCellValue(r.Storage.Cost.ToString("0.00"));
                        row.GetCell(6).SetCellValue((r.Storage.Cost * r.Number).ToString("0.00"));
                        row.GetCell(7).SetCellValue(r.Device.Inventory);
                        row.Height = -1;

                        step++;
                        sum += r.Storage.Cost * r.Number;
                    }

                    sheet.GetRow(20).ZeroHeight = true;

                    sheet.GetRow(21 + step).GetCell(6).SetCellValue(sum);

                    sheet.GetRow(40 + step).GetCell(0).SetCellValue("Акт составлен " + writeoff.Date.ToString("d MMMM yyyy"));
                    sheet.GetRow(42 + step).GetCell(3).SetCellValue(props[0]);
                    sheet.GetRow(42 + step).GetCell(6).SetCellValue(props[1]);

                }

                /* Эксплуатационные расходы (прочие) */
                if (writeoff.Type == "expl-1") {

                    if (props.Length != 2) return Json(new { Error = "Недостаточное количество параметров для экспорта. Получено " + props.Length + ", ожидается 2." });

                    sheet = book.GetSheetAt(0);
                    sheet.GetRow(13).GetCell(0).SetCellValue("      Комиссия,  назначенная приказом №108 от 28.08.2012г. произвела подсчет и списание товарно-материальных ценностей, израсходованных в " + months[writeoff.Date.Month - 1] + " " + writeoff.Date.Year + " г. Были использованы  следующие материалы:");

                    try
                    {
                        var repairsQuery = from r in db.Repairs
                                           from d in db.Devices.Where(x => x.Id == r.DeviceId).DefaultIfEmpty()
                                           from s in db.Storages.Where(x => x.Id == r.StorageId).DefaultIfEmpty()
                                           where r.WriteoffId == Id
                                           select new Repair
                                           {
                                               Id = r.Id,
                                               Number = r.Number,
                                               Device = new Device
                                               {
                                                   Id = d.Id,
                                                   Inventory = d.Inventory
                                               },
                                               Storage = new Storage
                                               {
                                                   Id = s.Id,
                                                   Inventory = s.Inventory,
                                                   Name = s.Name,
                                                   Cost = s.Cost,
                                                   Account = s.Account
                                               }
                                           };

                        repairs = repairsQuery.ToList();
                    }
                    catch (Exception)
                    {
                        return Json(new { Error = "Хуйня с запросом" });
                    }

                    foreach (Repair r in repairs)
                    {
                        string s = "";
                        if (r.Device.Inventory.Contains("***") || r.Device.Inventory.Contains("xxx"))
                        {
                            s = "Эксплуатационные нужды";
                        }
                        else
                        {
                            s = "Установлен в: инв. № " + r.Device.Inventory;
                        }

                        IRow row = sheet.CopyRow(20, 21 + step);
                        row.GetCell(0).SetCellValue(step + 1);
                        row.GetCell(1).SetCellValue(r.Storage.Inventory);
                        row.GetCell(2).SetCellValue(r.Storage.Name + "; счет " + r.Storage.Account);
                        row.GetCell(3).SetCellValue("шт.");
                        row.GetCell(4).SetCellValue(r.Number);
                        row.GetCell(5).SetCellValue(r.Storage.Cost.ToString("0.00"));
                        row.GetCell(6).SetCellValue((r.Storage.Cost * r.Number).ToString("0.00"));
                        row.GetCell(7).SetCellValue(s);
                        row.Height = -1;

                        step++;
                        sum += r.Storage.Cost * r.Number;
                    }

                    sheet.GetRow(20).ZeroHeight = true;

                    sheet.GetRow(21 + step).GetCell(6).SetCellValue(sum);

                    sheet.GetRow(44 + step).GetCell(0).SetCellValue("Акт составлен " + writeoff.Date.ToString("d MMMM yyyy"));
                    sheet.GetRow(46 + step).GetCell(3).SetCellValue(props[0]);
                    sheet.GetRow(46 + step).GetCell(6).SetCellValue(props[1]);



                }

                /* Ремонт основного средства (ПТК АСУ) */
                if (writeoff.Type == "mat")
                {
                    sheet = book.GetSheetAt(7);
                    sheet.GetRow(25).GetCell(16).SetCellValue(Id);
                    sheet.GetRow(26).GetCell(16).SetCellValue(writeoff.Date.ToString("d MMMM yyyy") + " г.");
                    sheet.GetRow(27).GetCell(16).SetCellValue(writeoff.Date.ToString("dd.MM.yyyy") + " г.");
                    sheet.GetRow(28).GetCell(16).SetCellValue(writeoff.Date.ToString("MMMM yyyy") + " г.");
                    sheet.GetRow(27).GetCell(39).SetCellValue(writeoff.Date.ToString("yyyy") + " г.");

                    var repairsQuery = from raw in db.Repairs
                                           .Where(x => x.WriteoffId == Id)
                                           .Select(x => new { x.Id, x.DeviceId })
                                           .ToList()
                                       group raw by raw.DeviceId into g
                                       select new
                                       {
                                           DeviceNumber = g.Key,
                                           RepairsCount = g.Count()
                                       };

                    var rs = repairsQuery.FirstOrDefault();

                    if (rs == null) return Json(new { Error = "В ремонтах не найден идентификатор основного средства, либо списание не содержит ремонтов" });

                    int DeviceId = rs.DeviceNumber;
                    int RepairsCount = rs.RepairsCount;

                    var device = db.Devices.Where(x => x.Id == DeviceId).FirstOrDefault();
                    if (device == null) return Json(new { Error = "Устройство не найдено" });

                    var metals = db.Objects1C
                        .Where(x => x.Inventory == device.Inventory)
                        .Select(x => new Object1C {
                            Description = x.Description,
                            Gold = x.Gold,
                            Silver = x.Silver,
                            Platinum = x.Platinum,
                            Palladium = x.Palladium,
                            Mpg = x.Mpg,
                            SubDivision = x.SubDivision
                        })
                        .FirstOrDefault() ?? new Object1C();

                    if ((device.Gold ?? "") == "") device.Gold = metals.Gold.ToString();
                    if ((device.Silver ?? "") == "") device.Silver = metals.Silver.ToString();
                    if ((device.Platinum ?? "") == "") device.Platinum = metals.Platinum.ToString();
                    if ((device.Palladium ?? "") == "") device.Palladium = metals.Palladium.ToString();
                    if ((device.MPG ?? "") == "") device.MPG = metals.Mpg.ToString();
                    if ((device.Mol ?? "") == "") device.Mol = metals.SubDivision.Trim() ?? "МОЛ не найден в 1С.";

                    string valueText = "";
                    switch (device.Inventory)
                    {
                        case "075755": valueText = "Оборудование ПТК АСУ: "; break;
                        case "075750": valueText = "Оборудование корпоративной сети: "; break;
                        case "075155": valueText = "Оборудование АСКУЭ ММПГ: "; break;
                    }

                    if (!string.IsNullOrEmpty(metals.Description))
                    {
                        sheet.GetRow(29).GetCell(16).SetCellValue(valueText + device.Description);
                    }
                    else
                    {
                        sheet.GetRow(29).GetCell(16).SetCellValue(valueText + metals.Description);
                    }
                    sheet.GetRow(31).GetCell(16).SetCellValue(device.Inventory);
                    sheet.GetRow(32).GetCell(16).SetCellValue(device.SerialNumber);
                    sheet.GetRow(33).GetCell(16).SetCellValue(RepairsCount);
                    sheet.GetRow(52).GetCell(39).SetCellValue(device.Mol);

                    switch (writeoff.CostArticle)
                    {
                        case 3: book.GetSheetAt(2).GetRow(14).GetCell(2).SetCellValue("ПТК АСУ"); break;
                        case 2: book.GetSheetAt(2).GetRow(14).GetCell(2).SetCellValue("Орг. техника"); break;
                        case 1: book.GetSheetAt(2).GetRow(14).GetCell(2).SetCellValue("Эксплуатационные расходы"); break;
                    }

                    var storagesQuery = from r in db.Repairs
                                        from s in db.Storages.Where(x => x.Id == r.StorageId).DefaultIfEmpty()
                                        where r.WriteoffId == Id
                                        select new Storage
                                        {
                                            Name = s.Name,
                                            Inventory = s.Inventory,
                                            Cost = s.Cost,
                                            Nall = r.Number
                                        };

                    var storages = storagesQuery.ToList();

                    foreach (var storage in storages)
                    {
                        sheet.GetRow(122 + step).GetCell(25).SetCellValue(storage.Nall);
                        sheet.GetRow(122 + step).GetCell(28).SetCellValue("шт.");
                        sheet.GetRow(122 + step).GetCell(32).SetCellValue(storage.Name);
                        sheet.GetRow(122 + step).GetCell(51).SetCellValue("текущий ремонт");
                        sheet.GetRow(122 + step).GetCell(57).SetCellValue(storage.Cost.ToString("0.00"));
                        sheet.GetRow(122 + step).GetCell(63).SetCellValue(storage.Inventory);
                        step++;
                    }

                    dynamic drags;

                    using (var _db = new SiteContext())
                    {
                        drags = new
                        {
                            Gold = _db.Constants.Where(x => x.Keyword == "Gold").Select(x => x.Value).FirstOrDefault(),
                            Silver = _db.Constants.Where(x => x.Keyword == "Silver").Select(x => x.Value).FirstOrDefault(),
                            MPG = _db.Constants.Where(x => x.Keyword == "MPG").Select(x => x.Value).FirstOrDefault()
                        };
                    }

                    sheet.GetRow(10).GetCell(16).SetCellValue(drags.Gold.Replace('.', ','));
                    sheet.GetRow(11).GetCell(16).SetCellValue(drags.Silver.Replace('.', ','));
                    sheet.GetRow(12).GetCell(16).SetCellValue(drags.MPG.Replace('.', ','));

                    if (props.Length > 4)
                    {
                        sheet.GetRow(50).GetCell(27).SetCellValue(props[0] + " " + props[1]);
                        sheet.GetRow(52).GetCell(27).SetCellValue(props[1] + " " + props[0]);
                        sheet.GetRow(52).GetCell(16).SetCellValue(props[2]);
                        sheet.GetRow(34).GetCell(16).SetCellValue(props[3]);
                        sheet.GetRow(33).GetCell(16).SetCellValue(props[4]);
                        sheet.GetRow(27).GetCell(48).SetCellValue(props[5]);
                    }

                    sheet.GetRow(63).GetCell(59).SetCellValue(float.TryParse(device.Gold, out float v)
                        ? v.ToString("0.000000").Replace('.', ',')
                        : "0,000000");
                    sheet.GetRow(64).GetCell(59).SetCellValue(float.TryParse(device.Silver, out v)
                        ? v.ToString("0.000000").Replace('.', ',')
                        : "0,000000");
                    sheet.GetRow(65).GetCell(59).SetCellValue(float.TryParse(device.Platinum, out v)
                        ? v.ToString("0.000000").Replace('.', ',')
                        : "0,000000");

                    var palladium = float.TryParse(device.Palladium, out v) ? v : 0;
                    var mpg = float.TryParse(device.MPG, out v) ? v : 0;

                    sheet.GetRow(66).GetCell(59).SetCellValue((mpg + palladium).ToString("0.000000").Replace('.', ','));

                    sheet.ForceFormulaRecalculation = true;
                }

                /* Ремонт основного средства (оргтехника) */
                if (writeoff.Type == "mat-org")
                {
                    sheet = book.GetSheetAt(7);
                    sheet.GetRow(25).GetCell(16).SetCellValue(Id);
                    sheet.GetRow(26).GetCell(16).SetCellValue(writeoff.Date.ToString("d MMMM yyyy") + " г.");
                    sheet.GetRow(27).GetCell(16).SetCellValue(writeoff.Date.ToString("dd.MM.yyyy") + " г.");
                    sheet.GetRow(28).GetCell(16).SetCellValue(writeoff.Date.ToString("MMMM yyyy") + " г.");
                    sheet.GetRow(27).GetCell(39).SetCellValue(writeoff.Date.ToString("yyyy") + " г.");

                    var repairsQuery = from raw in db.Repairs
                                           .Where(x => x.WriteoffId == Id)
                                           .Select(x => new { x.Id, x.DeviceId })
                                           .ToList()
                                       group raw by raw.DeviceId into g
                                       select new
                                       {
                                           DeviceNumber = g.Key,
                                           RepairsCount = g.Count()
                                       };

                    var rs = repairsQuery.FirstOrDefault();

                    if (rs == null) return Json(new { Error = "В ремонтах не найден идентификатор основного средства, либо списание не содержит ремонтов" });

                    int DeviceId = rs.DeviceNumber;
                    int RepairsCount = rs.RepairsCount;

                    var device = db.Devices.Where(x => x.Id == DeviceId).FirstOrDefault();
                    if (device == null) return Json(new { Error = "Устройство не найдено" });

                    var metals = db.Objects1C
                        .Where(x => x.Inventory == device.Inventory)
                        .Select(x => new Object1C
                        {
                            Description = x.Description,
                            Gold = x.Gold,
                            Silver = x.Silver,
                            Platinum = x.Platinum,
                            Palladium = x.Palladium,
                            Mpg = x.Mpg,
                            SubDivision = x.SubDivision
                        })
                        .FirstOrDefault() ?? new Object1C();

                    if ((device.Gold ?? "") == "") device.Gold = metals.Gold.ToString();
                    if ((device.Silver ?? "") == "") device.Silver = metals.Silver.ToString();
                    if ((device.Platinum ?? "") == "") device.Platinum = metals.Platinum.ToString();
                    if ((device.Palladium ?? "") == "") device.Palladium = metals.Palladium.ToString();
                    if ((device.MPG ?? "") == "") device.MPG = metals.Mpg.ToString();
                    if ((device.Mol ?? "") == "") device.Mol = metals.SubDivision.Trim() ?? "МОЛ не найден в 1С.";

                    string valueText = "";
                    switch (device.Inventory)
                    {
                        case "075755": valueText = "Оборудование ПТК АСУ: "; break;
                        case "075750": valueText = "Оборудование корпоративной сети: "; break;
                        case "075155": valueText = "Оборудование АСКУЭ ММПГ: "; break;
                    }

                    if (!string.IsNullOrEmpty(metals.Description))
                    {
                        sheet.GetRow(29).GetCell(16).SetCellValue(valueText + device.Description);
                    }
                    else
                    {
                        sheet.GetRow(29).GetCell(16).SetCellValue(valueText + metals.Description);
                    }
                    sheet.GetRow(31).GetCell(16).SetCellValue(device.Inventory);
                    sheet.GetRow(32).GetCell(16).SetCellValue(device.SerialNumber);
                    sheet.GetRow(33).GetCell(16).SetCellValue(RepairsCount);
                    sheet.GetRow(52).GetCell(39).SetCellValue(device.Mol);

                    switch (writeoff.CostArticle)
                    {
                        case 3: book.GetSheetAt(2).GetRow(14).GetCell(2).SetCellValue("ПТК АСУ"); break;
                        case 2: book.GetSheetAt(2).GetRow(14).GetCell(2).SetCellValue("Орг. техника"); break;
                        default: book.GetSheetAt(2).GetRow(14).GetCell(2).SetCellValue("Эксплуатационные расходы"); break;
                    }

                    var storagesQuery = from r in db.Repairs
                                        from s in db.Storages.Where(x => x.Id == r.StorageId).DefaultIfEmpty()
                                        where r.WriteoffId == Id
                                        select new Storage
                                        {
                                            Name = s.Name,
                                            Inventory = s.Inventory,
                                            Cost = s.Cost,
                                            Nall = r.Number
                                        };

                    var storages = storagesQuery.ToList();

                    foreach (var storage in storages)
                    {
                        sheet.GetRow(122 + step).GetCell(25).SetCellValue(storage.Nall);
                        sheet.GetRow(122 + step).GetCell(28).SetCellValue("шт.");
                        sheet.GetRow(122 + step).GetCell(32).SetCellValue(storage.Name);
                        sheet.GetRow(122 + step).GetCell(51).SetCellValue("текущий ремонт");
                        sheet.GetRow(122 + step).GetCell(57).SetCellValue(storage.Cost.ToString("0.00"));
                        sheet.GetRow(122 + step).GetCell(63).SetCellValue(storage.Inventory);
                        step++;
                    }

                    dynamic drags;

                    using (var _db = new SiteContext())
                    {
                        drags = new
                        {
                            Gold = _db.Constants.Where(x => x.Keyword == "Gold").Select(x => x.Value).FirstOrDefault(),
                            Silver = _db.Constants.Where(x => x.Keyword == "Silver").Select(x => x.Value).FirstOrDefault(),
                            MPG = _db.Constants.Where(x => x.Keyword == "MPG").Select(x => x.Value).FirstOrDefault()
                        };
                    }

                    sheet.GetRow(10).GetCell(16).SetCellValue(drags.Gold.Replace('.', ','));
                    sheet.GetRow(11).GetCell(16).SetCellValue(drags.Silver.Replace('.', ','));
                    sheet.GetRow(12).GetCell(16).SetCellValue(drags.MPG.Replace('.', ','));

                    if (props.Length > 4)
                    {
                        sheet.GetRow(50).GetCell(27).SetCellValue(props[0] + " " + props[1]);
                        sheet.GetRow(52).GetCell(27).SetCellValue(props[1] + " " + props[0]);
                        sheet.GetRow(52).GetCell(16).SetCellValue(props[2]);
                        sheet.GetRow(34).GetCell(16).SetCellValue(props[3]);
                        sheet.GetRow(33).GetCell(16).SetCellValue(props[4]);
                        sheet.GetRow(27).GetCell(48).SetCellValue(props[5]);
                    }

                    sheet.GetRow(63).GetCell(59).SetCellValue(float.TryParse(device.Gold, out float v)
                        ? v.ToString("0.000000").Replace('.', ',')
                        : "0,000000");
                    sheet.GetRow(64).GetCell(59).SetCellValue(float.TryParse(device.Silver, out v)
                        ? v.ToString("0.000000").Replace('.', ',')
                        : "0,000000");
                    sheet.GetRow(65).GetCell(59).SetCellValue(float.TryParse(device.Platinum, out v)
                        ? v.ToString("0.000000").Replace('.', ',')
                        : "0,000000");

                    var palladium = float.TryParse(device.Palladium, out v) ? v : 0;
                    var mpg = float.TryParse(device.MPG, out v) ? v : 0;

                    sheet.GetRow(66).GetCell(59).SetCellValue((mpg + palladium).ToString("0.000000").Replace('.', ','));
                }

                output = output.Replace("\"", "");

                using (var fs = new FileStream(Server.MapPath(Url.Action("excels", "content")) + "\\" + output, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    book.Write(fs);
                }

                return Json(new { Good = "Файл Excel списания успешно создан", Link = Url.Action("excels", "content") + "/" + output });
            }
        }
    }
}