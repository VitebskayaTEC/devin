using Dapper;
using Devin.Models;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Devin.Controllers
{
    public class DevicesController : Controller
    {
        public ActionResult Index() => View();

        public ActionResult List() => View();

        public ActionResult Cart(string Id) => View(model: Id);

        public ActionResult History() => View();

        public ActionResult HistoryById(string Id) => View(model: Id);

        public ActionResult ElmById(string Id) => View(model: Id);

        public ActionResult DefectAct() => View();

        public ActionResult Table() => View();

        public ActionResult Table1C() => View();

        public ActionResult Compare1C() => View();

        public ActionResult WorkPlaces() => View();


        public string Update([Bind(Include = "DeviceNumber,Name,Inventory,DeviceClass,Description,Location,PlaceId,Mol,SerialNumber,PassportNumber,ServiceTag,OS,OSKey,PrinterId,InUse")] Device device)
        {
            // Валидация 
            if (!DateTime.TryParse(Request.Form.Get("DateInstall"), out DateTime d)) return "error:Дата установки введена в неверном формате"; else device.DateInstall = d;

            if (DateTime.TryParse(Request.Form.Get("LastRepairDate"), out d)) device.LastRepairDate = d;
            else device.LastRepairDate = null;


            using (var conn = Database.Connection())
            {
                // Логирование изменений
                var old = conn.QueryFirst<Device>(@"SELECT
                    d.Inventory
                    ,d.Name
                    ,d.Description
                    ,d.Mol
                    ,d.OS
                    ,d.OSKey
                    ,d.PlaceId
                    ,d.number_device   AS [DeviceNumber]
                    ,d.class_device    AS [DeviceClass]
                    ,c.number_device   AS [ComputerId]
                    ,d.install_date    AS [DateInstall]
                    ,d.number_serial   AS [SerialNumber]
                    ,d.number_passport AS [PassportNumber]
                    ,d.attribute       AS [Location]
                    ,d.ID_prn          AS [PrinterId]
                    ,g.G_ID            AS [GroupId]
                    ,d.service_tag     AS [ServiceTag]
                    ,d.used            AS [InUse]
                FROM Device d
                LEFT OUTER JOIN Device  c ON c.number_device = d.number_comp
                LEFT OUTER JOIN [Group] g ON g.G_ID          = d.G_ID
                WHERE d.number_device = @DeviceNumber", new { device.DeviceNumber });

                List<string> changes = new List<string>();

                if (device.Inventory != old.Inventory)
                {
                    changes.Add($"инвентарный номер [{old.Inventory} => {device.Inventory}]");
                }
                if (device.Name != old.Name)
                {
                    changes.Add($"наименование [{old.Name} => {device.Name}]");
                }
                if (device.DeviceClass != old.DeviceClass)
                {
                    changes.Add($"класс [{old.DeviceClass} => {device.DeviceClass}]");
                }
                if (device.Description != old.Description)
                {
                    changes.Add($"описание [{old.Description} => {device.Description}]");
                }
                if (device.Location != old.Location)
                {
                    changes.Add($"расположение [{old.Location} => {device.Location}]");
                }
                if (device.PlaceId != old.PlaceId)
                {
                    changes.Add($"помещение [{old.PlaceId} => {device.PlaceId}]");
                }
                if (device.SerialNumber != old.SerialNumber)
                {
                    changes.Add($"серийный номер [{old.SerialNumber} => {device.SerialNumber}]");
                }
                if (device.PassportNumber != old.PassportNumber)
                {
                    changes.Add($"паспортный номер [{old.PassportNumber} => {device.PassportNumber}]");
                }
                if (device.ServiceTag != old.ServiceTag)
                {
                    changes.Add($"сервис-тег [{old.ServiceTag} => {device.ServiceTag}]");
                }
                if (device.OS != old.OS)
                {
                    changes.Add($"операционная система [{old.OS} => {device.OS}]");
                }
                if (device.OSKey != old.OSKey)
                {
                    changes.Add($"ключ ОС [{old.OSKey} => {device.OSKey}]");
                }
                if (device.PrinterId != old.PrinterId)
                {
                    changes.Add($"типовой принтер [{old.PrinterId} => {device.PrinterId}]");
                }
                if (device.InUse != old.InUse)
                {
                    changes.Add($"списан [{old.InUse} => {device.InUse}]");
                }
                if (device.DateInstall != old.DateInstall)
                {
                    changes.Add($"дата установки [{old.DateInstall} => {device.DateInstall}]");
                }
                if (device.LastRepairDate != old.LastRepairDate)
                {
                    changes.Add($"дата последнего ремонта [{old.LastRepairDate} => {device.LastRepairDate}]");
                }
                if (device.Mol != old.Mol)
                {
                    changes.Add($"дата последнего ремонта [{old.Mol} => {device.Mol}]");
                }

                string destination = Request.Form.Get("Destination");
                device.ComputerId = old.ComputerId;
                device.GroupId = old.GroupId;

                string oldDestination = old.ComputerId != null 
                    ? ("(компьютер) " + old.ComputerId) 
                    : (old.GroupId != 0 
                        ? ("(папка) " + old.GroupId)
                        : "отдельно");

                if (destination.Contains("computer"))
                {
                    destination = destination.Replace("computer", "");
                    if (oldDestination != ("(компьютер) " + destination))
                    {
                        device.ComputerId = destination;
                        device.GroupId = 0;
                        changes.Add($"расположение [{oldDestination} => (компьютер) {destination}]");
                    }
                }
                else if (destination.Contains("folder"))
                {
                    int i = int.Parse(destination.Replace("folder", ""));
                    if (oldDestination != ("(папка) " + i))
                    {
                        device.ComputerId = null;
                        device.GroupId = i;
                        changes.Add($"расположение [{oldDestination} => (папка) {i}]");
                    }
                }
                else
                {
                    if (oldDestination != "отдельно")
                    {
                        device.GroupId = 0;
                        device.ComputerId = null;
                        changes.Add($"расположение [{oldDestination} => отдельно]");
                    }                    
                }

                if (changes.Count > 0)
                {
                    // Сохранение в базе
                    conn.Execute(@"UPDATE Device SET
                        Inventory        = @Inventory
                        ,Name            = @Name
                        ,Description     = @Description
                        ,OS              = @OS
                        ,OSKey           = @OSKey
                        ,PlaceId         = @PlaceId
                        ,class_device    = @DeviceClass
                        ,number_comp     = @ComputerId
                        ,install_date    = @DateInstall
                        ,number_serial   = @SerialNumber
                        ,number_passport = @PassportNumber
                        ,attribute       = @Location
                        ,ID_prn          = @PrinterId
                        ,G_ID            = @GroupId
                        ,service_tag     = @ServiceTag
                        ,used            = @InUse
                        ,Mol             = @Mol
                    FROM Device
                    WHERE number_device = @DeviceNumber", device);

                    conn.Execute("INSERT INTO Activity (Date, Source, Id, Text, Username) VALUES (@Date, @Source, @Id, @Text, @Username)", new Activity
                    {
                        Date = DateTime.Now,
                        Source = "devices",
                        Id = device.DeviceNumber,
                        Text = "Позиция изменена. Изменения: " + string.Join(",\n", changes.ToArray()),
                        Username = User.Identity.Name
                    });

                    return "Позиция успешно обновлена! Изменены поля: <br />" + string.Join(",<br />", changes.ToArray());
                }
                else
                {
                    return "Изменений не было";
                }
            }
        }


        public void HideDevice1C(int Id, bool Hide)
        {
            using (var conn = Database.Connection())
            {
                conn.Execute("UPDATE Devices1C SET IsHide = @Hide WHERE Inventory = @Id", new { Id, Hide });
            }
        }

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
            sheet.GetRow(87).GetCell(8).SetCellValue(months[model.Date.Month - 1]);
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

        public string PrintRecordCartByIp(string Ip)
        {
            IPAddress addr = IPAddress.Parse(Ip);
            IPHostEntry entry = Dns.GetHostEntry(addr);
            string Name = entry.HostName.Substring(0, entry.HostName.IndexOf('.'));

            using (var conn = Database.Connection())
            {
                string Id = conn.Query<string>("SELECT number_device FROM Device WHERE Name = @Name", new { Name }).FirstOrDefault();
                if (Id == null) return "error:Компьютер с именем \"" + Name + "\" не найден в базе.";
                return PrintRecordCart(Id);
            }
        }

        public string PrintRecordCart(string Id)
        {
            string template = Server.MapPath(Url.Action("exl", "content") + "/ReportCart.xlsx");
            string output = Server.MapPath(Url.Action("excels", "content") + "/ReportCart.xlsx");

            XSSFWorkbook book;
            using (var fs = new FileStream(template, FileMode.Open, FileAccess.Read))
            {
                book = new XSSFWorkbook(fs);
            }

            ISheet sheet = book.GetSheetAt(0);

            string cabinet;
            List<Device> devices;
            using (var conn = Database.Connection())
            {
                cabinet = conn.QueryFirst<string>(@"SELECT Location FROM Device LEFT OUTER JOIN WorkPlaces ON WorkPlaces.Id = Device.PlaceId WHERE number_device = @Id", new { Id });

                devices = conn.Query<Device>(@"SELECT
	                Device.Inventory,
	                Device.Name,
	                Device.Description,
	                Devices1C.Description AS Description1C,
	                Device.number_serial AS SerialNumber,
	                CASE WHEN Devices1C.Mol IS NULL THEN Device.Mol ELSE Devices1C.Mol END AS Mol,
	                Device.install_date AS DateInstall
                FROM Device
                LEFT OUTER JOIN Devices1C ON Devices1C.Inventory = Device.Inventory
                WHERE number_device = @Id OR number_comp = @Id
                ORDER BY Device.Inventory, Description1C", new { Id }).AsList();
            }

            string now = DateTime.Now.ToString("dd.MM.yyyy");
            sheet.GetRow(4).GetCell(9).SetCellValue(cabinet);
            sheet.GetRow(20).GetCell(0).SetCellValue(now);

            int step = 0;
            foreach (Device device in devices)
            {
                IRow row = sheet.GetRow(8 + step);
                row.GetCell(0).SetCellValue(device.Inventory);
                row.GetCell(1).SetCellValue(device.Description1C ?? device.Description);
                row.GetCell(2).SetCellValue(device.SerialNumber ?? "");
                row.GetCell(3).SetCellValue(device.Description);
                row.GetCell(4).SetCellValue(device.DateInstall.ToString("dd.MM.yyyy"));
                row.GetCell(5).SetCellValue(device.Mol ?? "");
                row.GetCell(6).SetCellValue(now);
                step++;
            }

            using (var fs = new FileStream(output, FileMode.OpenOrCreate, FileAccess.Write))
            {
                book.Write(fs);
            }


            return Url.Action("excels", "content") + "/ReportCart.xlsx";
        }


        public void CreateWorkPlace()
        {
            using (var conn = Database.Connection())
            {
                conn.Execute("INSERT INTO WorkPlaces (Location) VALUES ('')");
                int Id = conn.QueryFirst<int>("SELECT Max(Id) FROM WorkPlaces");
                conn.Execute("UPDATE WorkPlaces SET Location = @location WHERE Id = @Id", new WorkPlace
                {
                    Id = Id,
                    Location = "Новое рабочее место #" + Id
                });
            }
        }

        public void UpdateWorkPlace([Bind(Include = "Id,Location,Guild")] WorkPlace workPlace)
        {
            using (var conn = Database.Connection())
            {
                conn.Execute("UPDATE WorkPlaces SET Location = @Location, Guild = @Guild WHERE Id = @Id", workPlace);
            }
        }

        public void DeleteWorkPlace(int Id)
        {
            using (var conn = Database.Connection())
            {
                conn.Execute("DELETE FROM WorkPlaces WHERE Id = @Id", new { Id });
            }
        }
    }
}