using Devin.Models;
using Devin.ViewModels;
using LinqToDB;
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
                using (var db = new DevinContext())
                {
                    int id = int.Parse(Item.Replace("objects", ""));
                    if (id > 200)
                    {
                        var foldersQuery = from o in db.Objects1C
                                           where o.Account == null
                                           orderby o.Guild
                                           group o by o.Guild into g
                                           select g.Key;

                        var foldersNames = foldersQuery.ToArray();
                        string guild = "";

                        for (int i = 0; i < foldersNames.Length; i++)
                        {
                            if (200 + i == id)
                            {
                                guild = foldersNames[i];
                                break;
                            }
                        }

                        var model = db.Objects1C
                            .Where(x => x.Account == null && x.Guild == guild)
                            .OrderBy(x => x.Inventory)
                            .ToList();

                        return View("Items", model);
                    }
                    else if (id > 100)
                    {
                        var foldersQuery = from o in db.Objects1C
                                           where o.Account != null
                                           orderby o.Account
                                           group o by o.Account into g
                                           select g.Key;

                        var foldersNames = foldersQuery.ToArray();
                        string account = "";

                        for (int i = 0; i < foldersNames.Length; i++)
                        {
                            if (100 + i == id)
                            {
                                account = foldersNames[i];
                                break;
                            }
                        }

                        var model = db.Objects1C
                             .Where(x => x.Account == account)
                             .OrderBy(x => x.Inventory)
                             .ToList();
                        
                        return View("Items", model);
                    }
                    else
                    {
                        var model = new Objects1CViewModel();
                        if (id == 2)
                        {
                            return View("FolderData", model.OS);
                        }
                        else
                        {
                            return View("FolderData", model.Materials);
                        }
                    }
                }
            }
            else if (!string.IsNullOrEmpty(Search))
            {
                var model = new Objects1CViewModel(Search);
                return View("Search", model.SearchResults);
            }
            else
            {
                var model = new Objects1CViewModel();
                return View("List", model);
            }
        }

        public ActionResult Import() => View();

        public ActionResult Asutp() => View();


        public JsonResult CreateDevice(string Id)
        {
            using (var db = new DevinContext())
            {
                var obj = db.Objects1C.Where(x => x.Inventory == Id).FirstOrDefault();

                var device = db.Devices.Where(x => x.Inventory == Id).Select(x => x.Id).FirstOrDefault();
                var storage = db.Storages.Where(x => x.Inventory == Id).Select(x => x.Id).FirstOrDefault();

                if (device != 0) return Json(new { Warning = "Уже существует устройство с таким инвентарным номером." });
                if (storage != 0) return Json(new { Error = "Уже существует позиция на складе с таким инвентарным номером." });

                int id = db.InsertWithInt32Identity(new Device
                {
                    Inventory = obj.Inventory,
                    PublicName = obj.Description,
                    DateInstall = obj.Date ?? DateTime.Now,
                    Location = obj.Location,
                    IsOff = false,
                    IsDeleted = false
                });

                db.Log(User, "devices", id, "Создано устройство по данным из 1С [object" + Id + "]");
                db.Log(User, "objects1c", Id, "С данной записи создана карточка устройства [device" + id + "]. Запись отмечена как проверенная");

                return Json(new { Good = "Карточка устройства успешно создана" });
            }
        }

        public JsonResult CreateStorage(string Id)
        {
            using (var db = new DevinContext())
            {
                var obj = db.Objects1C.Where(x => x.Inventory == Id).FirstOrDefault();

                var device = db.Devices.Where(x => x.Inventory == Id).Select(x => x.Id).FirstOrDefault();
                var storage = db.Storages.Where(x => x.Inventory == Id).Select(x => x.Id).FirstOrDefault();

                if (device != 0) return Json(new { Warning = "Уже существует устройство с таким инвентарным номером." });
                if (storage != 0) return Json(new { Error = "Уже существует позиция на складе с таким инвентарным номером." });

                var newStorage = new Storage
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
                    FolderId = 0,
                    Type = ""
                };

                string name = newStorage.Name.ToLower();

                // Картриджи
                if (name.Contains("картридж") || name.Contains("тонер") || name.Contains("чернильница") || name.Contains("катридж")) newStorage.Type = "PRN";

                // Мониторы
                else if (name.Contains("монитор")) newStorage.Type = "DIS";

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
                    || name.Contains("привод")) newStorage.Type = "CMP";

                // Периферия
                else if (name.Contains("клави") || name.Contains("мыш")) newStorage.Type = "INP";

                // Коммутаторы
                else if (name.Contains("коммутатор")) newStorage.Type = "SWT";

                // Периферия
                else if (name.Contains("батарея") || name.Contains("ибп") || name.Contains("элемент питания")) newStorage.Type = "UPS";

                // Другое
                else newStorage.Type = "RR";

                int id = db.InsertWithInt32Identity(newStorage);

                db.Log(User, "devices", id, "Создана позиция на складе по данным из 1С [object" + Id + "]");
                db.Log(User, "objects1c", Id, "С данной записи создана карточка позиции [storage" + id + "]. Запись отмечена как проверенная");

                return Json(new { Good = "Позиция успешно добавлена на склад" });
            }
        }

        public JsonResult ProcessSelected(string Select, string Mode)
        {
            try
            {
                var inventories = (Select ?? "")
                    .Replace("object", "")
                    .Split(new [] { "," }, StringSplitOptions.RemoveEmptyEntries);

                if (inventories.Length == 0) return Json(new { Warning = "Не передано ни одного объекта для обработки" });

                using (var db = new DevinContext())
                {
                    string text = "", message = "";

                    if (Mode == "check")
                    {
                        text = "Объект отмечен как проверенный";
                        message = "проверенные";
                    }
                    if (Mode == "uncheck")
                    {
                        text = "Объект отмечен как непроверенный";
                        message = "непроверенные";
                    }
                    if (Mode == "hide")
                    {
                        text = "Объект отмечен как скрытый";
                        message = "скрытые";
                    }
                    if (Mode == "visible")
                    {
                        text = "Объект отмечен как отображаемый";
                        message = "отображаемые";
                    }
                    
                    foreach (var inventory in inventories)
                    {
                        db.Log(User, "objects1c", inventory, text);
                    }

                    return Json(new { Good = "Выбранные записи успешно отмечены как " + message });
                }
            }
            catch (Exception e)
            {
                return Json(new { Error = "Ошибка при выполнении кода на сервере<br />" + e.Message });
            }
        }
    }
}