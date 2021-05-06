using Devin.Models;
using LinqToDB;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace Devin.Controllers
{
    public class WriteoffsController : Controller
    {
        public ActionResult Cart(int Id) => View(model: Id);

        public ActionResult History(string Id) => View(model: Id);

		[HttpPost]
		public ActionResult Create()
        {
			try
			{
				using (var db = new DevinContext())
				{
					var id = db.Writeoffs
						.Value(x => x.Name, "Новое списание")
						.Value(x => x.Date, DateTime.Now)
						.Value(x => x.Type, "expl")
						.Value(x => x.FolderId, 0)
						.Value(x => x.CostArticle, 0)
						.InsertWithInt32Identity();

					if (id.HasValue)
					{
						db.Writeoffs
							.Where(x => x.Id == id)
							.Set(x => x.Name, x => x.Name + " #" + id)
							.Update();

						db.Log(User, "writeoffs", id, "Создано списание");

						return Json(new { Id = "off" + id, Good = "Создано новое списание" });
					}
					else
					{
						return Json(new { Error = "Ошибка в базе данных, списание не создано" });
					}
				}
			}
			catch (Exception e)
			{
				return Json(new { Error = e.Message });
			}
        }

		[HttpPost]
		public JsonResult Update(
            [Bind(Include = "Id,Name,Type,Description,CostArticle,FolderId")] Writeoff writeoff,
            string Date,
			string boardsWeight, string boardsCount
        )
        {
			try
			{
				writeoff.Date = DateTime.Parse(Date);
			}
			catch
			{
				return Json(new { Error = "Дата введена в неверном формате. Ожидается формат \"дд.ММ.гггг чч:мм\"" });
			}

            using (var db = new DevinContext())
            {
                var old = db.Writeoffs.Where(x => x.Id == writeoff.Id).FirstOrDefault();

                var changes = new List<string>();
                if (writeoff.Name != old.Name) changes.Add("наименование [\"" + old.Name + "\" => \"" + writeoff.Name + "\"]");
                if (writeoff.Type != old.Type) changes.Add("тип [\"" + old.Type + "\" => \"" + writeoff.Type + "\"]");
                if ((writeoff.Description ?? "") != (old.Description ?? "")) changes.Add("описание [\"" + old.Description + "\" => \"" + writeoff.Description + "\"]");
                if (writeoff.CostArticle != old.CostArticle) changes.Add("статья расходов [\"" + old.CostArticle + "\" => \"" + writeoff.CostArticle + "\"]");
                if (writeoff.FolderId != old.FolderId) changes.Add("папка [\"" + (old.FolderId == 0 ? "отдельно" : ("folder" + old.FolderId)) + "\" => \"" + (writeoff.FolderId == 0 ? "отдельно" : ("folder" + writeoff.FolderId)) + "\"]");
                if (writeoff.Date != old.Date) changes.Add("дата создания [\"" + old.Date + "\" => \"" + writeoff.Date + "\"]");

				if (old.Type == "mat")
				{
					try
					{
						writeoff.BoardsCount = Math.Abs(int.Parse(boardsCount));
					}
					catch
					{
						return Json(new { Error = "Количество плат должно быть целым неотрицательным числом" });
					}

					try
					{
						writeoff.BoardsWeight = Math.Abs(float.Parse(boardsWeight.Replace('.', ',')));
					}
					catch
					{
						return Json(new { Error = "Вес плат должно быть вещественным неотрицательным числом" });
					}

					if (writeoff.BoardsCount != old.BoardsCount) changes.Add("кол-во плат [\"" + old.BoardsCount + "\" => \"" + writeoff.BoardsCount + "\"]");
					if (writeoff.BoardsWeight != old.BoardsWeight) changes.Add("вес плат [\"" + old.BoardsWeight + "\" => \"" + writeoff.BoardsWeight + "\"]");
				}

                if (changes.Count > 0)
                {
                    db.Writeoffs
                        .Where(x => x.Id == writeoff.Id)
                        .Set(x => x.Name, writeoff.Name)
                        .Set(x => x.Type, writeoff.Type)
                        .Set(x => x.Description, writeoff.Description)
                        .Set(x => x.CostArticle, writeoff.CostArticle)
                        .Set(x => x.FolderId, writeoff.FolderId)
                        .Set(x => x.Date, writeoff.Date)
                        .Update();

					if (old.Type == "mat")
					{
						db.Writeoffs
							.Where(x => x.Id == writeoff.Id)
							.Set(x => x.BoardsCount, writeoff.BoardsCount)
							.Set(x => x.BoardsWeight, writeoff.BoardsWeight)
							.Update();
					}

                    db.Log(User, "writeoffs", writeoff.Id, "Списание обновлено. Изменения: " + changes.ToLog());

                    return Json(new { Good = "Списание обновлено. Изменения:<br />" + changes.ToHtml() });
                }
                else
                {
                    return Json(new { Warning = "Изменений не было" });
                }
            }
        }

        public JsonResult Move(int Id, int FolderId)
        {
            using (var db = new DevinContext())
            {
                string folder = db.Folders.Where(x => x.Id == FolderId).Select(x => x.Name).FirstOrDefault() ?? "<не определено>";

                db.Writeoffs
                    .Where(x => x.Id == Id)
                    .Set(x => x.FolderId, FolderId)
                    .Update();
                db.Log(User, "writeoffs", Id, "Списание перемещено в папку \"" + folder + "\" [folder" + FolderId + "]");

                return Json(new { Good = "Списание перемещено" });
            }
        }

        public JsonResult Delete(string Id)
        {
            int id = int.TryParse(Id.Replace("off", ""), out int i) ? i : 0;

            using (var db = new DevinContext())
            {
                db.Writeoffs.Delete(x => x.Id == id);
                db.Log(User, "writeoffs", id, "Списание удалено без отмены вложенных ремонтов");
            }

            return Json(new { Good = "Списание удалено без отмены вложенных ремонтов" });
        }

        public JsonResult Print(int Id)
        {
			var officials = Official.Load();

			string[] months = { "январе", "феврале", "марте", "апреле", "мае", "июне", "июле", "августе", "сентябре", "октябре", "ноябре", "декабре" };
			string[] months2 = { "января", "февраля", "марта", "апреля", "мая", "июня", "июля", "августа", "сентября", "октября", "ноября", "декабря" };

			using (var db = new DevinContext())
            {
                var query = from w in db.Writeoffs
                            from t in db._WriteoffTypes.InnerJoin(x => x.Id == w.Type)
                            where w.Id == Id
                            select new
                            {
                                w.Name,
                                w.Date,
                                w.Params,
                                w.Type,
                                w.CostArticle,
								w.BoardsCount,
								w.BoardsWeight,
                                t.Template,
                                t.DefaultData
                            };

                var writeoff = query.FirstOrDefault();

				string template = Server.MapPath(Url.Action("templates", "content")) + "\\" + writeoff.Type + ".xls";
                string output = writeoff.Name + " " + DateTime.Now.ToLongDateString() + ".xls";

                IWorkbook book;
                ISheet sheet;

                int step = 0;
				string name = writeoff.Type + " " + DateTime.Today.ToString("MMMM yyyy г");

				/* Эксплуатационные расходы */
				if (writeoff.Type == "expl")
				{
					name = "Эксплуатационные расходы " + DateTime.Today.ToString("MMMM yyyy г");

					if (!System.IO.File.Exists(template))
					{
						return Json(new { Error = "Файла шаблона не существует либо путь к нему неправильно прописан в исходниках<br />Путь: " + template });
					}

					using (var fs = new FileStream(template, FileMode.Open, FileAccess.Read))
					{
						book = new HSSFWorkbook(fs);
					}

                    sheet = book.GetSheetAt(0);
                    sheet.GetRow(13).GetCell(0).SetCellValue(sheet.GetRow(13).GetCell(0).StringCellValue.Replace("@data", months[writeoff.Date.Month - 1] + " " + writeoff.Date.Year + " г."));

                    try
                    {
                        var _query = from r in db.Repairs
                                     from d in db.Devices.Where(x => x.Id == r.DeviceId).DefaultIfEmpty()
                                     from s in db.Storages.Where(x => x.Id == r.StorageId).DefaultIfEmpty()
                                     where r.WriteoffId == Id
                                     orderby s.Inventory, d.Inventory, r.Date
                                     select new
                                     {
                                         r.Id,
                                         r.Number,
                                         Device = new Device
                                         {
                                             Id = d.Id,
                                             Inventory = d.Inventory
                                         },
                                         StorageId = s.Id,
                                         StorageInventory = s.Inventory,
                                         StorageName = s.Name,
                                         StorageCost = s.Cost,
                                         StorageAccount = s.Account
                                     };

                        var groups = _query
                            .ToList()
                            .GroupBy(x => new
                            {
                                x.StorageId,
                                x.StorageInventory,
                                x.StorageName,
                                x.StorageCost,
                                x.StorageAccount
                            })
                            .Select(x => new
                            {
                                Storage = new
                                {
                                    Id = x.Key.StorageId,
                                    Inventory = x.Key.StorageInventory,
                                    Name = x.Key.StorageName,
                                    Cost = x.Key.StorageCost,
                                    Account = x.Key.StorageAccount,
                                },
                                Repairs = x.Select(y => new
                                {
                                    y.Id,
                                    y.Number,
                                    y.Device,
                                })
                            })
                            .ToList();

						float sum = 0;
						foreach (var g in groups)
                        {
                            int initialStep = step;

                            foreach (var r in g.Repairs)
                            {
                                if (r.Device.Inventory.Contains("***") || r.Device.Inventory.Contains("xxx"))
                                {
                                    r.Device.Inventory = "Эксплуатационные нужды";
                                }
                                else
                                {
                                    r.Device.Inventory = "Установлен в: инв. № " + r.Device.Inventory;
                                }

                                IRow row = sheet.CopyRow(20, 21 + step);
                                row.GetCell(0).SetCellValue(step + 1);
                                row.GetCell(1).SetCellValue(g.Storage.Inventory);
                                row.GetCell(2).SetCellValue(g.Storage.Name + "; счет " + g.Storage.Account);
                                row.GetCell(3).SetCellValue("шт.");
                                row.GetCell(4).SetCellValue(r.Number);
                                row.GetCell(5).SetCellValue(g.Storage.Cost.ToString("0.00").Replace('.', ','));
                                row.GetCell(6).SetCellValue((g.Storage.Cost * r.Number).ToString("0.00").Replace('.', ','));
                                row.GetCell(7).SetCellValue(r.Device.Inventory);
                                row.Height = -1;

                                step++;
                                sum += g.Storage.Cost * r.Number;
                            }

                            sheet.AddMergedRegion(new CellRangeAddress(21 + initialStep, 21 + step - 1, 1, 1));
                            sheet.AddMergedRegion(new CellRangeAddress(21 + initialStep, 21 + step - 1, 2, 2));
                            sheet.AddMergedRegion(new CellRangeAddress(21 + initialStep, 21 + step - 1, 3, 3));
                        }

						sheet.GetRow(21 + step).GetCell(6).SetCellValue(sum);

						HSSFFormulaEvaluator.EvaluateAllFormulaCells(book);
					}
                    catch (Exception)
                    {
                        return Json(new { Error = "Хуйня с запросом" });
                    }

					output = output.Replace("\"", "");
					using (var fs = new FileStream(Server.MapPath(Url.Action("excels", "content")) + "\\" + output, FileMode.OpenOrCreate, FileAccess.Write))
					{
						book.Write(fs);
					}

					//foreach (Repair r in repairs)
					//{
					//    if (r.Device.Inventory.Contains("***") || r.Device.Inventory.Contains("xxx"))
					//    {
					//        r.Device.Inventory = "Эксплуатационные нужды";
					//    }
					//    else
					//    {
					//        r.Device.Inventory = "Установлен в: инв. № " + r.Device.Inventory;
					//    }

					//    IRow row = sheet.CopyRow(20, 21 + step);
					//    row.GetCell(0).SetCellValue(step + 1);
					//    row.GetCell(1).SetCellValue(r.Storage.Inventory);
					//    row.GetCell(2).SetCellValue(r.Storage.Name + "; счет " + r.Storage.Account);
					//    row.GetCell(3).SetCellValue("шт.");
					//    row.GetCell(4).SetCellValue(r.Number);
					//    row.GetCell(5).SetCellValue(r.Storage.Cost.ToString("0.00").Replace('.', ','));
					//    row.GetCell(6).SetCellValue((r.Storage.Cost * r.Number).ToString("0.00").Replace('.', ','));
					//    row.GetCell(7).SetCellValue(r.Device.Inventory);
					//    row.Height = -1;

					//    step++;
					//    sum += r.Storage.Cost * r.Number;
					//}

					sheet.GetRow(20).ZeroHeight = true;

                    

                    sheet.GetRow(40 + step).GetCell(0).SetCellValue("Акт составлен " + writeoff.Date.ToString("d MMMM yyyy"));
                    sheet.GetRow(42 + step).GetCell(3).SetCellValue(officials["Начальник уАСУТП"].Title);
                    sheet.GetRow(42 + step).GetCell(6).SetCellValue(officials["Начальник уАСУТП"].Initials + " " + officials["Начальник уАСУТП"].Surname);
                }


                /* Эксплуатационные расходы (прочие) */
                if (writeoff.Type == "expl-1")
				{
					name = "Эксплуатационные расходы " + DateTime.Today.ToString("MMMM yyyy г");

					using (var fs = new FileStream(template, FileMode.Open, FileAccess.Read))
					{
						book = new HSSFWorkbook(fs);
					}

                    sheet = book.GetSheetAt(0);
					sheet.GetRow(13).GetCell(0).SetCellValue(sheet.GetRow(13).GetCell(0).StringCellValue.Replace("@data", months[writeoff.Date.Month - 1] + " " + writeoff.Date.Year + " г."));

					try
                    {
                        var repairsQuery = from r in db.Repairs
                                           from d in db.Devices.Where(x => x.Id == r.DeviceId).DefaultIfEmpty()
                                           from s in db.Storages.Where(x => x.Id == r.StorageId).DefaultIfEmpty()
                                           where r.WriteoffId == Id
                                           select new Repair
                                           {
                                               Id = r.Id,
                                               Number = r.Number,
                                               Device = new Device
                                               {
                                                   Id = d.Id,
                                                   Inventory = d.Inventory
                                               },
                                               Storage = new Storage
                                               {
                                                   Id = s.Id,
                                                   Inventory = s.Inventory,
                                                   Name = s.Name,
                                                   Cost = s.Cost,
                                                   Account = s.Account
                                               }
                                           };

                        var repairs = repairsQuery.ToList();
                    
						float sum = 0;
						foreach (Repair r in repairs)
						{
							string s = "";
							if (r.Device.Inventory.Contains("***") || r.Device.Inventory.Contains("xxx"))
							{
								s = "Эксплуатационные нужды";
							}
							else
							{
								s = "Установлен в: инв. № " + r.Device.Inventory;
							}

							IRow row = sheet.CopyRow(20, 21 + step);
							row.GetCell(0).SetCellValue(step + 1);
							row.GetCell(1).SetCellValue(r.Storage.Inventory);
							row.GetCell(2).SetCellValue(r.Storage.Name + "; счет " + r.Storage.Account);
							row.GetCell(3).SetCellValue("шт.");
							row.GetCell(4).SetCellValue(r.Number);
							row.GetCell(5).SetCellValue(r.Storage.Cost.ToString("0.00").Replace('.', ','));
							row.GetCell(6).SetCellValue((r.Storage.Cost * r.Number).ToString("0.00").Replace('.', ','));
							row.GetCell(7).SetCellValue(s);
							row.Height = -1;

							step++;
							sum += r.Storage.Cost * r.Number;
						}

						sheet.GetRow(20).ZeroHeight = true;

						sheet.GetRow(21 + step).GetCell(6).SetCellValue(sum);

						sheet.GetRow(44 + step).GetCell(0).SetCellValue("Акт составлен " + writeoff.Date.ToString("d MMMM yyyy"));
                        sheet.GetRow(42 + step).GetCell(3).SetCellValue(officials["Начальник уАСУТП"].Title);
                        sheet.GetRow(42 + step).GetCell(6).SetCellValue(officials["Начальник уАСУТП"].Initials + " " + officials["Начальник уАСУТП"].Surname);

                        HSSFFormulaEvaluator.EvaluateAllFormulaCells(book);
					}
					catch (Exception)
					{
						return Json(new { Error = "Хуйня с запросом" });
					}

					output = output.Replace("\"", "");
					using (var fs = new FileStream(Server.MapPath(Url.Action("excels", "content")) + "\\" + output, FileMode.OpenOrCreate, FileAccess.Write))
					{
						book.Write(fs);
					}
				}


                /* Ремонт основного средства */
                if (writeoff.Type == "mat")
                {
					template = Server.MapPath(Url.Action("templates", "content")) + "\\mat.xls";

					using (var fs = new FileStream(template, FileMode.Open, FileAccess.Read))
					{
						book = new HSSFWorkbook(fs);
					}

					// переходим на лист "Сводная таблица"
					sheet = book.GetSheet("Сводная таблица");

					// стоимость драгметаллов из 1С
					using (var site = new SiteContext())
					{
						var metals = site.MetalsCosts
							.ToList()
							.GroupBy(x => x.Name)
							.Select(g => new
							{
								Name = g.Key.Trim(),
								Cost = g
									.OrderByDescending(x => x.Date)
									.Select(x => x.Cost / 100 * (100 - x.Discount))
									.First()
							})
							.ToList();

						sheet.GetRow(10).GetCell(16).SetCellValue(metals.FirstOrDefault(x => x.Name == "Золото")?.Cost ?? 0F);
						sheet.GetRow(11).GetCell(16).SetCellValue(metals.FirstOrDefault(x => x.Name == "Серебро")?.Cost ?? 0F);
						sheet.GetRow(12).GetCell(16).SetCellValue(metals.FirstOrDefault(x => x.Name == "Платина")?.Cost ?? 0F);
						sheet.GetRow(13).GetCell(16).SetCellValue(metals.FirstOrDefault(x => x.Name == "Палладий")?.Cost ?? 0F); // МПГ
						sheet.GetRow(14).GetCell(16).SetCellValue(metals.FirstOrDefault(x => x.Name == "Палладий")?.Cost ?? 0F);
					}

					// даты
					var now = DateTime.Now;
                    sheet.GetRow(27).GetCell(16).SetCellValue("\"" + now.Day + "\" " + months2[now.Month - 1] + " " + now.ToString("yyyy г."));
                    sheet.GetRow(28).GetCell(16).SetCellValue(now.ToString("dd.MM.yyyy г."));
                    sheet.GetRow(29).GetCell(16).SetCellValue(months[now.Month - 1] + " " + now.ToString("yyyy г."));
                    sheet.GetRow(28).GetCell(39).SetCellValue(now.ToString("yyyy г."));
					sheet.GetRow(28).GetCell(48).SetCellValue("\"" + now.Day + "\" " + months2[now.Month - 1] + " " + now.ToString("yyyy г."));

					// номера актов
					string number = now.Month + "/" + now.Day + "-" + Id;
					sheet.GetRow(26).GetCell(16).SetCellValue(number + "/2");
					sheet.GetRow(33).GetCell(48).SetCellValue(number + "/1");

					// информация о ремонтах (исп. деталях)
					var _repairs = from r in db.Repairs
								   from s in db.Storages.InnerJoin(x => x.Id == r.StorageId)
								   where r.WriteoffId == Id
								   select new
								   {
									   r.Number,
									   r.DeviceId,
									   s.Name,
									   s.Cost,
									   s.Inventory
								   };

					var repairs = _repairs.ToList();

					if (repairs.Count == 0)
					{
						return Json(new { Error = "В списании нет ремонтов" });
					}

					int i = 123;
					foreach (var r in repairs)
					{
						sheet.GetRow(i).GetCell(25).SetCellValue(r.Number);
						sheet.GetRow(i).GetCell(28).SetCellValue("шт");
						sheet.GetRow(i).GetCell(32).SetCellValue(r.Name);
						sheet.GetRow(i).GetCell(51).SetCellValue("текущий ремонт");
						sheet.GetRow(i).GetCell(57).SetCellValue(r.Cost);
						sheet.GetRow(i).GetCell(63).SetCellValue(r.Inventory);
						i++;
					}

					// информация об основном средстве
					var deviceId = _repairs
						.Select(x => x.DeviceId)
						.FirstOrDefault();

					var _device = from d in db.Devices
								  from o in db.Objects1C.InnerJoin(x => x.Inventory == d.Inventory)
								  where d.Id == deviceId
								  select new
								  {
									  Description = o.Description ?? d.PublicName,
									  o.Inventory,
									  d.SerialNumber,
									  o.Mol,
									  o.Gold,
									  o.Silver,
									  o.Platinum,
									  o.Mpg,
									  o.Palladium,
									  Name = "Ремонт " + d.Name + " (" + d.Inventory + ") " + DateTime.Today.ToString("MMMM yyyy г"),
								  };

					var device = _device.FirstOrDefault();

					name = device.Name;

					if (device != null)
					{
						sheet.GetRow(32).GetCell(16).SetCellValue(device.Inventory);
						sheet.GetRow(33).GetCell(16).SetCellValue(device.SerialNumber);
						sheet.GetRow(34).GetCell(16).SetCellValue(device.SerialNumber);

						sheet.GetRow(64).GetCell(59).SetCellValue(device.Gold);
						sheet.GetRow(65).GetCell(59).SetCellValue(device.Silver);
						sheet.GetRow(66).GetCell(59).SetCellValue(device.Platinum);
						sheet.GetRow(67).GetCell(59).SetCellValue(device.Mpg);
						sheet.GetRow(68).GetCell(59).SetCellValue(device.Palladium);

						string valueText = "";
						switch (device.Inventory)
						{
							case "075755": valueText = "Оборудование ПТК АСУ: "; break;
							case "075750": valueText = "Оборудование корпоративной сети: "; break;
							case "075155": valueText = "Оборудование АСКУЭ ММПГ: "; break;
						}
						sheet.GetRow(30).GetCell(16).SetCellValue(valueText + device.Description);
					}

					// кол-во плат и вес из карточки
					sheet.GetRow(34).GetCell(16).SetCellValue(writeoff.BoardsCount ?? 0);
					sheet.GetRow(35).GetCell(16).SetCellValue(writeoff.BoardsWeight ?? 0F);

					// статья расходов
                    switch (writeoff.CostArticle)
                    {
                        case 3: book.GetSheetAt(2).GetRow(14).GetCell(2).SetCellValue("ПТК АСУ"); break;
                        case 2: book.GetSheetAt(2).GetRow(14).GetCell(2).SetCellValue("Орг. техника"); break;
                        case 1: book.GetSheetAt(2).GetRow(14).GetCell(2).SetCellValue("Эксплуатационные расходы"); break;
                    }

					var mol = device.Mol.Split(' ');
					mol[0] = mol[0].Substring(0, 1).ToUpper() + mol[0].Substring(1).ToLower();

					sheet.GetRow(51).GetCell(27).SetCellValue(mol[0] + " " + mol[1]);
					sheet.GetRow(53).GetCell(27).SetCellValue(mol[1] + " " + mol[0]);

					// заставляем сделать пересчёт всех формул и ссылок, чтобы акты взяли новые данные из сводной таблицы 
					sheet.ForceFormulaRecalculation = true;
					HSSFFormulaEvaluator.EvaluateAllFormulaCells(book);

					// размер строк
					var _sheet = book.GetSheet("Ведомость потребности и мат.");
					i = 15;
					foreach (var r in repairs)
					{
						_sheet.GetRow(i).Height = -1;
						i++;
					}

					_sheet = book.GetSheet("Акт на списание материалов");
					i = 79;
					foreach (var r in repairs)
					{
						_sheet.GetRow(i).Height = -1;
						i++;
					}

					_sheet = book.GetSheet("Смета");
					i = 22;
					foreach (var r in repairs)
					{
						_sheet.GetRow(i).Height = -1;
						i++;
					}

					// сохранение
					output = writeoff.Name.Replace("\"", "") + " " + DateTime.Now.ToLongDateString() + ".xls";
					using (var fs = new FileStream(Server.MapPath(Url.Action("excels", "content")) + "\\" + output, FileMode.OpenOrCreate, FileAccess.Write))
					{
						book.Write(fs);
					}
				}


                return Json(new
				{
					Good = "Файл Excel списания успешно создан",
					Link = Url.Action("excels", "content") + "/" + output,
					Name = name,
				});
            }
        }

		public JsonResult Mark(string Id, string Mark)
		{
			int id = int.TryParse(Id.Replace("off", ""), out int i) ? i : 0;

			if (id == 0) return Json(new { Error = "Не получен Id" });

			using (var db = new DevinContext())
			{
				db.Writeoffs
					.Where(x => x.Id == id)
					.Set(x => x.Mark, Mark)
					.Update();

				db.Log(User, "writeoffs", id, "Списание отмечено цветом \"" + Mark + "\"");
			}

			return Json(new { Done = "Списание отмечено цветом \"" + Mark + "\"" });
		}
    }
}