using Dapper;
using Devin.Forms;
using Devin.Models;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Mvc;

namespace Devin.Controllers
{
    public class StoragesController : Controller
    {
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
                using (var fs = new FileStream(Server.MapPath("/devin/content/exl/") + "labels.xls", FileMode.Open, FileAccess.Read))
                {
                    book = new HSSFWorkbook(fs);
                }
                var sheet = book.GetSheetAt(0);

                bool isLeft = true;
                int rowCount = 1;

                for (int i = 0; i < model.Count; i++)
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
                
                using (var fs = new FileStream(Server.MapPath("/devin/content/excels/") + "Бирки.xls", FileMode.OpenOrCreate, FileAccess.Write))
                {
                    book.Write(fs);
                }

                return "<a href='/devin/content/excels/Бирки.xls'>Сохранить файл</a>"; ;
            }
        }
    }
}