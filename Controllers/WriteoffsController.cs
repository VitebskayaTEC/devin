using Dapper;
using Devin.Models;
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
        public string Print(int Id)
        {
            string[] months = new string[] { "январе", "феврале", "марте", "апреле", "мае", "июне", "июле", "августе", "сентябре", "октябре", "ноябре", "декабре" };

            using (var conn = Database.Connection())
            {
                var writeoff = conn.QueryFirst<Writeoff>(@"SELECT 
                    Writeoffs.*,
                    c.O_Excel        AS [DefExcel],
                    c.O_Template     AS [Template],
                    w.CostArticle
                FROM Writeoffs
                LEFT OUTER JOIN catalog_writeoffs c ON Writeoffs.Type = c.O_Alias 
                WHERE (Writeoffs.Id = @Id)", new { Id });

                string template = Server.MapPath("/devin/content/exl/") + writeoff.Type + ".xls";
                string output = writeoff.Name + " " + DateTime.Now.ToLongDateString() + ".xls";

                if (!System.IO.File.Exists(template)) return "Файла шаблона не существует либо путь к нему неправильно прописан в исходниках";

                IWorkbook book;
                ISheet sheet;
                IEnumerable<Repair> repairs;
                int step = 0;

                if (string.IsNullOrEmpty(writeoff.Params)) return "В списании не были заданы параметры";

                string[] props = writeoff.Params.Split(new string[] { ";;" }, StringSplitOptions.None);

                using (var fs = new FileStream(template, FileMode.Open, FileAccess.Read))
                {
                    book = new HSSFWorkbook(fs);
                }

                switch (writeoff.Type)
                {
                    /* Эксплуатационные расходы */
                    case "expl":

                        if (props.Length != 2) return "Недостаточное количество параметров для экспорта. Получено " + props.Length + ", ожидается 2.";

                        sheet = book.GetSheetAt(0);
                        sheet.GetRow(13).GetCell(0).SetCellValue("      Комиссия,  назначенная приказом №108 от 28.08.2012г. произвела подсчет и списание товарно-материальных ценностей, израсходованных в " + months[writeoff.Date.Month - 1] + " " + writeoff.Date.Year + " г. Были использованы  следующие материалы:");

                        try
                        {
                            repairs = conn.Query<Repair>(@"SELECT 
                                Storages.Inventory AS [StorageInventory],
                                Storages.Name      AS [StorageName],
                                Repairs.Number     AS [StorageCount],
                                Storages.Cost      AS [StoragePrice],
                                Devices.Inventory  AS [DeviceInventory],
                                Storages.Account   AS [StorageList]
                            FROM Repairs 
                            LEFT OUTER JOIN Storages  ON Repairs.StorageInventory = Storages.Inventory 
                            LEFT OUTER JOIN DEVICE    ON Repairs.DeviceId         = Devices.Id 
                            WHERE (Repairs.WriteoffId = @Id)", new { Id });
                        }
                        catch (Exception)
                        {
                            return "Хуйня с запросом";
                        }
                        
                        float sum = 0;

                        foreach (Repair r in repairs)
                        {
                            if (r.DeviceInventory.Contains("***") || r.DeviceInventory.Contains("xxx"))
                            {
                                r.DeviceInventory = "Эксплуатационные нужды";
                            }
                            else
                            {
                                r.DeviceInventory = "Установлен в: инв. № " + r.DeviceInventory;
                            }

                            IRow row = sheet.CopyRow(20, 21 + step);
                            row.GetCell(0).SetCellValue(step + 1);
                            row.GetCell(1).SetCellValue(r.StorageInventory);
                            row.GetCell(2).SetCellValue(r.StorageName + "; счет " + r.StorageList);
                            row.GetCell(3).SetCellValue("шт.");
                            row.GetCell(4).SetCellValue(r.StorageCount);
                            row.GetCell(5).SetCellValue(r.StoragePrice.ToString("0.00"));
                            row.GetCell(6).SetCellValue((r.StoragePrice * r.StorageCount).ToString("0.00"));
                            row.GetCell(7).SetCellValue(r.DeviceInventory);
                            row.Height = -1;

                            step++;
                            sum += r.StoragePrice * r.StorageCount;
                        }

                        sheet.GetRow(20).ZeroHeight = true;

                        sheet.GetRow(21 + step).GetCell(6).SetCellValue(sum);

                        sheet.GetRow(40 + step).GetCell(0).SetCellValue("Акт составлен " + writeoff.Date.ToString("d MMMM yyyy"));
                        sheet.GetRow(42 + step).GetCell(3).SetCellValue(props[0]);
                        sheet.GetRow(42 + step).GetCell(6).SetCellValue(props[1]);

                        

                        break;


                    /* Ремонт основного средства */
                    case "mat":
                        sheet = book.GetSheetAt(7);
                        sheet.GetRow(25).GetCell(16).SetCellValue(Id);
                        sheet.GetRow(26).GetCell(16).SetCellValue(writeoff.Date.ToString("d MMMM yyyy") + " г.");
                        sheet.GetRow(27).GetCell(16).SetCellValue(writeoff.Date.ToString("dd.MM.yyyy") + " г.");
                        sheet.GetRow(28).GetCell(16).SetCellValue(writeoff.Date.ToString("MMMM yyyy") + " г.");
                        sheet.GetRow(27).GetCell(39).SetCellValue(writeoff.Date.ToString("yyyy") + " г.");

                        var rs = conn.QueryFirst(@"SELECT 
                            TOP (1) DeviceId AS [DeviceNumber], 
                            COUNT(Id) AS [RepairsCount] 
                        FROM Repairs WHERE (WriteoffId = @Id) GROUP BY DeviceId", new { Id });

                        if (rs == null) return "В ремонтах не найден идентификатор основного средства, либо списание не содержит ремонтов";

                        int DeviceId = (int)rs.DeviceNumber;
                        int RepairsCount = (int)rs.RepairsCount;

                        Device device = conn.QueryFirst<Device>(@"SELECT * FROM Devices WHERE (Id = @Id)", new { DeviceId });
                        if (device == null) return "Устройство не найдено";

                        var metals = conn.Query<Device1C>("SELECT Description, Gold, Silver, Platinum, Palladium, Mpg, SubDivision FROM Devices1C WHERE Inventory = @Inventory", new { device.Inventory }).FirstOrDefault();

                        if (metals == null) metals = new Device1C();

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

                        repairs = conn.Query<Repair>(@"SELECT 
                            Repairs.Number     AS [StorageCount], 
                            Storages.Name      AS [StorageName], 
                            Storages.Cost      AS [StoragePrice],
                            Storages.Inventory AS [StorageInventory]
                        FROM Repairs 
                        LEFT OUTER JOIN Storages ON Repairs.StorageInventory = Storages.Inventory 
                        WHERE (Repairs.WriteoffId = @Id)", new { Id });

                        
                        foreach (Repair r in repairs)
                        {
                            sheet.GetRow(122 + step).GetCell(25).SetCellValue(r.StorageCount);
                            sheet.GetRow(122 + step).GetCell(28).SetCellValue("шт.");
                            sheet.GetRow(122 + step).GetCell(32).SetCellValue(r.StorageName);
                            sheet.GetRow(122 + step).GetCell(51).SetCellValue("текущий ремонт");
                            sheet.GetRow(122 + step).GetCell(57).SetCellValue(r.StoragePrice.ToString("0.00"));
                            sheet.GetRow(122 + step).GetCell(63).SetCellValue(r.StorageInventory);
                            step++;
                        }

                        DragMetal drags;

                        using (var c = Database.Connection("Site"))
                        {
                            drags = new DragMetal
                            {
                                Gold = c.QueryFirst<string>("SELECT Value FROM Constants WHERE Keyword = 'Gold'"),
                                Silver = c.QueryFirst<string>("SELECT Value FROM Constants WHERE Keyword = 'Silver'"),
                                MPG = c.QueryFirst<string>("SELECT Value FROM Constants WHERE Keyword = 'MPG'")
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

                        break;
                }

                output = output.Replace("\"", "");

                using (var fs = new FileStream(Server.MapPath("/devin/content/excels/") + output, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    book.Write(fs);
                }

                return "<a href='/devin/content/excels/" + output + "'>" + output + "</a>";
            }
        }

        public ActionResult Cart(int Id) => View(model: Id);

        public JsonResult Create()
        {
            using (var conn = Database.Connection())
            {
                conn.Execute("INSERT INTO Writeoffs (Name, Date, Type, FolderId, CostArticle) VALUES ('Новое списание', GetDate(), 'expl', 0, 0)");
                int Id = conn.QueryFirst<int>("SELECT Max(Id) FROM Writeoffs");
                conn.Execute("UPDATE Writeoffs SET Name = Name + @Text WHERE Id = @Id", new { Id, Text = " #" + Id });
                conn.Execute("INSERT INTO Activity (Date, Source, Id, Text, Username) VALUES (GetDate(), 'writeoffs', @Id, @Text, @Username)", new Activity
                {
                    Id = Id.ToString(),
                    Text = "Создано списание",
                    Username = User.Identity.Name
                });

                return Json(new { Id, Good = "Создано новое списание" });
            }
        }

        public JsonResult Update(int Id, [Bind(Include = "Name,Type,Description,CostArticle,FolderId")] Writeoff writeoff, string[] Params, string Date)
        {
            writeoff.Params = string.Join(";;", Params);
            if (!DateTime.TryParse(Date, out DateTime d))
            {
                return Json(new { Error = "Дата введена в неверном формате. Ожидается формат \"дд.ММ.гггг чч:мм\"" });
            }
            else writeoff.Date = d;

            using (var conn = Database.Connection())
            {
                var old = conn.QueryFirst<Writeoff>("SELECT * FROM Writeoffs WHERE Id = @Id", new { Id });

                List<string> changes = new List<string>();
                if (writeoff.Name != old.Name) changes.Add("наименование [\"" + old.Name + "\" => \"" + writeoff.Name + "\"]");
                if (writeoff.Type != old.Type) changes.Add("тип [\"" + old.Type + "\" => \"" + writeoff.Type + "\"]");
                if (writeoff.Description != old.Description) changes.Add("описание [\"" + old.Description + "\" => \"" + writeoff.Description + "\"]");
                if (writeoff.CostArticle != old.CostArticle) changes.Add("статья расходов [\"" + old.CostArticle + "\" => \"" + writeoff.CostArticle + "\"]");
                if (writeoff.FolderId != old.FolderId) changes.Add("папка [\"" + old.FolderId + "\" => \"" + writeoff.FolderId + "\"]");
                if (writeoff.Params != old.Params) changes.Add("параметры экспорта [\"" + old.Params + "\" => \"" + writeoff.Params + "\"]");
                if (writeoff.Date != old.Date) changes.Add("дата создания [\"" + old.Date + "\" => \"" + writeoff.Date + "\"]");

                if (changes.Count > 0)
                {
                    // Сохранение в базе
                    conn.Execute(@"UPDATE Writeoffs SET
                        Name        = @Name,
                        Type        = @Type,
                        Description = @Description,
                        CostArticle = @CostArticle,
                        FolderId    = @FolderId,
                        Params      = @Params,
                        Date        = @Date
                    WHERE Id = @Id", writeoff);

                    conn.Execute("INSERT INTO Activity (Date, Source, Id, Text, Username) VALUES (GetDate(), 'writeoffs', @Id, @Text, @Username)", new Activity
                    {
                        Id = Id.ToString(),
                        Text = "Списание обновлено. Изменения: " + string.Join(",\n", changes.ToArray()),
                        Username = User.Identity.Name
                    });

                    return Json(new { Good = "Списание обновлено!<br />Изменены поля:<br />" + string.Join(",<br />", changes.ToArray()) });
                }
                else
                {
                    return Json(new { Warning = "Изменений не было" });
                }
            }
        }

        public JsonResult Move(string Id, string FolderId)
        {
            int id = int.TryParse(Id.Replace("off", ""), out int i) ? i : 0;
            int folderId = int.TryParse(Id.Replace("rg", ""), out i) ? i : 0;
            
            using (var conn = Database.Connection())
            {
                conn.Execute("UPDATE Writeoffs SET FolderId = @FolderId WHERE Id = @Id", new
                {
                    Id = id,
                    FolderId = folderId
                });
            }

            return Json(new { Good = "Списание перемещено" });
        }

        public JsonResult Delete(string Id)
        {
            int id = int.TryParse(Id.Replace("off", ""), out int i) ? i : 0;

            using (var conn = Database.Connection())
            {
                conn.Execute("DELETE FROM Writeoffs WHERE Id = @Id", new { Id = id });
                conn.Execute("INSERT INTO Activity (Date, Source, Id, Text, Username) VALUES (GetDate(), 'writeoffs', @Id, @Text, @Username)", new Activity
                {
                    Id = id.ToString(),
                    Text = "Списание удалено без отмены вложенных ремонтов",
                    Username = User.Identity.Name
                });
            }

            return Json(new { Good = "Списание удалено без отмены вложенных ремонтов" });
        }
    }
}