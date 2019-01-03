using Dapper;
using Devin.Models;
using Devin.ViewModels;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Devin.Controllers
{
    public class Objects1CController : Controller
    {
        public ActionResult Index() => View();

        public ActionResult Load(string Item, string Search)
        {
            if (!string.IsNullOrEmpty(Item))
            {
                using (var conn = Database.Connection())
                {
                    int id = int.Parse(Item.Replace("objects", ""));
                    if (id > 200)
                    {
                        var folders = conn.Query<Folder>("SELECT (200 + ROW_NUMBER() OVER(ORDER BY Guild ASC)) AS Id, Guild AS Name FROM Objects1C WHERE Account IS NULL AND IsHide = 0 GROUP BY Guild ORDER BY Guild").AsList();
                        var folder = folders.FirstOrDefault(x => x.Id == id);
                        var model = conn.Query<Object1C>("SELECT * FROM Objects1C WHERE Account IS NULL AND Guild = @Name AND IsHide = 0 ORDER BY Inventory", new { folder.Name }).AsList();
                        return View("Items", model);
                    }
                    else if (id > 100)
                    {
                        var folders = conn.Query<Folder>("SELECT (100 + ROW_NUMBER() OVER(ORDER BY Account ASC)) AS Id, Account AS Name FROM Objects1C WHERE Account IS NOT NULL AND IsHide = 0 GROUP BY Account ORDER BY Account").AsList();
                        var folder = folders.FirstOrDefault(x => x.Id == id);
                        var model = conn.Query<Object1C>("SELECT * FROM Objects1C WHERE Account = @Name AND IsHide = 0 ORDER BY Inventory", new { folder.Name }).AsList();
                        return View("Items", model);
                    }
                    else
                    {
                        var model = new Objects1CViewModel();
                        if (id == 2)
                        {
                            return View("FolderData", model.OS);
                        }
                        else if (id == 1)
                        {
                            return View("FolderData", model.Materials);
                        }
                        else
                        {
                            return View("Items", model.Hided.Objects);
                        }
                    }
                }
            }
            else
            {
                var model = new Objects1CViewModel();
                if (!string.IsNullOrEmpty(Search))
                {
                    return View("Search", model.SearchResults);
                }
                else
                {
                    return View("List", model);
                }
            }
        }

        public ActionResult Import() => View();

        public JsonResult CreateDevice(string Id)
        {
            using (var conn = Database.Connection())
            {
                var obj = conn.QueryFirst<Object1C>("SELECT * FROM Objects1C WHERE Inventory = @Id", new { Id });
                var device = conn.Query<Device>("SELECT Id FROM Devices WHERE Inventory = @Id", new { Id }).FirstOrDefault();
                var storage = conn.Query<Storage>("SELECT Id FROM Storages WHERE Inventory = @Id", new { Id }).FirstOrDefault();

                if (device != null) return Json(new { Warning = "Уже существует устройство с таким инвентарным номером." });
                if (storage != null) return Json(new { Error = "Уже существует позиция на складе с таким инвентарным номером." });

                conn.Execute("INSERT INTO Devices (Inventory, PublicName, Mol, DateInstall, Location, IsOff, IsDeleted) VALUES (@Inventory, @Description, @Mol, @Date, @Location, 0, 0)", obj);
                conn.Execute("UPDATE Objects1C SET IsChecked = 1 WHERE Inventory = @Id", new { Id });

                int id = conn.QueryFirst<int>("SELECT Max(Id) FROM Devices");

                conn.Log(User, "devices", id, "Создано устройство по данным из 1С [object" + Id + "]");
                conn.Log(User, "objects1c", Id, "С данной записи создана карточка устройства [device" + id + "]. Запись отмечена как проверенная");

                return Json(new { Good = "Карточка устройства успешно создана" });
            }
        }

        public JsonResult CreateStorage(string Id)
        {
            using (var conn = Database.Connection())
            {
                var obj = conn.QueryFirst<Object1C>("SELECT * FROM Objects1C WHERE Inventory = @Id", new { Id });
                var device = conn.Query<Device>("SELECT Id FROM Devices WHERE Inventory = @Id", new { Id }).FirstOrDefault();
                var storage = conn.Query<Storage>("SELECT Id FROM Storages WHERE Inventory = @Id", new { Id }).FirstOrDefault();

                if (device != null) return Json(new { Error = "Уже существует устройство с таким инвентарным номером." });
                if (storage != null) return Json(new { Warning = "Уже существует позиция на складе с таким инвентарным номером." });

                storage = new Storage
                {
                    Inventory = obj.Inventory,
                    Name = obj.Description,
                    Cost = obj.RestCost,
                    Nall = obj.Rest,
                    Nstorage = obj.Rest,
                    Nrepairs = 0,
                    Noff = 0,
                    Date = obj.Date ?? DateTime.Now,
                    Account = obj.Account,
                    IsDeleted = false,
                    CartridgeId = 0,
                    FolderId = 0
                };

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

                conn.Execute("INSERT INTO Storages (Inventory, Name, Type, Cost, Nall, Nstorage, Nrepairs, Noff, Date, IsDeleted, Account) VALUES (@Inventory, @Name, @Type, @Cost, @Nall, @Nstorage, @Nrepairs, @Noff, @Date, @IsDeleted, @Account)", storage);
                conn.Execute("UPDATE Objects1C SET IsChecked = 1 WHERE Inventory = @Id", new { Id });

                int id = conn.QueryFirst<int>("SELECT Max(Id) FROM Storages");
                conn.Log(User, "devices", id, "Создана позиция на складе по данным из 1С [object" + Id + "]");
                conn.Log(User, "objects1c", Id, "С данной записи создана карточка позиции [storage" + id + "]. Запись отмечена как проверенная");

                return Json(new { Good = "Позиция успешно добавлена на склад" });
            }
        }

        public JsonResult Check(string Id)
        {
            using (var conn = Database.Connection())
            {
                conn.Execute("UPDATE Objects1C SET IsChecked = 1 WHERE Inventory = @Id", new { Id });
                conn.Log(User, "objects1c", Id, "Запись отмечена как проверенная");

                return Json(new { Good = "Запись отмечена как проверенная" });
            }
        }

        public JsonResult Hide(string Id)
        {
            using (var conn = Database.Connection())
            {
                conn.Execute("UPDATE Objects1C SET IsChecked = 1, IsHide = 1 WHERE Inventory = @Id", new { Id });
                conn.Log(User, "objects1c", Id, "Запись отмечена как скрытая");

                return Json(new { Good = "Запись отмечена как скрытая" });
            }
        }
    }
}