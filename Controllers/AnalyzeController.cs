using Devin.Models;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Mvc;

namespace Devin.Controllers
{
    public class AnalyzeController : Controller
    {
        public string Print()
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