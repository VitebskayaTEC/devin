using Dapper;
using Devin.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Devin.Controllers
{
    public class RepairsController : Controller
    {
        public ActionResult Index() => View();

        public ActionResult List() => View();

        public ActionResult YearReport() => View();

        public ActionResult CartridgesUsage() => View();

        public ActionResult Cart(int Id) => View(model: Id);

        public ActionResult Device(int Id) => View(model: Id);

        public ActionResult Storage(string Id) => View(model: Id);

        public ActionResult CreateFromDevice(int Id) => View(model: Id);

        public ActionResult CreateFromDeviceData(int Id) => View(model: Id);

        public JsonResult Update(int Id, [Bind(Include = "DeviceId,StorageInventory,Number,IsOff,IsVirtual")] Repair repair)
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

                if (old.StorageInventory != repair.StorageInventory)
                {
                    conn.Execute("INSERT INTO Activity (Date, Source, Id, Text, Username) VALUES (@Date, @Source, @Id, @Text, @Username)", new Activity
                    {
                        Date = DateTime.Now,
                        Source = "storages",
                        Id = old.StorageInventory.ToString(),
                        Text = "Ремонт устройства #" + old.DeviceId + " перемещен на другую позицию: #" + repair.StorageInventory,
                        Username = User.Identity.Name
                    });
                    conn.Execute("INSERT INTO Activity (Date, Source, Id, Text, Username) VALUES (@Date, @Source, @Id, @Text, @Username)", new Activity
                    {
                        Date = DateTime.Now,
                        Source = "storages",
                        Id = repair.StorageInventory.ToString(),
                        Text = "Ремонт устройства #" + old.DeviceId + " перемещен c другой позиции: #" + old.StorageInventory,
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

                List<string> changes = new List<string>();
                if (old.DeviceId != repair.DeviceId) changes.Add("объект ремонта [" + old.DeviceId + " => " + repair.DeviceId + "]");
                if (old.StorageInventory != repair.StorageInventory) changes.Add("исп. позиция [" + old.StorageInventory + " => " + repair.StorageInventory + "]");
                if (old.Date != repair.Date) changes.Add("дата проведения [" + old.Date + " => " + repair.Date + "]");
                if (old.Number != repair.Number) changes.Add("кол-во исп. [" + old.Number + " => " + repair.Number + "]");
                if (old.IsOff != repair.IsOff) changes.Add("списан [" + old.IsOff + " => " + repair.IsOff + "]");
                if (old.IsVirtual != repair.IsVirtual) changes.Add("виртуальный [" + old.IsVirtual + " => " + repair.IsVirtual + "]");

                if (changes.Count > 0)
                {
                    // Сохранение в базе
                    conn.Execute(@"UPDATE Repairs SET 
                        DeviceId          = @DeviceId
                        ,StorageInventory = @StorageInventory
                        ,Date             = @Date
                        ,Number           = @Number
                        ,IsOff            = @IsOff
                        ,IsVirtual        = @IsVirtual
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
                        conn.Execute("UPDATE Storages SET Noff = Noff - @Number WHERE Inventory = @StorageInventory", repair);
                    }
                    else
                    {
                        conn.Execute("UPDATE Storages SET Nstorage = Nstorage + @Number, Noff = Noff - @Number WHERE Inventory = @StorageInventory", repair);
                    }
                }
                else
                {
                    if (repair.IsVirtual)
                    {
                        conn.Execute("UPDATE Storages SET Nrepairs = Nrepairs - @Number WHERE Inventory = @StorageInventory", repair);
                    }
                    else
                    {
                        conn.Execute("UPDATE Storages SET Nstorage = Nstorage + @Number, Nrepairs = Nrepairs - @Number WHERE Inventory = @StorageInventory", repair);
                    }
                }
                conn.Execute("INSERT INTO Activity (Date, Source, Id, Text, Username) VALUES (@Date, @Source, @Id, @Text, @Username)", new Activity
                {
                    Date = DateTime.Now,
                    Source = "storages",
                    Id = repair.StorageInventory.ToString(),
                    Text = "Отменен ремонт #" + Id + ". Исп. кол-во возвращено",
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

        public JsonResult Move(string repairs, string key)
        {
            using (var conn = Database.Connection())
            {
                int WriteoffId = int.TryParse(key.Replace("w", ""), out int i) ? i : 0;
                int FolderId = int.TryParse(key.Replace("g", ""), out i) ? i : 0;

                string[] Repairs = repairs.Split(new string[] { ";;" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var repair in Repairs)
                {
                    int Id = int.TryParse(repair, out i) ? i : 0;
                    
                    if (Id != 0)
                    {
                        conn.Execute("UPDATE Repairs SET WriteoffId = @WriteoffId, FolderId = @FolderId WHERE Id = @Id", new { WriteoffId, FolderId, Id });
                        string text = key == "0" 
                            ? "Ремонт [repair" + Id + "] размещен отдельно" 
                            : WriteoffId != 0 
                                ? "Ремонт [repair" + Id + "] перемещен в списание [off" + WriteoffId + "]"
                                : "Ремонт [repair" + Id + "] перемещен в группу [group" + FolderId + "]";

                        conn.Execute("INSERT INTO Activity (Date, Source, Id, Text, Username) VALUES (@Date, @Source, @Id, @Text, @Username)", new Activity
                        {
                            Date = DateTime.Now,
                            Source = "storages",
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
                    conn.Execute(@"
                        UPDATE Storages SET Noff = Noff + @Number, Nrepairs = Nrepairs - @Number WHERE StorageInventory = @StorageInventory;
                        UPDATE Repairs SET IsOff = 1 WHERE Id = @Id;
                        INSERT INTO Activity (Date, Source, Id, Text, Username) VALUES (GetDate(), 'storages', @StorageInventory, 'Обновлена позиция [' + @StorageInventory + '] при переводе ремонта [repair' + CAST(@Id AS varchar(10)) + '] в списанные: ' + CAST(@Number AS varchar(10)) + ' шт. деталей перемещены из используемых в списанные', @Author);
                        INSERT INTO Activity (Date, Source, Id, Text, Username) VALUES (GetDate(), 'repairs', @Id, 'Ремонт [repair' + CAST(@Id AS varchar(10)) + '] помечен как списанный', @Author);", repair);
                }
            }

            return Json(new { Good = "Все ремонты отмечены как списанные, позиции возвращены на склад" });
        }

        public JsonResult OffSelected(string repairs)
        {
            string[] Repairs = repairs.Split(new string[] { ";;" }, StringSplitOptions.RemoveEmptyEntries);
            using (var conn = Database.Connection())
            {
                foreach (string r in Repairs)
                {
                    int Id = int.TryParse(r, out int i) ? i : 0;
                    if (Id != 0)
                    {
                        var repair = conn.Query<Repair>("SELECT Id, Number, StorageInventory, Author = @Author FROM Repairs WHERE Id = @Id AND IsOff <> 1", new { Id, Author = User.Identity.Name }).FirstOrDefault();
                        if (repair != null)
                        {
                            conn.Execute("UPDATE Storages SET Noff = Noff + @Number, Nrepairs = Nrepairs - @Number WHERE Inventory = @StorageInventory", repair);
                            conn.Execute("UPDATE Repairs SET IsOff = 1 WHERE Id = @Id", repair);
                            conn.Execute("INSERT INTO Activity (Date, Source, Id, Text, Username) VALUES (GetDate(), 'storages', @StorageInventory, @Text, @Author)", new {
                                repair.StorageInventory,
                                repair.Author,
                                Text = "Обновлена позиция [" + repair.StorageInventory + "] при переводе ремонта [repair" + repair.Id + "] в списанные: " + repair.Number + " шт. деталей перемещены из используемых в списанные"
                            });
                            conn.Execute("INSERT INTO Activity (Date, Source, Id, Text, Username) VALUES (GetDate(), 'storages', @StorageInventory, @Text, @Author)", new
                            {
                                repair.StorageInventory,
                                repair.Author,
                                Text = "Ремонт [repair" + repair.Id + "] помечен как списанный"
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
                var repairs = conn.Query<Repair>("SELECT Id, Number, StorageInventory, Author = @Author FROM Repairs WHERE WriteoffId = @Id AND IsOff <> 0", new { Id, Author = User.Identity.Name });

                foreach (var repair in repairs)
                {
                    conn.Execute(@"
                        UPDATE Storages SET Noff = Noff - @Number, Nrepairs = Nrepairs + @Number WHERE StorageInventory = @StorageInventory;
                        UPDATE Repairs SET IsOff = 0 WHERE Id = @Id;
                        INSERT INTO Activity (Date, Source, Id, Text, Username) VALUES (GetDate(), 'storages', @StorageInventory, 'Обновлена позиция [' + @StorageInventory + '] при переводе ремонта [repair' + CAST(@Id AS varchar(10)) + '] в списанные: ' + CAST(@Number AS varchar(10)) + ' шт. деталей перемещены из списанных в используемые', @Author);
                        INSERT INTO Activity (Date, Source, Id, Text, Username) VALUES (GetDate(), 'repairs', @Id, 'Ремонт [repair' + CAST(@Id AS varchar(10)) + '] помечен как активный', @Author);", repair);
                }
            }

            return Json(new { Good = "Все ремонты отмечены как активные, позиции забраны со склада" });
        }

        public JsonResult OnSelected(string repairs)
        {
            string[] Repairs = repairs.Split(new string[] { ";;" }, StringSplitOptions.RemoveEmptyEntries);
            using (var conn = Database.Connection())
            {
                foreach (string r in Repairs)
                {
                    int Id = int.TryParse(r, out int i) ? i : 0;
                    if (Id != 0)
                    {
                        var repair = conn.Query<Repair>("SELECT Id, Number, StorageInventory, Author = @Author FROM Repairs WHERE Id = @Id AND IsOff <> 0", new { Id, Author = User.Identity.Name }).FirstOrDefault();
                        if (repair != null)
                        {
                            conn.Execute("UPDATE Storages SET Noff = Noff - @Number, Nrepairs = Nrepairs + @Number WHERE Inventory = @StorageInventory", repair);
                            conn.Execute("UPDATE Repairs SET IsOff = 0 WHERE Id = @Id", repair);
                            conn.Execute("INSERT INTO Activity (Date, Source, Id, Text, Username) VALUES (GetDate(), 'storages', @StorageInventory, @Text, @Author)", new
                            {
                                repair.StorageInventory,
                                repair.Author,
                                Text = "Обновлена позиция [" + repair.StorageInventory + "] при переводе ремонта [repair" + repair.Id + "] в активное состояние: " + repair.Number + " шт. деталей из списанных в используемые"
                            });
                            conn.Execute("INSERT INTO Activity (Date, Source, Id, Text, Username) VALUES (GetDate(), 'storages', @StorageInventory, @Text, @Author)", new
                            {
                                repair.StorageInventory,
                                repair.Author,
                                Text = "Ремонт [repair" + repair.Id + "] помечен как активный"
                            });
                        }
                    }
                }
            }

            return Json(new { Good = "Все ремонты отмечены как активные, позиции забраны со склада" });
        }
    }
}