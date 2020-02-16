using Devin.Models;
using LinqToDB;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Devin.Controllers
{
	public class OfficialsController : Controller
    {
		public ActionResult Index() => View();

		public ActionResult Create() => View();

		public ActionResult Details(int Id) => View(model: Id);

		[HttpPost]
		public ActionResult Create(string Name, string Title, string ReplaceTitle, string Division)
		{
			try
			{
				if (string.IsNullOrEmpty(Name)) return Json(new { Error = "Наименование должностного лица не может быть пустым" });
				if (string.IsNullOrEmpty(Division)) return Json(new { Error = "Не указано подразделение" });

				using (var db = new DevinContext())
				{
					var id = db.Officials
						.Value(x => x.Name, Name)
						.Value(x => x.Title, Title ?? "")
						.Value(x => x.ReplaceTitle, ReplaceTitle ?? "")
						.Value(x => x.Division, Division)
						.InsertWithInt32Identity();

					if (id.HasValue) return Json(new { Done = "Должностное лицо успешно добавлено" });

					return Json(new { Error = "Ошибка базы данных, должностное лицо не добавлено" });
				}
			}
			catch (Exception e)
			{
				return Json(new { Error = e.Message });
			}
		}

		[HttpPost]
		public ActionResult Update(int Id, string Name, string Title, string ReplaceTitle, string Division)
		{
			try
			{
				if (string.IsNullOrEmpty(Name)) return Json(new { Error = "Наименование должностного лица не может быть пустым" });
				if (string.IsNullOrEmpty(Division)) return Json(new { Error = "Не указано подразделение" });

				using (var db = new DevinContext())
				{
					var id = db.Officials
						.Where(x => x.Id == Id)
						.Set(x => x.Name, Name)
						.Set(x => x.Title, Title ?? "")
						.Set(x => x.ReplaceTitle, ReplaceTitle ?? "")
						.Set(x => x.Division, Division)
						.Update();

					if (id > 0) return Json(new { Done = "Должностное лицо успешно обновлено" });

					return Json(new { Error = "Ошибка базы данных, должностное лицо не обновлено" });
				}
			}
			catch (Exception e)
			{
				return Json(new { Error = e.Message });
			}
		}

		[HttpPost]
		public ActionResult Delete(int Id)
		{
			try
			{
				using (var db = new DevinContext())
				{
					var id = db.Officials
						.Where(x => x.Id == Id)
						.Delete();

					if (id > 0) return Json(new { Done = "Должностное лицо успешно удалено" });

					return Json(new { Error = "Ошибка базы данных, должностное лицо не удалено" });
				}
			}
			catch (Exception e)
			{
				return Json(new { Error = e.Message });
			}
		}

		public ActionResult Employees(int Id) => View(model: Id);

		[HttpPost]
		public ActionResult CreateRelation(int Id, int EmployeeId, int Weight)
		{
			try
			{
				using (var db = new DevinContext())
				{
					var id = db.Relation_Officials_Employees
						.Value(x => x.OfficialId, Id)
						.Value(x => x.EmployeeId, EmployeeId)
						.Value(x => x.Weight, Weight)
						.InsertWithInt32Identity();

					if (id.HasValue) return Json(new { Done = "Связь добавлена" });

					return Json(new { Error = "Ошибка базы данных, связь не добавлена" });
				}
			}
			catch (Exception e)
			{
				return Json(new { Error = e.Message });
			}
		}

		[HttpPost]
		public ActionResult UpdateRelation(int Id, int EmployeeId, int Weight)
		{
			try
			{
				using (var db = new DevinContext())
				{
					var id = db.Relation_Officials_Employees
						.Where(x => x.Id == Id)
						.Set(x => x.EmployeeId, EmployeeId)
						.Set(x => x.Weight, Weight)
						.Update();

					if (id > 0) return Json(new { Done = "Связь обновлена" });

					return Json(new { Error = "Ошибка базы данных, связь не обновлена" });
				}
			}
			catch (Exception e)
			{
				return Json(new { Error = e.Message });
			}
		}

		[HttpPost]
		public ActionResult RemoveRelation(int Id)
		{
			try
			{
				using (var db = new DevinContext())
				{
					var id = db.Relation_Officials_Employees
						.Where(x => x.Id == Id)
						.Delete();

					if (id > 0) return Json(new { Done = "Связь удалена" });

					return Json(new { Error = "Ошибка базы данных, связь не удалена" });
				}
			}
			catch (Exception e)
			{
				return Json(new { Error = e.Message });
			}
		}
	}
}