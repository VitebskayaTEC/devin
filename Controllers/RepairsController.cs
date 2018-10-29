using Dapper;
using Devin.Models;
using System;
using System.Collections.Generic;
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
    }
}