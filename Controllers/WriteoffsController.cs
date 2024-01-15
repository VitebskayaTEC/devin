using Devin.Models;
using LinqToDB;
using Newtonsoft.Json;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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


				string output = writeoff.Name + " " + DateTime.Now.ToLongDateString();

				/* Ремонт основного средства */
				if (writeoff.Type == "mat")
				{
					IWorkbook book;
					ISheet sheet;

					using (var fs = new FileStream(Server.MapPath(Url.Action("templates", "content") + "\\writeoff\\mat.xlsx"), FileMode.Open, FileAccess.Read))
					{
						book = new XSSFWorkbook(fs);
					}

					// переходим на лист "Сводная таблица"
					sheet = book.GetSheet("Сводная");

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

						sheet.GetRow(6).GetCell(6).SetCellValue(metals.FirstOrDefault(x => x.Name == "Золото")?.Cost ?? 0F);
						sheet.GetRow(7).GetCell(6).SetCellValue(metals.FirstOrDefault(x => x.Name == "Серебро")?.Cost ?? 0F);
						sheet.GetRow(8).GetCell(6).SetCellValue(metals.FirstOrDefault(x => x.Name == "Платина")?.Cost ?? 0F);
						sheet.GetRow(9).GetCell(6).SetCellValue(metals.FirstOrDefault(x => x.Name == "Палладий")?.Cost ?? 0F); // МПГ
						sheet.GetRow(10).GetCell(6).SetCellValue(metals.FirstOrDefault(x => x.Name == "Палладий")?.Cost ?? 0F);

						var metals_date = site.MetalsCosts
							.OrderByDescending(x => x.Date)
							.Select(x => x.Date)
							.DefaultIfEmpty(DateTime.MinValue)
							.FirstOrDefault();

						sheet.GetRow(5).GetCell(6).SetCellValue(metals_date.ToString("dd.MM.yyyy"));
					}

					// даты
					var now = DateTime.Now;
					sheet.GetRow(2).GetCell(2).SetCellValue(now.ToString("yyyy г."));
					sheet.GetRow(8).GetCell(2).SetCellValue("\"" + now.Day + "\" " + months2[now.Month - 1] + " " + now.ToString("yyyy г."));
					sheet.GetRow(9).GetCell(2).SetCellValue(now.ToString("dd.MM.yyyy г."));
					sheet.GetRow(10).GetCell(2).SetCellValue(months[now.Month - 1] + " " + now.ToString("yyyy г."));

					// номера актов
					string number = now.Month + "/" + now.Day + "-" + Id;
					sheet.GetRow(7).GetCell(2).SetCellValue(number);

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

					int i = 22;
					foreach (var r in repairs)
					{
						sheet.GetRow(i).GetCell(3).SetCellValue(r.Number);
						sheet.GetRow(i).GetCell(4).SetCellValue("шт");
						sheet.GetRow(i).GetCell(7).SetCellValue(r.Name);
						//sheet.GetRow(i).GetCell(8).SetCellValue("текущий ремонт");
						sheet.GetRow(i).GetCell(12).SetCellValue(r.Cost);
						sheet.GetRow(i).GetCell(13).SetCellValue(r.Inventory);
						Debug.WriteLine(JsonConvert.SerializeObject(r));
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

					if (device != null)
					{
						sheet.GetRow(12).GetCell(2).SetCellValue(device.Description);
						sheet.GetRow(13).GetCell(2).SetCellValue(device.Inventory);
						sheet.GetRow(14).GetCell(2).SetCellValue(device.SerialNumber);

						sheet.GetRow(6).GetCell(8).SetCellValue(device.Gold);
						sheet.GetRow(7).GetCell(8).SetCellValue(device.Silver);
						sheet.GetRow(8).GetCell(8).SetCellValue(device.Platinum);
						sheet.GetRow(9).GetCell(8).SetCellValue(device.Mpg);
						sheet.GetRow(10).GetCell(8).SetCellValue(device.Palladium);
					}

					// кол-во плат и вес из карточки
					sheet.GetRow(6).GetCell(4).SetCellValue(writeoff.BoardsCount ?? 0);
					sheet.GetRow(10).GetCell(4).SetCellValue(writeoff.BoardsWeight ?? 0F);

					// заставляем сделать пересчёт всех формул и ссылок, чтобы акты взяли новые данные из сводной таблицы 
					sheet.ForceFormulaRecalculation = true;

					// сохранение
					output = writeoff.Name.Replace("\"", "") + " " + DateTime.Now.ToLongDateString() + ".xlsx";
					using (var fs = new FileStream(Server.MapPath(Url.Action("excels", "content")) + "\\" + output, FileMode.OpenOrCreate, FileAccess.Write))
					{
						book.Write(fs);
					}
				}

				return Json(new
				{
					Good = "Файл Excel списания успешно создан",
					Link = Url.Action("excels", "content") + "/" + output,
					Name = output,
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