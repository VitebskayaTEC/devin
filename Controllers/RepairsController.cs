using Dapper;
using Devin.Models;
using Devin.ViewModels;
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
            using (var conn = Database.Connection())
            {
                var old = conn.QueryFirst<Repair>("SELECT * FROM Repairs WHERE Id = @Id", new { Id });
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

                if (old.DeviceId != repair.DeviceId)
                {
                    conn.Execute("INSERT INTO Activity (Date, Source, Id, Text, Username) VALUES (@Date, @Source, @Id, @Text, @Username)", new Activity
                    {
                        Date = DateTime.Now,
                        Source = "devices",
                        Id = old.DeviceId.ToString(),
                        Text = "Ремонт устройства перемещен на другое устройство: #" + repair.DeviceId,
                        Username = User.Identity.Name
                    });
                    conn.Execute("INSERT INTO Activity (Date, Source, Id, Text, Username) VALUES (@Date, @Source, @Id, @Text, @Username)", new Activity
                    {
                        Date = DateTime.Now,
                        Source = "devices",
                        Id = repair.DeviceId.ToString(),
                        Text = "Добавлен ремонт c другого устройства: #" + old.DeviceId,
                        Username = User.Identity.Name
                    });
                }

                if (old.StorageId != repair.StorageId)
                {
                    conn.Execute("INSERT INTO Activity (Date, Source, Id, Text, Username) VALUES (@Date, @Source, @Id, @Text, @Username)", new Activity
                    {
                        Date = DateTime.Now,
                        Source = "storages",
                        Id = old.StorageId.ToString(),
                        Text = "Ремонт устройства [device" + old.DeviceId + "] перемещен на другую позицию: [storage" + repair.StorageId + "]",
                        Username = User.Identity.Name
                    });
                    conn.Execute("INSERT INTO Activity (Date, Source, Id, Text, Username) VALUES (@Date, @Source, @Id, @Text, @Username)", new Activity
                    {
                        Date = DateTime.Now,
                        Source = "storages",
                        Id = repair.StorageId.ToString(),
                        Text = "Ремонт устройства [device" + old.DeviceId + "] перемещен c другой позиции: [storage" + old.StorageId + "]",
                        Username = User.Identity.Name
                    });

                    // Полная отмена изменений склада от ремонта
                    if (old.IsOff)
                    {
                        if (old.IsVirtual)
                        {
                            conn.Execute("UPDATE Storages SET Noff = Noff - @Number WHERE Inventory = @StorageInventory", old);
                        }
                        else
                        {
                            conn.Execute("UPDATE Storages SET Nstorage = Nstorage + @Number, Noff = Noff - @Number WHERE Inventory = @StorageInventory", old);
                        }
                    }
                    else
                    {
                        if (old.IsVirtual)
                        {
                            conn.Execute("UPDATE Storages SET Nrepairs = Nrepairs - @Number WHERE Inventory = @StorageInventory", old);
                        }
                        else
                        {
                            conn.Execute("UPDATE Storages SET Nstorage = Nstorage + @Number, Nrepairs = Nrepairs - @Number WHERE Inventory = @StorageInventory", old);
                        }
                    }

                    // Создание изменений склада по новым данным
                    if (old.IsOff)
                    {
                        if (old.IsVirtual)
                        {
                            conn.Execute("UPDATE Storages SET Noff = Noff + @Number WHERE Inventory = @StorageInventory", new { repair.StorageId, old.Number });
                        }
                        else
                        {
                            conn.Execute("UPDATE Storages SET Nstorage = Nstorage - @Number, Noff = Noff + @Number WHERE Inventory = @StorageInventory", new { repair.StorageId, old.Number });
                        }
                    }
                    else
                    {
                        if (old.IsVirtual)
                        {
                            conn.Execute("UPDATE Storages SET Nrepairs = Nrepairs + @Number WHERE Inventory = @StorageInventory", new { repair.StorageId, old.Number });
                        }
                        else
                        {
                            conn.Execute("UPDATE Storages SET Nstorage = Nstorage - @Number, Nrepairs = Nrepairs + @Number WHERE Inventory = @StorageInventory", new { repair.StorageId, old.Number });
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
                            conn.Execute("UPDATE Storages SET Noff = Noff - @Number WHERE Inventory = @Inventory", new { old.Number, Inventory });
                        }
                        else
                        {
                            conn.Execute("UPDATE Storages SET Nstorage = Nstorage + @Number, Noff = Noff - @Number WHERE Inventory = @Inventory", new { old.Number, Inventory });
                        }
                    }
                    else
                    {
                        if (old.IsVirtual)
                        {
                            conn.Execute("UPDATE Storages SET Nrepairs = Nrepairs - @Number WHERE Inventory = @Inventory", new { old.Number, Inventory });
                        }
                        else
                        {
                            conn.Execute("UPDATE Storages SET Nstorage = Nstorage + @Number, Nrepairs = Nrepairs - @Number WHERE Inventory = @Inventory", new { old.Number, Inventory });
                        }
                    }

                    // Создание изменений склада по новым данным
                    if (repair.IsOff)
                    {
                        if (repair.IsVirtual)
                        {
                            conn.Execute("UPDATE Storages SET Noff = Noff + @Number WHERE Inventory = @StorageInventory", repair);
                        }
                        else
                        {
                            conn.Execute("UPDATE Storages SET Nstorage = Nstorage - @Number, Noff = Noff + @Number WHERE Inventory = @StorageInventory", repair);
                        }
                    }
                    else
                    {
                        if (repair.IsVirtual)
                        {
                            conn.Execute("UPDATE Storages SET Nrepairs = Nrepairs + @Number WHERE Inventory = @StorageInventory", repair);
                        }
                        else
                        {
                            conn.Execute("UPDATE Storages SET Nstorage = Nstorage - @Number, Nrepairs = Nrepairs + @Number WHERE Inventory = @StorageInventory", repair);
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
                    // Сохранение в базе
                    conn.Execute(@"UPDATE Repairs SET 
                        DeviceId          = @DeviceId
                        ,StorageId        = @StorageId
                        ,Date             = @Date
                        ,Number           = @Number
                        ,IsOff            = @IsOff
                        ,IsVirtual        = @IsVirtual
                        ,WriteoffId       = @WriteoffId
                        ,FolderId         = @FolderId
                    WHERE Id = @Id", repair);

                    conn.Execute("INSERT INTO Activity (Date, Source, Id, Text, Username) VALUES (@Date, @Source, @Id, @Text, @Username)", new Activity
                    {
                        Date = DateTime.Now,
                        Source = "repairs",
                        Id = repair.Id.ToString(),
                        Text = "Ремонт изменен. Изменения: " + string.Join(",\n", changes.ToArray()),
                        Username = User.Identity.Name
                    });

                    return Json(new { Good = "Запись о ремонте сохранена!<br/>Изменены поля:<br />" + string.Join(",<br />", changes.ToArray()) });
                }
                else
                {
                    return Json(new { Warning = "Изменений не было" });
                }
            }
        }

        public JsonResult Delete(int Id)
        {
            using (var conn = Database.Connection())
            {
                var repair = conn.QueryFirst<Repair>("SELECT * FROM Repairs WHERE Id = @Id", new { Id });
                if (repair.IsOff)
                {
                    if (repair.IsVirtual)
                    {
                        conn.Execute("UPDATE Storages SET Noff = Noff - @Number WHERE Id = @StorageId", repair);
                    }
                    else
                    {
                        conn.Execute("UPDATE Storages SET Nstorage = Nstorage + @Number, Noff = Noff - @Number WHERE Id = @StorageId", repair);
                    }
                }
                else
                {
                    if (repair.IsVirtual)
                    {
                        conn.Execute("UPDATE Storages SET Nrepairs = Nrepairs - @Number WHERE Id = @StorageId", repair);
                    }
                    else
                    {
                        conn.Execute("UPDATE Storages SET Nstorage = Nstorage + @Number, Nrepairs = Nrepairs - @Number WHERE Id = @StorageId", repair);
                    }
                }
                conn.Execute("INSERT INTO Activity (Date, Source, Id, Text, Username) VALUES (@Date, @Source, @Id, @Text, @Username)", new Activity
                {
                    Date = DateTime.Now,
                    Source = "storages",
                    Id = repair.StorageId.ToString(),
                    Text = "Отменен ремонт [repair" + Id + "]. Исп. кол-во возвращено",
                    Username = User.Identity.Name
                });
                conn.Execute("INSERT INTO Activity (Date, Source, Id, Text, Username) VALUES (@Date, @Source, @Id, @Text, @Username)", new Activity
                {
                    Date = DateTime.Now,
                    Source = "repairs",
                    Id = Id.ToString(),
                    Text = "Ремонт удален",
                    Username = User.Identity.Name
                });
                conn.Execute("DELETE FROM Repairs WHERE Id = @Id", new { Id });

                return Json(new { Good = "Ремонт удален<br />Использованные позиции возвращены на склад" });
            }
        }

        public JsonResult DeleteAll(int Id)
        {
            using (var conn = Database.Connection())
            {
                var repairs = conn.Query<int>("SELECT Id FROM Repairs WHERE WriteoffId = @Id", new { Id });
                foreach (var repair in repairs)
                {
                    Delete(repair);
                }
            }

            return Json(new { Good = "Все ремонты из списания отменены" });
        }

        public JsonResult Move(string Repairs, string Key)
        {
            using (var conn = Database.Connection())
            {
                int WriteoffId = int.TryParse(Key.Replace("w", ""), out int i) ? i : 0;
                int FolderId = int.TryParse(Key.Replace("g", ""), out i) ? i : 0;

                string[] repairs = Repairs.Split(new string[] { ";;" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var repair in repairs)
                {
                    int Id = int.TryParse(repair, out i) ? i : 0;
                    
                    if (Id != 0)
                    {
                        conn.Execute("UPDATE Repairs SET WriteoffId = @WriteoffId, FolderId = @FolderId WHERE Id = @Id", new { WriteoffId, FolderId, Id });
                        string text = Key == "0" 
                            ? "Ремонт [repair" + Id + "] размещен отдельно" 
                            : WriteoffId != 0 
                                ? "Ремонт [repair" + Id + "] перемещен в списание [off" + WriteoffId + "]"
                                : "Ремонт [repair" + Id + "] перемещен в группу [group" + FolderId + "]";

                        conn.Execute("INSERT INTO Activity (Date, Source, Id, Text, Username) VALUES (@Date, @Source, @Id, @Text, @Username)", new Activity
                        {
                            Date = DateTime.Now,
                            Source = "repairs",
                            Id = Id.ToString(),
                            Text = text,
                            Username = User.Identity.Name
                        });
                    }
                }
            }

            return Json(new { Good = "Перемещение выполнено успешно" });
        }

        public JsonResult Off(int Id)
        {
            using (var conn = Database.Connection())
            {
                var repairs = conn.Query<Repair>("SELECT Id, Number, StorageInventory, Author = @Author FROM Repairs WHERE WriteoffId = @Id AND IsOff <> 1", new { Id, Author = User.Identity.Name });

                foreach (var repair in repairs)
                {
                    conn.Execute("UPDATE Storages SET Noff = Noff + @Number, Nrepairs = Nrepairs - @Number WHERE Inventory = @StorageInventory", repair);
                    conn.Execute("UPDATE Repairs SET IsOff = 1 WHERE Id = @Id", repair);
                    conn.Execute("INSERT INTO Activity (Date, Source, Id, Text, Username) VALUES (GetDate(), 'storages', @StorageInventory, @Text, @Name)", new
                    {
                        repair.StorageId,
                        Text = "Обновлена позиция [storage" + repair.StorageId + "] при переводе ремонта [repair" + repair.Id + "] в списанный: " + repair.Number + " шт. деталей перемещены из используемых в списанные",
                        User.Identity.Name
                    });
                    conn.Execute("INSERT INTO Activity (Date, Source, Id, Text, Username) VALUES (GetDate(), 'repairs', @Id, @Text, @Name)", new
                    {
                        repair.Id,
                        Text = "Ремонт помечен как списанный",
                        User.Identity.Name
                    });
                }
            }

            return Json(new { Good = "Все ремонты отмечены как списанные, позиции возвращены на склад" });
        }

        public JsonResult OffSelected(string Repairs)
        {
            string[] repairs = Repairs.Split(new string[] { ";;" }, StringSplitOptions.RemoveEmptyEntries);
            using (var conn = Database.Connection())
            {
                foreach (string r in repairs)
                {
                    int Id = int.TryParse(r, out int i) ? i : 0;
                    if (Id != 0)
                    {
                        var repair = conn.Query<Repair>("SELECT Id, Number, StorageId, Author = @Author FROM Repairs WHERE Id = @Id AND IsOff <> 1", new { Id, Author = User.Identity.Name }).FirstOrDefault();
                        if (repair != null)
                        {
                            conn.Execute("UPDATE Storages SET Noff = Noff + @Number, Nrepairs = Nrepairs - @Number WHERE Id = @StorageId", repair);
                            conn.Execute("UPDATE Repairs SET IsOff = 1 WHERE Id = @Id", repair);
                            conn.Execute("INSERT INTO Activity (Date, Source, Id, Text, Username) VALUES (GetDate(), 'storages', @StorageId, @Text, @Author)", new {
                                repair.StorageId,
                                repair.Author,
                                Text = "Обновлена позиция при переводе ремонта [repair" + repair.Id + "] в списанные: " + repair.Number + " шт. деталей перемещены из используемых в списанные"
                            });
                            conn.Execute("INSERT INTO Activity (Date, Source, Id, Text, Username) VALUES (GetDate(), 'storages', @StorageInventory, @Text, @Author)", new
                            {
                                repair.StorageId,
                                repair.Author,
                                Text = "Ремонт помечен как списанный"
                            });
                        }
                    }
                }
            }

            return Json(new { Good = "Все ремонты отмечены как списанные, позиции возвращены на склад" });
        }

        public JsonResult On(int Id)
        {
            using (var conn = Database.Connection())
            {
                var repairs = conn.Query<Repair>("SELECT Id, Number, StorageId, Author = @Author FROM Repairs WHERE WriteoffId = @Id AND IsOff <> 0", new { Id, Author = User.Identity.Name });

                foreach (var repair in repairs)
                {
                    conn.Execute("UPDATE Storages SET Noff = Noff - @Number, Nrepairs = Nrepairs + @Number WHERE Id = @StorageId", repair);
                    conn.Execute("UPDATE Repairs SET IsOff = 0 WHERE Id = @Id", repair);
                    conn.Execute("INSERT INTO Activity (Date, Source, Id, Text, Username) VALUES (GetDate(), 'storages', @StorageId, @Text, @Name)", new
                    {
                        repair.StorageId,
                        Text = "Обновлена позиция при переводе ремонта [repair" + repair.Id + "] в активное состояние: " + repair.Number + " шт. деталей перемещены из списанных в используемые",
                        User.Identity.Name
                    });
                    conn.Execute("INSERT INTO Activity (Date, Source, Id, Text, Username) VALUES (GetDate(), 'repairs', @Id, @Text, @Name)", new
                    {
                        repair.Id,
                        Text = "Ремонт переведен в активное состояние",
                        User.Identity.Name
                    });
                }
            }

            return Json(new { Good = "Все ремонты отмечены как активные, позиции забраны со склада" });
        }

        public JsonResult OnSelected(string Repairs)
        {
            string[] repairs = Repairs.Split(new string[] { ";;" }, StringSplitOptions.RemoveEmptyEntries);
            using (var conn = Database.Connection())
            {
                foreach (string r in repairs)
                {
                    int Id = int.TryParse(r, out int i) ? i : 0;
                    if (Id != 0)
                    {
                        var repair = conn.Query<Repair>("SELECT Id, Number, StorageId, Author = @Author FROM Repairs WHERE Id = @Id AND IsOff <> 0", new { Id, Author = User.Identity.Name }).FirstOrDefault();
                        if (repair != null)
                        {
                            conn.Execute("UPDATE Storages SET Noff = Noff - @Number, Nrepairs = Nrepairs + @Number WHERE Id = @StorageId", repair);
                            conn.Execute("UPDATE Repairs SET IsOff = 0 WHERE Id = @Id", repair);
                            conn.Execute("INSERT INTO Activity (Date, Source, Id, Text, Username) VALUES (GetDate(), 'storages', @StorageId, @Text, @Author)", new
                            {
                                repair.StorageId,
                                repair.Author,
                                Text = "Обновлена позиция при переводе ремонта [repair" + repair.Id + "] в активное состояние: " + repair.Number + " шт. деталей из списанных в используемые"
                            });
                            conn.Execute("INSERT INTO Activity (Date, Source, Id, Text, Username) VALUES (GetDate(), 'repairs', @StorageInventory, @Text, @Author)", new
                            {
                                repair.StorageId,
                                repair.Author,
                                Text = "Ремонт помечен как активный"
                            });
                        }
                    }
                }
            }

            return Json(new { Good = "Все ремонты отмечены как активные, позиции забраны со склада" });
        }

        public JsonResult EndCreateFromDevice(string Id, string[] Repairs, string Writeoff)
        {
            List<Repair> repairs = new List<Repair>();
            int id = int.Parse(Id.Replace("device", ""));
            foreach (string s in Repairs)
            {
                string[] sub = s.Split(':');
                var r = new Repair
                {
                    StorageId = int.TryParse(sub[0], out int i) ? i : 0,
                    Number = int.TryParse(sub[1], out i) ? i : 0,
                    IsVirtual = sub[2] == "true",
                    DeviceId = id,
                    FolderId = 0,
                    Author = User.Identity.Name,
                    Date = DateTime.Now,
                    IsOff = false
                };
                if (r.StorageId != 0 && r.Number != 0) repairs.Add(r);
            }

            using (var conn = Database.Connection())
            {
                int WriteoffId = 0;
                if (!string.IsNullOrEmpty(Writeoff))
                {
                    conn.Execute("INSERT INTO Writeoffs (Name, Type, Date, FolderId, CostArticle) VALUES (@Writeoff, 'mat', GetDate(), 0, 0)", new { Writeoff });
                    WriteoffId = conn.QueryFirst<int>("SELECT Max(Id) FROM Writeoffs");
                    foreach (var repair in repairs) repair.WriteoffId = WriteoffId;
                    conn.Execute("INSERT INTO Activity (Date, Source, Id, Text, Username) VALUES (GetDate(), 'writeoffs', @WriteoffId, @Text, @Name)", new
                    {
                        WriteoffId,
                        User.Identity.Name,
                        Text = "Автоматически создано списание при ремонте устройства [device" + Id + "]"
                    });
                }

                foreach (var repair in repairs)
                {
                    conn.Execute("INSERT INTO Repairs (DeviceId, StorageId, Number, Date, IsOff, IsVirtual, Author) VALUES (@DeviceId, @StorageId, @Number, @Date, @IsOff, @IsVirtual, @Author)", repair);
                    conn.Execute("UPDATE Storages SET " + (repair.IsVirtual ? "" : "Nstorage = Nstorage - @Number,") + "Nrepairs = Nrepairs + @Number WHERE Id = @StorageId", repair);

                    repair.Id = conn.QueryFirst<int>("SELECT Max(Id) FROM Repairs");

                    conn.Execute("INSERT INTO Activity (Date, Source, Id, Text, Username) VALUES (GetDate(), 'repairs', @Id, @Text, @Name)", new
                    {
                        repair.Id,
                        User.Identity.Name,
                        Text = "Ремонт: использована позиция с инвентарным № [storage" + repair.StorageId + "] в количестве " + repair.Number + " шт." + (repair.IsVirtual ? " (виртуальный)" : "")
                    });
                }

                return Json(new
                {
                    Good = "Ремонты успешно созданы",
                    WriteoffId
                });
            }
        }

        public JsonResult EndCreateFromStorages(string[] Repairs, string Writeoff)
        {
            List<Repair> repairs = new List<Repair>();
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
                    IsOff = false
                };
                if (r.StorageId != 0 && r.Number != 0) repairs.Add(r);
            }

            repairs = repairs.Where(x => x.DeviceId != 0).ToList();
            if (repairs.Count == 0) return Json(new { Warning = "Нет выбранных объектов для создания ремонтов" });

            using (var conn = Database.Connection())
            {
                int WriteoffId = 0;
                if (!string.IsNullOrEmpty(Writeoff))
                {
                    conn.Execute("INSERT INTO Writeoffs (Name, Type, Date, FolderId, CostArticle) VALUES (@Writeoff, 'expl', GetDate(), 0, 0)", new { Writeoff });
                    WriteoffId = conn.QueryFirst<int>("SELECT Max(Id) FROM Writeoffs");
                    foreach (var repair in repairs) repair.WriteoffId = WriteoffId;
                    conn.Execute("INSERT INTO Activity (Date, Source, Id, Text, Username) VALUES (GetDate(), 'writeoffs', @WriteoffId, @Text, @Name)", new
                    {
                        WriteoffId,
                        User.Identity.Name,
                        Text = "Автоматически создано списание при создании группы ремонтов на складе"
                    });
                }

                foreach (var repair in repairs)
                {
                    conn.Execute("INSERT INTO Repairs (DeviceId, StorageId, Number, Date, IsOff, IsVirtual, Author) VALUES (@DeviceId, @StorageId, @Number, @Date, @IsOff, @IsVirtual, @Author)", repair);
                    conn.Execute("UPDATE Storages SET " + (repair.IsVirtual ? "" : "Nstorage = Nstorage - @Number,") + "Nrepairs = Nrepairs + @Number WHERE Id = @StorageId", repair);

                    repair.Id = conn.QueryFirst<int>("SELECT Max(Id) FROM Repairs");

                    conn.Execute("INSERT INTO Activity (Date, Source, Id, Text, Username) VALUES (GetDate(), 'repairs', @Id, @Text, @Name)", new
                    {
                        repair.Id,
                        User.Identity.Name,
                        Text = "Ремонт: использована позиция с инвентарным № [storage" + repair.StorageId + "] в количестве " + repair.Number + " шт." + (repair.IsVirtual ? " (виртуальный)" : "")
                    });
                }

                return Json(new
                {
                    Good = "Ремонты успешно созданы",
                    WriteoffId
                });
            }
        }
    }
}