using Dapper;
using Devin.Models;
using Devin.ViewModels;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
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

        public ActionResult Load(string Item, string Search)
        {
            if ((Item ?? "").Contains("device"))
            {
                int Id = int.Parse(Item.Replace("device", ""));
                Computer computer = new Computer { Id = Id };
                computer.Load();
                return View("ComputerData", computer);
            }
            else
            {
                var model = new DevicesViewModel(Search);

                if ((Item ?? "").Contains("folder"))
                {
                    int Id = int.Parse(Item.Replace("folder", ""));
                    return View("FolderData", Folder.FindSubFolder(model.Folders, Id));
                }
                else if (!string.IsNullOrEmpty(Search))
                {
                    ViewBag.Search = Search;
                    return View("Search", model.Devices);
                }
                else
                {
                    return View("List", model);
                }
            }
        }

        public ActionResult Cart(int Id) => View(model: Id);

        public ActionResult History() => View();

        public ActionResult Files(string Id) => View(model: Id);

        public ActionResult HistoryById(string Id) => View(model: Id);

        public ActionResult ElmById(string Id) => View(model: Id);

        public ActionResult HistoryRepairsById(int Id) => View(model: Id);

        public ActionResult DefectAct(int Id) => View(model: Id);

        public ActionResult Table() => View();

        public ActionResult Table1C() => View();

        public ActionResult Compare1C() => View();

        public ActionResult WorkPlaces() => View();

        public ActionResult Repair(int Id) => View(model: Id);


        public JsonResult Update(int Id, [Bind(Include = "Id,Name,Inventory,Type,Description,PublicName,Location,PlaceId,Mol,SerialNumber,PassportNumber,ServiceTag,OS,OSKey,PrinterId,IsOff")] Device device)
        {
            // Валидация 
            if (!DateTime.TryParse(Request.Form.Get("DateInstall"), out DateTime d)) return Json(new { Error = "Дата установки введена в неверном формате" }); else device.DateInstall = d;

            if (DateTime.TryParse(Request.Form.Get("DateLastRepair"), out d)) device.DateLastRepair = d;
            else device.DateLastRepair = null;


            using (var conn = Database.Connection())
            {
                // Логирование изменений
                var old = conn.QueryFirst<Device>(@"SELECT * FROM Devices WHERE Id = @Id", new { device.Id });

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
                if (device.PublicName != old.PublicName)
                {
                    changes.Add($"имя для печати [{old.PublicName} => {device.PublicName}]");
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
                        ,DateLastRepair = @DateLastRepair
                        ,PublicName     = @PublicName
                    WHERE Id = @Id", device);

                    conn.Execute("INSERT INTO Activity (Date, Source, Id, Text, Username) VALUES (@Date, @Source, @Id, @Text, @Username)", new Activity
                    {
                        Date = DateTime.Now,
                        Source = "devices",
                        Id = device.Id.ToString(),
                        Text = "Позиция изменена. Изменения: " + string.Join(",\n", changes.ToArray()),
                        Username = User.Identity.Name
                    });

                    return Json(new { Good = "Позиция успешно обновлена!<br />Изменены поля:<br />" + string.Join(",<br />", changes.ToArray()) });
                }
                else
                {
                    return Json(new { Warning = "Изменений не было" });
                }
            }
        }

        public JsonResult Copy(string Id)
        {
            int id = int.Parse(Id.Replace("device", ""));
            using (var conn = Database.Connection())
            {
                Device device = conn.QueryFirst<Device>("SELECT * FROM Devices WHERE id = @id", new { id });
                device.Name += " (копия)";
                conn.Execute(@"INSERT INTO Devices (
                    [Inventory], [Type], [Name], [NetworkName], [Description], [DateInstall], [DateLastRepair], [Mol], [SerialNumber], [PassportNumber], [Location], [OS], [OSKey], [PrinterId], [FolderId], [IsOff], [IsDeleted], [ServiceTag], [PlaceId], [ComputerId]
                ) VALUES (
                    @Inventory, @Type, @Name, @NetworkName, @Description, @DateInstall, @DateLastRepair, @Mol, @SerialNumber, @PassportNumber, @Location, @OS, @OSKey, @PrinterId, @FolderId, @IsOff, @IsDeleted, @ServiceTag, @PlaceId, @ComputerId
                )", device);

                int newid = conn.QueryFirst<int>("SELECT Max(Id) FROM Devices");

                conn.Execute("INSERT INTO Activity (Date, Source, Id, Text, Username) VALUES (@Date, @Source, @Id, @Text, @Username)", new Activity
                {
                    Date = DateTime.Now,
                    Source = "devices",
                    Id = newid.ToString(),
                    Text = "Позиция скопирована из [device" + Id + "]",
                    Username = User.Identity.Name
                });

                return Json(new { Id = "device" + newid, Good = "Карточка устройства успешно скопирована" });
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
                    DateInstall = DateTime.Now.Date,
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

        public JsonResult Delete(string Id)
        {
            int id = int.Parse(Id.Replace("device", ""));

            using (var conn = Database.Connection())
            {
                conn.Execute("UPDATE Devices SET IsDeleted = 1 WHERE id = @id", new { id });

                conn.Execute("INSERT INTO Activity (Date, Source, Id, Text, Username) VALUES (@Date, @Source, @Id, @Text, @Username)", new Activity
                {
                    Date = DateTime.Now,
                    Source = "devices",
                    Id = id.ToString(),
                    Text = "Устройство удалено",
                    Username = User.Identity.Name
                });

                return Json(new { Good = "Устройство удалено" });
            }
        }

        public JsonResult MoveSelected(string Devices, string Key)
        {
            string[] devices = Devices.Split(new string[] { ";;" }, StringSplitOptions.RemoveEmptyEntries);

            using (var conn = Database.Connection())
            {
                int ComputerId = int.TryParse(Key.Replace("computer", ""), out int i) ? i : 0;
                int FolderId = int.TryParse(Key.Replace("folder", ""), out i) ? i : 0;

                string message = "Устройство размещено отдельно";
                if (Key.Contains("computer")) message = "Устройство перемещено в компьютер [device" + ComputerId + "]";
                if (Key.Contains("folder")) message = "Устройство перемещено в папку [folder" + FolderId + "]";

                foreach (string device in devices)
                {
                    int Id = int.TryParse(device, out i) ? i : 0;
                    if (Id != 0)
                    {
                        conn.Execute("UPDATE Devices SET ComputerId = @ComputerId, FolderId = @FolderId WHERE Id = @Id", new { ComputerId, FolderId, Id });
                        conn.Execute("INSERT INTO Activity (Date, Source, Id, Text, Username) VALUES (GetDate(), 'devices', @Id, @Text, @Name)", new
                        {
                            Id,
                            Text = message,
                            User.Identity.Name
                        });
                    }
                }
            }

            return Json(new { Good = "Все устройства успешно перемещены" });
        }

        public JsonResult Move(int Id, int FolderId)
        {
            using (var conn = Database.Connection())
            {
                conn.Execute("UPDATE Devices SET FolderId = @FolderId WHERE Id = @Id", new { Id, FolderId });
                return Json(new { Good = "Компьютер успешно перемещен" });
            }
        }

        public void HideObject1C(string Id, bool Hide)
        {
            using (var conn = Database.Connection())
            {
                conn.Execute("UPDATE Objects1C SET IsHide = @Hide WHERE Inventory = @Id", new { Id, Hide });
            }
        }

        public JsonResult PrintDefectAct(string Description, string Inventory, string Name, string Position, string Mol, string Time)
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

            string[] months = new string[] { "января", "февраля", "марта", "апреля", "мая", "июня", "июля", "августа", "сентября", "октября", "ноября", "декабря" };


            DateTime Date = DateTime.TryParse(Request.Form.Get("DateInstall"), out DateTime d) ? d : DateTime.Now;
            string[] Positions = Request.Form.Get("positions").Split(new string[] { ";;" }, StringSplitOptions.RemoveEmptyEntries);

            string output = Url.Action("excels", "content") + "defect.xls";

            if (!System.IO.File.Exists(Server.MapPath("../content/exl/defect.xls"))) return Json(new { Error = "Файла шаблона не существует либо путь к нему неправильно прописан в исходниках" });

            HSSFWorkbook book;
            using (var fs = new FileStream(Server.MapPath("../content/exl/defect.xls"), FileMode.Open, FileAccess.Read))
            {
                book = new HSSFWorkbook(fs);
            }

            var sheet = book.GetSheetAt(0);

            sheet.GetRow(27).GetCell(22).SetCellValue(Position);
            sheet.GetRow(27).GetCell(68).SetCellValue(Mol);
            sheet.GetRow(33).GetCell(0).SetCellValue(Description + " (" + Name + ") инв. № " + Inventory + " Cрок работы: " + Time + " часов");
            sheet.GetRow(35).GetCell(16).SetCellValue("произошел выход из строя следующих комплектующих:");

            sheet.GetRow(87).GetCell(2).SetCellValue(Date.Day);
            sheet.GetRow(87).GetCell(8).SetCellValue(months[Date.Month - 1]);
            sheet.GetRow(87).GetCell(32).SetCellValue(Date.Year);

            string defects = "";
            int i = 0;

            foreach (string position in Positions)
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

            using (var fs = new FileStream(Server.MapPath("../content/excels/defect.xls"), FileMode.OpenOrCreate, FileAccess.Write))
            {
                book.Write(fs);
            }

            return Json(new { Good = "Дефектный акт создан", Link = Url.Action("excels", "content") + "/defect.xls" });
        }

        public JsonResult PrintRecordCartByIp(string Ip)
        {
            IPAddress addr = IPAddress.Parse(Ip);
            IPHostEntry entry = Dns.GetHostEntry(addr);
            string Name = entry.HostName.Substring(0, entry.HostName.IndexOf('.'));

            using (var conn = Database.Connection())
            {
                int Id = conn.Query<int>("SELECT Id FROM Devices WHERE Name = @Name", new { Name }).FirstOrDefault();
                if (Id == 0) return Json(new { Error = "Компьютер с именем \"" + Name + "\" не найден в базе." });
                return PrintRecordCart(Id.ToString());
            }
        }

        public JsonResult PrintRecordCart(string Id)
        {
            Id = Id.Replace("device", "");
            string template = Server.MapPath(Url.Action("exl", "content") + "/ReportCart.xls");

            HSSFWorkbook book;
            using (var fs = new FileStream(template, FileMode.Open, FileAccess.Read))
            {
                book = new HSSFWorkbook(fs);
            }

            ISheet sheet = book.GetSheetAt(0);

            string cabinet;
            List<Device> devices;
            string name;
            using (var conn = Database.Connection())
            {
                cabinet = conn.QueryFirst<string>(@"SELECT WorkPlaces.Location FROM Devices LEFT OUTER JOIN WorkPlaces ON WorkPlaces.Id = Devices.PlaceId WHERE Devices.Id = @Id", new { Id });

                devices = conn.Query<Device>(@"SELECT
	                Devices.Inventory,
	                Devices.Name,
	                Devices.Description,
	                Devices.PublicName,
	                Objects1C.Description AS Description1C,
	                Devices.SerialNumber,
	                CASE WHEN Objects1C.Mol IS NULL THEN Devices.Mol ELSE Objects1C.Mol END AS Mol,
	                Devices.DateInstall
                FROM Devices
                LEFT OUTER JOIN Objects1C ON Objects1C.Inventory = Devices.Inventory
                WHERE Devices.Id = @Id OR Devices.ComputerId = @Id AND Devices.IsDeleted <> 1
                ORDER BY Devices.Inventory, Description1C", new { Id }).AsList();

                name = conn.QueryFirst<string>("SELECT Name FROM Devices WHERE Id = @Id", new { Id });
            }

            string now = DateTime.Now.ToString("dd.MM.yyyy");
            sheet.GetRow(4).GetCell(9).SetCellValue(cabinet);
            sheet.GetRow(12).GetCell(0).SetCellValue(now);

            int step = 0;
            foreach (Device device in devices)
            {
                sheet.CopyRow(8, 9 + step);
                IRow row = sheet.GetRow(9 + step);
                row.GetCell(0).SetCellValue(device.Inventory);
                row.GetCell(1).SetCellValue(device.Description1C ?? device.Description);
                row.GetCell(2).SetCellValue(device.SerialNumber ?? "");
                row.GetCell(3).SetCellValue(device.PublicName);
                row.GetCell(4).SetCellValue(device.DateInstall.ToString("dd.MM.yyyy"));
                row.GetCell(5).SetCellValue(device.Mol ?? "");
                row.GetCell(6).SetCellValue(now);
                step++;
            }

            sheet.GetRow(8).Height = 0;
            string output = @"\\backup\pub\web\devin\Карточка_учета_оргтехники_" + name.Replace("/", "-").Replace(".", "") + ".xls";

            using (var fs = new FileStream(output, FileMode.OpenOrCreate, FileAccess.Write))
            {
                book.Write(fs);
            }

            return Json(new
            {
                Good = "Карточка учета вычислительной техники на рабочем месте \"" + name + "\" создана",
                Link = "http://www.vst.vitebsk.energo.net/files/devin/Карточка_учета_оргтехники_" + name.Replace("/", "-").Replace(".", "") + ".xls?r=" + (new Random()).Next()
            }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult PrintRecordCartByFolder(int Id)
        {
            string template = Server.MapPath(Url.Action("exl", "content") + "/ReportCart.xls");

            HSSFWorkbook book;
            using (var fs = new FileStream(template, FileMode.Open, FileAccess.Read))
            {
                book = new HSSFWorkbook(fs);
            }

            ISheet sheet = book.GetSheetAt(0);

            Folder cabinet;
            List<Device> devices;
            using (var conn = Database.Connection())
            {
                cabinet = conn.QueryFirst<Folder>(@"SELECT * FROM Folders WHERE Id = @Id", new { Id });

                devices = conn.Query<Device>(@"SELECT
					Devices.Inventory,
	                Devices.Name,
	                Devices.Description,
	                Devices.PublicName,
	                Objects1C.Description AS Description1C,
	                Devices.SerialNumber,
	                CASE WHEN Objects1C.Mol IS NULL THEN Devices.Mol ELSE Objects1C.Mol END AS Mol,
	                Devices.DateInstall
                FROM Devices
				LEFT OUTER JOIN Devices AS Computers ON Computers.Id        = Devices.ComputerId
                LEFT OUTER JOIN Objects1C            ON Objects1C.Inventory = Devices.Inventory
                WHERE Devices.FolderId = @Id OR Computers.FolderId = @Id AND Devices.IsDeleted <> 1
                GROUP BY Devices.Inventory,
	                Devices.Name,
	                Devices.Description,
	                Devices.PublicName,
	                Objects1C.Description,
	                Devices.SerialNumber,
	                CASE WHEN Objects1C.Mol IS NULL THEN Devices.Mol ELSE Objects1C.Mol END,
	                Devices.DateInstall
                ORDER BY Devices.Inventory, Description1C", new { Id }).AsList();
            }

            string now = DateTime.Now.ToString("dd.MM.yyyy");
            sheet.GetRow(4).GetCell(9).SetCellValue(cabinet.Name);
            sheet.GetRow(12).GetCell(0).SetCellValue(now);

            int step = 0;
            foreach (Device device in devices)
            {
                sheet.CopyRow(8, 9 + step);
                IRow row = sheet.GetRow(9 + step);
                row.GetCell(0).SetCellValue(device.Inventory);
                row.GetCell(1).SetCellValue(device.Description1C ?? device.Description);
                row.GetCell(2).SetCellValue(device.SerialNumber ?? "");
                row.GetCell(3).SetCellValue(device.PublicName);
                row.GetCell(4).SetCellValue(device.DateInstall.ToString("dd.MM.yyyy"));
                row.GetCell(5).SetCellValue(device.Mol ?? "");
                row.GetCell(6).SetCellValue(now);
                step++;
            }

            sheet.GetRow(8).Height = 0;
            
            string output = @"\\backup\pub\web\devin\Карточка_учета_оргтехники_" + cabinet.Name.Replace("/", "-").Replace(".", "") + ".xls";

            using (var fs = new FileStream(output, FileMode.OpenOrCreate, FileAccess.Write))
            {
                book.Write(fs);
            }

            return Json(new
            {
                Good = "Карточка учета вычислительной техники на рабочем месте \"" + cabinet.Name + "\" создана",
                Link = "http://www.vst.vitebsk.energo.net/files/devin/Карточка_учета_оргтехники_" + cabinet.Name.Replace("/", "-").Replace(".", "") + ".xls?r=" + (new Random()).Next()
            }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult CreateWorkPlace()
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
                return Json(new { Good = "Новое рабочее место создано" });
            }
        }

        public JsonResult UpdateWorkPlace([Bind(Include = "Id,Location,Guild")] WorkPlace workPlace)
        {
            using (var conn = Database.Connection())
            {
                conn.Execute("UPDATE WorkPlaces SET Location = @Location, Guild = @Guild WHERE Id = @Id", workPlace);
                return Json(new { Good = "Рабочее место обновлено" });
            }
        }

        public JsonResult DeleteWorkPlace(int Id)
        {
            using (var conn = Database.Connection())
            {
                conn.Execute("DELETE FROM WorkPlaces WHERE Id = @Id", new { Id });
                return Json(new { Good = "Рабочее место удалено" });
            }
        }

        public JsonResult UploadFile(string Id)
        {
            int id = int.Parse(Id.Replace("device", ""));
            string root = @"\\web\DevinFiles\";
            string name = "";

            try
            {
                if (!Directory.Exists(root)) return Json(new { Error = "Папка-хранилище файлов \"" + root + "\" недоступна" });
                if (!Directory.Exists(root + "devices")) Directory.CreateDirectory(root + "devices");
                if (!Directory.Exists(root + "devices\\" + id)) Directory.CreateDirectory(root + "devices\\" + id);

                var file = Request.Files[0];
                name = file.FileName;

                if (System.IO.File.Exists(root + "devices\\" + id + "\\" + name)) return Json(new { Warning = "Файл с таким именем уже существует" });

                file.SaveAs(root + "devices\\" + id + "\\" + name);
            }
            catch (Exception)
            {
                return Json(new { Error = "Ошибка при доступе к файловой системе, возможно, нет прав у учетной записи ASP.NET" });
            }

            using (var conn = Database.Connection())
            {
                string old = conn.Query<string>("SELECT Files FROM Devices WHERE Id = @id", new { id }).FirstOrDefault() ?? "";
                string changed = old + ";;" + name;

                conn.Execute("UPDATE Devices SET Files = @changed WHERE Id = @id", new { id, changed });
                conn.Execute("INSERT INTO Activity (Date, Source, Id, Text, Username) VALUES (GetDate(), 'devices', @Id, @Text, @Name)", new
                {
                    Id = id,
                    Text = "К списку файлов добавлен файл [" + name + "]",
                    User.Identity.Name
                });

                return Json(new { Good = "К списку файлов добавлен файл [" + name + "]" });
            }
        }

        public JsonResult DeleteFile(string Id, string File)
        {
            int id = int.Parse(Id.Replace("device", ""));

            System.IO.File.Delete(@"\\web\DevinFiles\devices\" + id + @"\" + File);

            using (var conn = Database.Connection())
            {
                string old = conn.Query<string>("SELECT Files FROM Devices WHERE Id = @id", new { id }).FirstOrDefault() ?? "";
                string changed = old.Replace(File, "").Replace(";;;;", ";;");

                conn.Execute("UPDATE Devices SET Files = @changed WHERE Id = @id", new { id, changed });
                conn.Execute("INSERT INTO Activity (Date, Source, Id, Text, Username) VALUES (GetDate(), 'devices', @Id, @Text, @Name)", new
                {
                    Id = id,
                    Text = "Из списка файлов удален файл [" + File + "]",
                    User.Identity.Name
                });

                return Json(new { Good = "Из списка файлов удален файл [" + File + "]" });
            }
        }
    }
}