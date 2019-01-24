using Devin.Models;
using Devin.ViewModels;
using LinqToDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Devin.Controllers
{
    public class RepairsController : Controller
    {
        public ActionResult Index() => View();

        public ActionResult Load(string Item, string Search)
        {
            if ((Item ?? "").Contains("off"))
            {
                int Id = int.Parse(Item.Replace("off", ""));
                Writeoff writeoff = new Writeoff { Id = Id };
                writeoff.Load();
                return View("WriteoffData", writeoff);
            }
            else
            {
                var model = new RepairsViewModel(Search);

                if ((Item ?? "").Contains("folder"))
                {
                    int Id = int.Parse(Item.Replace("folder", ""));
                    return View("FolderData", Folder.FindSubFolder(model.Folders, Id));
                }
                else if (!string.IsNullOrEmpty(Search))
                {
                    ViewBag.Search = Search;
                    return View("Search", model.Repairs);
                }
                else
                {
                    return View("List", model);
                }
            }
        }

        public ActionResult YearReport() => View();

        public ActionResult CartridgesUsage() => View();

        public ActionResult Cart(string Id) => View(model: Id);

        public ActionResult Device(int Id) => View(model: Id);

        public ActionResult Storage(int Id) => View(model: Id);

        public ActionResult History(int Id) => View(model: Id);

        public ActionResult CreateFromDevice(string Id) => View(model: Id);

        public ActionResult CreateFromDeviceData(string Id) => View(model: Id);

        public ActionResult CreateFromStorages(string Select) => View(model: Select);


        public JsonResult Update(int Id, [Bind(Include = "Id,DeviceId,StorageId,Number,IsOff,IsVirtual")] Repair repair, string Destination)
        {
            if (DateTime.TryParse(Request.Form.Get("Date"), out DateTime d))
            {
                repair.Date = d;
            }
            else
            {
                return Json(new { Error = "Дата проведения введена неправильно. Ожидается формат <b>дд.ММ.гггг чч:мм</b>" });
            }

            if (Destination.Contains("off"))
            {
                repair.FolderId = 0;
                repair.WriteoffId = int.TryParse(Destination.Replace("off", ""), out int i) ? i : 0;
            }
            else if (Destination.Contains("folder"))
            {
                repair.FolderId = int.TryParse(Destination.Replace("folder", ""), out int i) ? i : 0;
                repair.WriteoffId = 0;
            }
            else
            {
                repair.FolderId = 0;
                repair.WriteoffId = 0;
            }

            using (var db = new DevinContext())
            {
                var old = db.Repairs.Where(x => x.Id == Id).FirstOrDefault();

                if (old.DeviceId != repair.DeviceId)
                {
                    db.Log(User, "devices", old.DeviceId, "Ремонт устройства перемещен на другое устройство: [device" + repair.DeviceId + "]");
                    db.Log(User, "devices", repair.DeviceId, "Добавлен ремонт c другого устройства: [device" + old.DeviceId + "]");
                }

                if (old.StorageId != repair.StorageId)
                {
                    db.Log(User, "storages", old.StorageId, "Ремонт устройства перемещен на другую позицию: [storage" + repair.StorageId + "]");
                    db.Log(User, "storages", repair.StorageId, "Добавлен ремонт c другой позиции: [storage" + old.StorageId + "]");

                    // Полная отмена изменений склада от ремонта
                    if (old.IsOff)
                    {
                        if (old.IsVirtual)
                        {
                            db.Storages
                                .Where(x => x.Id == old.StorageId)
                                .Set(x => x.Noff, x => x.Noff - old.Number)
                                .Update();
                        }
                        else
                        {
                            db.Storages
                                .Where(x => x.Id == old.StorageId)
                                .Set(x => x.Nstorage, x => x.Nstorage + old.Number)
                                .Set(x => x.Noff, x => x.Noff - old.Number)
                                .Update();
                        }
                    }
                    else
                    {
                        if (old.IsVirtual)
                        {
                            db.Storages
                                .Where(x => x.Id == old.StorageId)
                                .Set(x => x.Nrepairs, x => x.Nrepairs - old.Number)
                                .Update();
                        }
                        else
                        {
                            db.Storages
                                .Where(x => x.Id == old.StorageId)
                                .Set(x => x.Nstorage, x => x.Nstorage + old.Number)
                                .Set(x => x.Nrepairs, x => x.Nrepairs - old.Number)
                                .Update();
                        }
                    }

                    // Создание изменений склада по новым данным
                    if (old.IsOff)
                    {
                        if (old.IsVirtual)
                        {
                            db.Storages
                                .Where(x => x.Id == repair.StorageId)
                                .Set(x => x.Noff, x => x.Noff + old.Number)
                                .Update();
                        }
                        else
                        {
                            db.Storages
                                .Where(x => x.Id == repair.StorageId)
                                .Set(x => x.Nstorage, x => x.Nstorage - old.Number)
                                .Set(x => x.Noff, x => x.Noff + old.Number)
                                .Update();
                        }
                    }
                    else
                    {
                        if (old.IsVirtual)
                        {
                            db.Storages
                                .Where(x => x.Id == repair.StorageId)
                                .Set(x => x.Nrepairs, x => x.Nrepairs + old.Number)
                                .Update();
                        }
                        else
                        {
                            db.Storages
                                .Where(x => x.Id == repair.StorageId)
                                .Set(x => x.Nstorage, x => x.Nstorage - old.Number)
                                .Set(x => x.Nrepairs, x => x.Nrepairs + old.Number)
                                .Update();
                        }
                    }
                }

                if (old.Number != repair.Number)
                {
                    int Inventory = repair.StorageId;
                    if (old.IsOff)
                    {
                        if (old.IsVirtual)
                        {
                            db.Storages
                                .Where(x => x.Id == old.StorageId)
                                .Set(x => x.Noff, x => x.Noff - old.Number)
                                .Update();
                        }
                        else
                        {
                            db.Storages
                                .Where(x => x.Id == old.StorageId)
                                .Set(x => x.Nstorage, x => x.Nstorage + old.Number)
                                .Set(x => x.Noff, x => x.Noff - old.Number)
                                .Update();
                        }
                    }
                    else
                    {
                        if (old.IsVirtual)
                        {
                            db.Storages
                                .Where(x => x.Id == repair.StorageId)
                                .Set(x => x.Nrepairs, x => x.Nrepairs - old.Number)
                                .Update();
                        }
                        else
                        {
                            db.Storages
                                .Where(x => x.Id == repair.StorageId)
                                .Set(x => x.Nstorage, x => x.Nstorage + old.Number)
                                .Set(x => x.Nrepairs, x => x.Nrepairs - old.Number)
                                .Update();
                        }
                    }

                    // Создание изменений склада по новым данным
                    if (repair.IsOff)
                    {
                        if (repair.IsVirtual)
                        {
                            db.Storages
                                .Where(x => x.Id == repair.StorageId)
                                .Set(x => x.Noff, x => x.Noff + repair.Number)
                                .Update();
                        }
                        else
                        {
                            db.Storages
                                .Where(x => x.Id == repair.StorageId)
                                .Set(x => x.Nstorage, x => x.Nstorage - repair.Number)
                                .Set(x => x.Noff, x => x.Noff + repair.Number)
                                .Update();
                        }
                    }
                    else
                    {
                        if (repair.IsVirtual)
                        {
                            db.Storages
                                .Where(x => x.Id == repair.StorageId)
                                .Set(x => x.Nrepairs, x => x.Nrepairs + repair.Number)
                                .Update();
                        }
                        else
                        {
                            db.Storages
                                .Where(x => x.Id == repair.StorageId)
                                .Set(x => x.Nstorage, x => x.Nstorage - repair.Number)
                                .Set(x => x.Nrepairs, x => x.Nrepairs + repair.Number)
                                .Update();
                        }
                    }
                }

                string oldDestination = old.WriteoffId != 0 
                    ? ("off" + old.WriteoffId) 
                    : (old.FolderId != 0 
                        ? ("folder" + old.FolderId) 
                        : "отдельно");

                string newDestination = repair.WriteoffId != 0
                    ? ("off" + repair.WriteoffId)
                    : (repair.FolderId != 0
                        ? ("folder" + repair.FolderId)
                        : "отдельно");

                List<string> changes = new List<string>();
                if (old.DeviceId != repair.DeviceId) changes.Add("объект ремонта [" + old.DeviceId + " => " + repair.DeviceId + "]");
                if (old.StorageId != repair.StorageId) changes.Add("исп. позиция [" + old.StorageId + " => " + repair.StorageId + "]");
                if (old.Date != repair.Date) changes.Add("дата проведения [" + old.Date + " => " + repair.Date + "]");
                if (old.Number != repair.Number) changes.Add("кол-во исп. [" + old.Number + " => " + repair.Number + "]");
                if (old.IsOff != repair.IsOff) changes.Add("списан [" + old.IsOff + " => " + repair.IsOff + "]");
                if (old.IsVirtual != repair.IsVirtual) changes.Add("виртуальный [" + old.IsVirtual + " => " + repair.IsVirtual + "]");
                if (oldDestination != newDestination) changes.Add("расположение [" + oldDestination + " => " + newDestination + "]");

                if (changes.Count > 0)
                {
                    db.Repairs.Where(x => x.Id == repair.Id)
                        .Set(x => x.DeviceId, repair.DeviceId)
                        .Set(x => x.StorageId, repair.StorageId)
                        .Set(x => x.Date, repair.Date)
                        .Set(x => x.Number, repair.Number)
                        .Set(x => x.IsOff, repair.IsOff)
                        .Set(x => x.IsVirtual, repair.IsVirtual)
                        .Set(x => x.WriteoffId, repair.WriteoffId)
                        .Set(x => x.FolderId, repair.FolderId)
                        .Update();
                    db.Log(User, "repairs", repair.Id, "Ремонт изменен. Изменения: " + changes.ToLog());

                    return Json(new { Good = "Ремонт изменен. Изменения:<br />" + changes.ToHtml() });
                }
                else
                {
                    return Json(new { Warning = "Изменений не было" });
                }
            }
        }

        public JsonResult Delete(int Id)
        {
            using (var db = new DevinContext())
            {
                var repair = db.Repairs.Where(x => x.Id == Id).FirstOrDefault();

                if (repair.IsOff)
                {
                    if (repair.IsVirtual)
                    {
                        db.Storages
                            .Where(x => x.Id == repair.StorageId)
                            .Set(x => x.Noff, x => x.Noff - repair.Number)
                            .Update();
                    }
                    else
                    {
                        db.Storages
                            .Where(x => x.Id == repair.StorageId)
                            .Set(x => x.Nstorage, x => x.Nstorage + repair.Number)
                            .Set(x => x.Noff, x => x.Noff - repair.Number)
                            .Update();
                    }
                }
                else
                {
                    if (repair.IsVirtual)
                    {
                        db.Storages
                            .Where(x => x.Id == repair.StorageId)
                            .Set(x => x.Nrepairs, x => x.Nrepairs - repair.Number)
                            .Update();
                    }
                    else
                    {
                        db.Storages
                            .Where(x => x.Id == repair.StorageId)
                            .Set(x => x.Nstorage, x => x.Nstorage + repair.Number)
                            .Set(x => x.Nrepairs, x => x.Nrepairs - repair.Number)
                            .Update();
                    }
                }

                db.Repairs.Where(x => x.Id == repair.Id).Delete();
                db.Log(User, "storages", repair.StorageId, "Отменен ремонт [repair" + Id + "]. Исп. кол-во возвращено");
                db.Log(User, "repairs", Id, "Ремонт удален");

                return Json(new { Good = "Ремонт удален<br />Использованные позиции возвращены на склад" });
            }
        }

        public JsonResult DeleteAll(int Id)
        {
            using (var db = new DevinContext())
            {
                var repairs = db.Repairs
                    .Where(x => x.WriteoffId == Id)
                    .Select(x => x.Id)
                    .ToList();

                foreach (var repair in repairs)
                {
                    Delete(repair);
                }
            }

            return Json(new { Good = "Все ремонты из списания отменены" });
        }

        public JsonResult Move(string Repairs, string Key)
        {
            using (var db = new DevinContext())
            {
                int WriteoffId = int.TryParse(Key.Replace("w", ""), out int i) ? i : 0;
                int FolderId = int.TryParse(Key.Replace("g", ""), out i) ? i : 0;

                string[] repairs = Repairs.Split(new string[] { ";;" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var repair in repairs)
                {
                    int Id = int.TryParse(repair.Replace("repair", ""), out i) ? i : 0;
                    
                    if (Id != 0)
                    {
                        db.Repairs
                            .Where(x => x.Id == Id)
                            .Set(x => x.WriteoffId, WriteoffId)
                            .Set(x => x.FolderId, FolderId)
                            .Update();

                        string text = Key == "0" 
                            ? "Ремонт [repair" + Id + "] размещен отдельно" 
                            : WriteoffId != 0 
                                ? "Ремонт [repair" + Id + "] перемещен в списание [off" + WriteoffId + "]"
                                : "Ремонт [repair" + Id + "] перемещен в группу [group" + FolderId + "]";

                        db.Log(User, "repairs", Id, text);

                        
                    }
                }

                return Json(new { Good = "Перемещение выполнено успешно" });
            }
        }

        public JsonResult Off(string Id)
        {
            using (var db = new DevinContext())
            {
                int id = int.TryParse(Id.Replace("writeoff", ""), out int i) ? i : 0;

                var repairs = db.Repairs
                    .Where(x => x.WriteoffId == id && !x.IsOff)
                    .Select(x => new { x.Id, x.Number, x.StorageId })
                    .ToList();

                foreach (var repair in repairs)
                {
                    db.Storages
                        .Where(x => x.Id == repair.StorageId)
                        .Set(x => x.Noff, x => x.Noff + repair.Number)
                        .Set(x => x.Nrepairs, x => x.Nrepairs - repair.Number)
                        .Update();
                    db.Repairs
                        .Where(x => x.Id == repair.Id)
                        .Set(x => x.IsOff, true)
                        .Update();

                    db.Log(User, "storages", repair.StorageId, "Обновлена позиция [storage" + repair.StorageId + "] при переводе ремонта [repair" + repair.Id + "] в списанный: " + repair.Number + " шт. деталей перемещены из используемых в списанные");
                    db.Log(User, "repairs", repair.Id, "Ремонт помечен как списанный");
                }
            }

            return Json(new { Good = "Все ремонты отмечены как списанные, позиции возвращены на склад" });
        }

        public JsonResult OffSelected(string Repairs)
        {
            var repairs = Repairs.Split(new [] { ";;" }, StringSplitOptions.RemoveEmptyEntries);

            using (var db = new DevinContext())
            {
                foreach (string r in repairs)
                {
                    int Id = int.TryParse(r.Replace("repair", ""), out int i) ? i : 0;

                    if (Id != 0)
                    {
                        var repair = db.Repairs
                            .Where(x => x.Id == Id)
                            .Select(x => new { x.Id, x.Number, x.StorageId })
                            .FirstOrDefault();

                        if (repair != null)
                        {
                            db.Storages
                                .Where(x => x.Id == repair.StorageId)
                                .Set(x => x.Noff, x => x.Noff + repair.Number)
                                .Set(x => x.Nrepairs, x => x.Nrepairs - repair.Number)
                                .Update();
                            db.Repairs
                                .Where(x => x.Id == repair.Id)
                                .Set(x => x.IsOff, true)
                                .Update();

                            db.Log(User, "storages", repair.StorageId, "Обновлена позиция при переводе ремонта [repair" + repair.Id + "] в списанные: " + repair.Number + " шт. деталей перемещены из используемых в списанные");
                            db.Log(User, "repairs", repair.Id, "Ремонт помечен как списанный");
                        }
                    }
                }
            }

            return Json(new { Good = "Все ремонты отмечены как списанные" });
        }

        public JsonResult On(string Id)
        {
            using (var db = new DevinContext())
            {
                int id = int.TryParse(Id.Replace("writeoff", ""), out int i) ? i : 0;

                var repairs = db.Repairs
                    .Where(x => x.WriteoffId == id && !x.IsOff)
                    .Select(x => new { x.Id, x.Number, x.StorageId })
                    .ToList();

                foreach (var repair in repairs)
                {
                    db.Storages
                        .Where(x => x.Id == repair.StorageId)
                        .Set(x => x.Noff, x => x.Noff - repair.Number)
                        .Set(x => x.Nrepairs, x => x.Nrepairs + repair.Number)
                        .Update();
                    db.Repairs
                        .Where(x => x.Id == repair.Id)
                        .Set(x => x.IsOff, false)
                        .Update();

                    db.Log(User, "storages", repair.StorageId, "Обновлена позиция при переводе ремонта [repair" + repair.Id + "] в активное состояние: " + repair.Number + " шт. деталей перемещены из списанных в используемые");
                    db.Log(User, "repairs", repair.Id, "Ремонт переведен в активное состояние");
                }
            }

            return Json(new { Good = "Все ремонты отмечены как активные" });
        }

        public JsonResult OnSelected(string Repairs)
        {
            var repairs = Repairs.Split(new [] { ";;" }, StringSplitOptions.RemoveEmptyEntries);

            using (var db = new DevinContext())
            {
                foreach (string r in repairs)
                {
                    int Id = int.TryParse(r.Replace("repair", ""), out int i) ? i : 0;

                    if (Id != 0)
                    {
                        var repair = db.Repairs
                            .Where(x => x.Id == Id)
                            .Select(x => new { x.Id, x.Number, x.StorageId })
                            .FirstOrDefault();

                        if (repair != null)
                        {
                            db.Storages
                                .Where(x => x.Id == repair.StorageId)
                                .Set(x => x.Noff, x => x.Noff - repair.Number)
                                .Set(x => x.Nrepairs, x => x.Nrepairs + repair.Number)
                                .Update();
                            db.Repairs
                                .Where(x => x.Id == repair.Id)
                                .Set(x => x.IsOff, false)
                                .Update();

                            db.Log(User, "storages", repair.StorageId, "Обновлена позиция при переводе ремонта [repair" + repair.Id + "] в активное состояние: " + repair.Number + " шт. деталей из списанных в используемые");
                            db.Log(User, "repairs", repair.Id, "Ремонт помечен как активный");
                        }
                    }
                }
            }

            return Json(new { Good = "Все ремонты отмечены как активные" });
        }

        public JsonResult EndCreateFromDevice(string Id, string[] Repairs, string Writeoff)
        {
            var repairs = new List<Repair>();

            int id = int.Parse(Id.Replace("device", ""));

            foreach (string s in Repairs)
            {
                string[] sub = s.Split(':');
                var r = new Repair
                {
                    DeviceId = id,
                    StorageId = int.TryParse(sub[0], out int i) ? i : 0,
                    Number = int.TryParse(sub[1], out i) ? i : 0,
                    Date = DateTime.Now,
                    Author = User.Identity.Name,
                    FolderId = 0,
                    WriteoffId = 0,
                    IsVirtual = sub[2] == "true",
                    IsOff = false
                };

                if (r.StorageId != 0 && r.Number != 0) repairs.Add(r);
            }

            using (var db = new DevinContext())
            {
                int writeoffId = 0;

                if (!string.IsNullOrEmpty(Writeoff))
                {
                    writeoffId = db.Insert(new Writeoff
                    {
                        Name = Writeoff,
                        Type = "mat",
                        Date = DateTime.Now,
                        FolderId = 0,
                        CostArticle = 0
                    });

                    foreach (var repair in repairs) repair.WriteoffId = writeoffId;

                    db.Log(User, "writeoffs", writeoffId, "Автоматически создано списание при ремонте устройства [device" + Id + "]");
                }

                foreach (var repair in repairs)
                {
                    int repairId = db.InsertWithInt32Identity(repair);
                    db.Storages
                        .Where(x => x.Id == repair.StorageId)
                        .Set(x => x.Nrepairs, x => x.Nrepairs + repair.Number)
                        .Set(x => x.Nstorage, x => repair.IsVirtual ? x.Nstorage : (x.Nstorage - repair.Number))
                        .Update();
                    
                    db.Log(User, "repairs", repairId, "Ремонт: использована позиция с инвентарным № [storage" + repair.StorageId + "] в количестве " + repair.Number + " шт." + (repair.IsVirtual ? " (виртуальный)" : ""));
                }

                if (writeoffId > 0)
                {
                    return Json(new { Good = "Ремонты успешно созданы", writeoffId });
                }
                else
                {
                    return Json(new { Good = "Ремонты успешно созданы" });
                }
            }
        }

        public JsonResult EndCreateFromStorages(string[] Repairs, string Writeoff)
        {
            var repairs = new List<Repair>();

            foreach (string s in Repairs)
            {
                string[] sub = s.Split(':');
                var r = new Repair
                {
                    StorageId = int.TryParse(sub[0], out int i) ? i : 0,
                    DeviceId = int.TryParse(sub[1], out i) ? i : 0,
                    Number = int.TryParse(sub[2], out i) ? i : 0,
                    IsVirtual = sub[3] == "true",
                    FolderId = 0,
                    Author = User.Identity.Name,
                    Date = DateTime.Now,
                    IsOff = false,
                    WriteoffId = 0
                };
                if (r.StorageId != 0 && r.Number != 0) repairs.Add(r);
            }

            repairs = repairs.Where(x => x.DeviceId != 0).ToList();
            if (repairs.Count == 0) return Json(new { Warning = "Нет выбранных объектов для создания ремонтов" });

            using (var db = new DevinContext())
            {
                int writeoffId = 0;
                if (!string.IsNullOrEmpty(Writeoff))
                {
                    writeoffId = db.InsertWithInt32Identity(new Writeoff
                    {
                        Name = Writeoff,
                        Type = "expl",
                        Date = DateTime.Now,
                        FolderId = 0,
                        CostArticle = 0
                    });

                    foreach (var repair in repairs) repair.WriteoffId = writeoffId;

                    db.Log(User, "writeoffs", writeoffId, "Автоматически создано списание при создании группы ремонтов на складе");
                }

                foreach (var repair in repairs)
                {
                    repair.Id = db.InsertWithInt32Identity(repair);
                    db.Storages
                        .Where(x => x.Id == repair.StorageId)
                        .Set(x => x.Nrepairs, x => x.Nrepairs + repair.Number)
                        .Set(x => x.Nstorage, x => repair.IsVirtual ? (x.Nstorage - repair.Number) : x.Nstorage)
                        .Update();

                    db.Log(User, "repairs", repair.Id, "Ремонт: использована позиция с инвентарным № [storage" + repair.StorageId + "] в количестве " + repair.Number + " шт." + (repair.IsVirtual ? " (виртуальный)" : ""));
                }

                return Json(new { Good = "Ремонты успешно созданы", writeoffId });
            }
        }
    }
}