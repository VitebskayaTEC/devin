using Dapper;
using Devin.Models;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Mvc;

namespace Devin.Controllers
{
    public class DevicesController : Controller
    {
        public ActionResult Index() => View();

        public ActionResult List() => View();

        public ActionResult DefectAct() => View();

        public string PrintDefectAct()
        {
            var keys = new Dictionary<string, string>
            {
                { "motherboard", "материнская плата (вздутие конденсаторов, отказ контроллеров)" },
                { "power", "блок питания (перегрев входных цепец)" },
                { "cpu", "процессор (повреждение внутренних цепей контроллера)" },
                { "hdd", "жесткий диск (разрушение поверхности пластин вследствие большого износа накопителя)" },
                { "ram", "ОЗУ (перегрев)" },
                { "videocard", "видеокарта (перегрев, ошибки контроллера, артефакты)" },
            };

            var works = new Dictionary<string, string>
            {
                { "motherboard", "Ремонт материнской платы" },
                {"power", "Ремонт блока питания" },
                { "cpu", "Ремонт процессора" },
                { "hdd", "Ремонт жесткого диска" },
                { "ram", "Ремонт ОЗУ" },
                { "videocard", "Ремонт видеокарты" },
            };

            var model = new
            {
                Id = Request.Form.Get("id"),
                Date = DateTime.TryParse(Request.Form.Get("data"), out DateTime d) ? d : DateTime.Now,
                Description = Request.Form.Get("description"),
                Inventory = Request.Form.Get("inventory"),
                Name = Request.Form.Get("name"),
                MolTitle = Request.Form.Get("mol_post"),
                MolName = Request.Form.Get("mol_name"),
                WorkTime = Request.Form.Get("work_time"),
                Positions = Request.Form.Get("positions").Split(new string[] { ";;" }, StringSplitOptions.RemoveEmptyEntries),
            };

            var months = new string[] { "января", "февраля", "марта", "апреля", "мая", "июня", "июля", "августа", "сентября", "октября", "ноября", "декабря" };

            string template = Server.MapPath("/devin/content/exl/") + "defect.xls";
            string output = Server.MapPath("/devin/content/excels/") + "defect.xls";

            if (!System.IO.File.Exists(template)) return "Файла шаблона не существует либо путь к нему неправильно прописан в исходниках";

            IWorkbook book;
            using (var fs = new FileStream(template, FileMode.Open, FileAccess.Read))
            {
                book = new HSSFWorkbook(fs);
            }

            var sheet = book.GetSheetAt(0);

            sheet.GetRow(27).GetCell(22).SetCellValue(model.MolTitle);
	        sheet.GetRow(27).GetCell(68).SetCellValue(model.MolName);
            sheet.GetRow(33).GetCell(0).SetCellValue(model.Description + " (" + model.Name + ") инв. № " + model.Inventory + " Cрок работы: " + model.WorkTime + " часов");
            sheet.GetRow(35).GetCell(16).SetCellValue("произошел выход из строя следующих комплектующих:");

	        sheet.GetRow(87).GetCell(2).SetCellValue(model.Date.Day);
            sheet.GetRow(87).GetCell(8).SetCellValue(months[model.Date.Month -1]);
            sheet.GetRow(87).GetCell(32).SetCellValue(model.Date.Year);

            string defects = "";
            int i = 0;

            foreach (var position in model.Positions)
            {
                if (position != "")
                {
                    var param = position.Split(new string[] { "::" }, StringSplitOptions.RemoveEmptyEntries);

                    if (defects != "") defects += ", ";
                    defects += keys.ContainsKey(param[0]) ? keys[param[0]] : param[0];

                    sheet.GetRow(73 + i).GetCell(5).SetCellValue(works.ContainsKey(param[0]) ? works[param[0]] : "Ремонт (" + param[0] + ")");
                    sheet.GetRow(73 + i).GetCell(70).SetCellValue(param[1]);
                    i++;
                }
            }

	        sheet.GetRow(38).GetCell(0).SetCellValue(defects);

            using (var fs = new FileStream(output, FileMode.OpenOrCreate, FileAccess.Write))
            {
                book.Write(fs);
            }

            return "<a href='/devin/content/excels/defect.xls'>Дефектный акт в формате Excel</a>";
        }

        public ActionResult Table() => View();

        public ActionResult Table1C() => View();

        public ActionResult Compare1C() => View();

        public void HideDevice1C(int Id, bool Hide)
        {
            using (var conn = Database.Connection())
            {
                conn.Execute("UPDATE Devices1C SET IsHide = @Hide WHERE Inventory = @Id", new { Id, Hide });
            }
        }
    }
}