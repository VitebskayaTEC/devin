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

        public ActionResult HistoryRepairsById(int Id) => View(model: Id);

        public ActionResult DefectAct(string Id) => View(model: Id);

        public ActionResult Table() => View();

        public ActionResult Table1C() => View();

        public ActionResult Compare1C() => View();

        public ActionResult WorkPlaces() => View();


        public string Update(int Id, [Bind(Include = "Name,Inventory,Type,Description,Location,PlaceId,Mol,SerialNumber,PassportNumber,ServiceTag,OS,OSKey,PrinterId,IsOff")] Device device)
        {
            device.Id = Id;

            // Валидация 
            if (!DateTime.TryParse(Request.Form.Get("DateInstall"), out DateTime d)) return "error:Дата установки введена в неверном формате"; else device.DateInstall = d;

            if (DateTime.TryParse(Request.Form.Get("DateLastRepair"), out d)) device.DateLastRepair = d;
            else device.DateLastRepair = null;


            using (var conn = Database.Connection())
            {
                // Логирование изменений
                var old = conn.QueryFirst<Device>(@"SELECT
                    Devices.Id
                    ,Devices.Inventory
                    ,Devices.Name
                    ,Devices.Description
                    ,Devices.Mol
                    ,Devices.OS
                    ,Devices.OSKey
                    ,Devices.PlaceId
                    ,Devices.Type
                    ,Devices.DateInstall
                    ,Devices.SerialNumber
                    ,Devices.PassportNumber
                    ,Devices.Location
                    ,Devices.PrinterId
                    ,Devices.ServiceTag
                    ,Devices.IsOff
                    ,Computers.Id AS ComputerId
                    ,Folders.Id   AS FoldersId
                FROM Devices
                LEFT OUTER JOIN Devices AS Computers ON Computers.Id = Devices.ComputerId
                LEFT OUTER JOIN Folders ON Folders.Id = Devices.FolderId
                WHERE Devices.Id = @Id", new { device.Id });

                List<string> changes = new List<string>();

                if (device.Inventory != old.Inventory)
                {
                    changes.Add($"инвентарный номер [{old.Inventory} => {device.Inventory}]");
                }
                if (device.Name != old.Name)
                {
                    changes.Add($"наименование [{old.Name} => {device.Name}]");
                }
                if (device.Type != old.Type)
                {
                    changes.Add($"класс [{old.Type} => {device.Type}]");
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
                if (device.IsOff != old.IsOff)
                {
                    changes.Add($"списан [{old.IsOff} => {device.IsOff}]");
                }
                if (device.DateInstall != old.DateInstall)
                {
                    changes.Add($"дата установки [{old.DateInstall} => {device.DateInstall}]");
                }
                if (device.DateLastRepair != old.DateLastRepair)
                {
                    changes.Add($"дата последнего ремонта [{old.DateLastRepair} => {device.DateLastRepair}]");
                }
                if (device.Mol != old.Mol)
                {
                    changes.Add($"дата последнего ремонта [{old.Mol} => {device.Mol}]");
                }

                string destination = Request.Form.Get("Destination");
                device.ComputerId = old.ComputerId;
                device.FolderId = old.FolderId;

                string oldDestination = old.ComputerId != 0 
                    ? ("(компьютер) " + old.ComputerId) 
                    : (old.FolderId != 0 
                        ? ("(папка) " + old.FolderId)
                        : "отдельно");

                if (destination.Contains("computer"))
                {
                    destination = destination.Replace("computer", "");
                    if (oldDestination != ("(компьютер) " + destination))
                    {
                        device.ComputerId = int.Parse(destination);
                        device.FolderId = 0;
                        changes.Add($"расположение [{oldDestination} => (компьютер) {destination}]");
                    }
                }
                else if (destination.Contains("folder"))
                {
                    int i = int.Parse(destination.Replace("folder", ""));
                    if (oldDestination != ("(папка) " + i))
                    {
                        device.ComputerId = 0;
                        device.FolderId = i;
                        changes.Add($"расположение [{oldDestination} => (папка) {i}]");
                    }
                }
                else
                {
                    if (oldDestination != "отдельно")
                    {
                        device.FolderId = 0;
                        device.ComputerId = 0;
                        changes.Add($"расположение [{oldDestination} => отдельно]");
                    }                    
                }

                if (changes.Count > 0)
                {
                    // Сохранение в базе
                    conn.Execute(@"UPDATE Devices SET
                        Inventory       = @Inventory
                        ,Name           = @Name
                        ,Description    = @Description
                        ,OS             = @OS
                        ,OSKey          = @OSKey
                        ,PlaceId        = @PlaceId
                        ,Type           = @Type
                        ,ComputerId     = @ComputerId
                        ,DateInstall    = @DateInstall
                        ,SerialNumber   = @SerialNumber
                        ,PassportNumber = @PassportNumber
                        ,Location       = @Location
                        ,PrinterId      = @PrinterId
                        ,FolderId       = @FolderId
                        ,ServiceTag     = @ServiceTag
                        ,IsOff          = @IsOff
                        ,Mol            = @Mol
                    WHERE Id = @Id", device);

                    conn.Execute("INSERT INTO Activity (Date, Source, Id, Text, Username) VALUES (@Date, @Source, @Id, @Text, @Username)", new Activity
                    {
                        Date = DateTime.Now,
                        Source = "devices",
                        Id = device.Id.ToString(),
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

        public JsonResult Copy(int Id)
        {
            using (var conn = Database.Connection())
            {
                Device device = conn.QueryFirst<Device>("SELECT * FROM Devices WHERE Id = @Id", new { Id });
                device.Name += " (копия)";
                conn.Execute(@"INSERT INTO Devices (
                    [Inventory], [Type], [Name], [NetworkName], [Description1C][Description], [DateInstall], [DateLastRepair], [Mol], [SerialNumber], [PassportNumber], [Location], [OS], [OSKey], [PrinterId], [FolderId], [IsOff], [IsDeleted], [ServiceTag], [PassportGold], [PassportSilver], [PassportPlatinum], [PassportMPG], [PlaceId], [ComputerId]
                ) VALUES (
                    @Inventory, @Type, @Name, @NetworkName, @Description1C, @Description, @DateInstall, @DateLastRepair, @Mol, @SerialNumber, @PassportNumber, @Location, @OS, @OSKey, @PrinterId, @FolderId, @IsOff, @IsDeleted, @ServiceTag, @PassportGold, @PassportSilver, @PassportPlatinum, @PassportMPG, @PlaceId, @ComputerId
                )", device);

                int id = conn.QueryFirst<int>("SELECT Max(Id) FROM Devices");

                conn.Execute("INSERT INTO Activity (Date, Source, Id, Text, Username) VALUES (@Date, @Source, @Id, @Text, @Username)", new Activity
                {
                    Date = DateTime.Now,
                    Source = "devices",
                    Id = id.ToString(),
                    Text = "Позиция скопирована из #" + Id,
                    Username = User.Identity.Name
                });

                return Json(new { Id = id, Good = "Карточка устройства успешно скопирована" });
            }
        }

        public JsonResult Create()
        {
            using (var conn = Database.Connection())
            {
                conn.Execute("INSERT INTO Devices (Inventory, ComputerId, FolderId, Name, Description, DateInstall, IsOff, IsDeleted) VALUES (@Inventory, @ComputerId, @FolderId, @Name, @Description, @DateInstall, @IsOff, @IsDeleted)", new Device {
                    Inventory = "000000",
                    ComputerId = 0,
                    FolderId = 0, 
                    Name = "Новое устройство",
                    Description = "",
                    DateInstall = DateTime.Now,
                    IsDeleted = false,
                    IsOff = false
                });

                int id = conn.QueryFirst<int>("SELECT Max(Id) FROM Devices");

                conn.Execute("INSERT INTO Activity (Date, Source, Id, Text, Username) VALUES (@Date, @Source, @Id, @Text, @Username)", new Activity
                {
                    Date = DateTime.Now,
                    Source = "devices",
                    Id = id.ToString(),
                    Text = "Создано новое устройство",
                    Username = User.Identity.Name
                });

                return Json(new { Id = id, Good = "Создано новое устройство" });
            }

        }

        public JsonResult Delete(int Id)
        {
            using (var conn = Database.Connection())
            {
                conn.Execute("UPDATE Devices SET IsDeleted = 1 WHERE Id = @Id", new { Id });

                conn.Execute("INSERT INTO Activity (Date, Source, Id, Text, Username) VALUES (@Date, @Source, @Id, @Text, @Username)", new Activity
                {
                    Date = DateTime.Now,
                    Source = "devices",
                    Id = Id.ToString(),
                    Text = "Устройство удалено",
                    Username = User.Identity.Name
                });

                return Json(new { Good = "Устройство удалено" });
            }
        }

        public void MoveSelected(string Devices, string Key)
        {
            string[] devices = Devices.Split(new string[] { ";;" }, StringSplitOptions.RemoveEmptyEntries);

            using (var conn = Database.Connection())
            {
                foreach (string device in devices)
                {
                    int Id = int.TryParse(device, out int i) ? i : 0;
                    string message = "";
                    if (Id != 0)
                    {
                        if (Key == "0")
                        {
                            conn.Execute("UPDATE Devices SET ComputerId = 0, FolderId = 0 WHERE Id = @Id", new { Id });
                            message = "Устройство размещено отдельно";
                        }
                        else if (Key.Contains("cmp"))
                        {
                            int key = int.TryParse(Key.Replace("cmp", ""), out i) ? i : 0;
                            conn.Execute("UPDATE Devices SET ComputerId = @Key, FolderId = 0 WHERE Id = @Id", new { Id, Key = key });
                            message = "Устройство перемещено в компьютер #" + key;
                        }
                        else
                        {
                            int key = int.TryParse(Key.Replace("g", ""), out i) ? i : 0;
                            conn.Execute("UPDATE Devices SET ComputerId = 0, FolderId = @Key0 WHERE Id = @Id", new { Id, Key = key });
                            message = "Устройство перемещено в папку #" + key;
                        }
                    }

                    conn.Execute("INSERT INTO Activity (Date, Source, Id, Text, Username) VALUES (@Date, @Source, @Id, @Text, @Username)", new Activity
                    {
                        Date = DateTime.Now,
                        Source = "devices",
                        Id = Id.ToString(),
                        Text = message,
                        Username = User.Identity.Name
                    });
                }
            }
        }

        public void Move(int Id, int Key)
        {
            using (var conn = Database.Connection())
            {
                conn.Execute("UPDATE Devices SET FolderId = @Key WHERE Id = @Id", new { Id, Key });
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
                int Id = conn.Query<int>("SELECT Id FROM Devices WHERE Name = @Name", new { Name }).FirstOrDefault();
                if (Id == 0) return "error:Компьютер с именем \"" + Name + "\" не найден в базе.";
                return PrintRecordCart(Id);
            }
        }

        public string PrintRecordCart(int Id)
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
                cabinet = conn.QueryFirst<string>(@"SELECT Location FROM Devices LEFT OUTER JOIN WorkPlaces ON WorkPlaces.Id = Devices.PlaceId WHERE Id = @Id", new { Id });

                devices = conn.Query<Device>(@"SELECT
	                Devices.Inventory,
	                Devices.Name,
	                Devices.Description,
	                Devices1C.Description AS Description1C,
	                Devices.SerialNumber,
	                CASE WHEN Devices1C.Mol IS NULL THEN Devices.Mol ELSE Devices1C.Mol END AS Mol,
	                Devices.DateInstall
                FROM Devices
                LEFT OUTER JOIN Devices1C ON Devices1C.Inventory = Devices.Inventory
                WHERE Devices.Id = @Id OR Devices.ComputerId = @Id
                ORDER BY Devices.Inventory, Description1C", new { Id }).AsList();
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