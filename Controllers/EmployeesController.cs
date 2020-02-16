using Devin.Models;
using LinqToDB;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Devin.Controllers
{
	public class EmployeesController : Controller
    {
		public ActionResult Index() => View();

		public ActionResult Create() => View();

		public ActionResult Details(int Id) => View(model: Id);

		[HttpPost]
		public ActionResult Create(string Surname, string Initials, string Division, bool OnWork)
		{
			try
			{
				if (string.IsNullOrEmpty(Surname)) return Json(new { Error = "Фамилия сотрудника не может быть пустой" });
				if (string.IsNullOrEmpty(Initials)) return Json(new { Error = "Не указаны инициалы сотрудника" });
				if (string.IsNullOrEmpty(Division)) return Json(new { Error = "Не указано подразделение" });

				using (var db = new DevinContext())
				{
					var id = db.Employees
						.Value(e => e.Division, Division)
						.Value(e => e.Initials, Initials)
						.Value(e => e.Surname, Surname)
						.Value(e => e.OnWork, OnWork)
						.InsertWithInt32Identity();

					if (id.HasValue) return Json(new { Done = "Сотрудник успешно добавлен" });

					return Json(new { Error = "Ошибка базы данных, сотрудник не добавлен" });
				}
			}
			catch (Exception e)
			{
				return Json(new { Error = e.Message });
			}
		}

		[HttpPost]
		public ActionResult Update(int Id, string Surname, string Initials, string Division, bool OnWork)
		{
			try
			{
				if (string.IsNullOrEmpty(Surname)) return Json(new { Error = "Фамилия сотрудника не может быть пустой" });
				if (string.IsNullOrEmpty(Initials)) return Json(new { Error = "Не указаны инициалы сотрудника" });
				if (string.IsNullOrEmpty(Division)) return Json(new { Error = "Не указано подразделение" });

				using (var db = new DevinContext())
				{
					var id = db.Employees
						.Where(x => x.Id == Id)
						.Set(e => e.Division, Division)
						.Set(e => e.Initials, Initials)
						.Set(e => e.Surname, Surname)
						.Set(e => e.OnWork, OnWork)
						.Update();

					if (id > 0) return Json(new { Done = "Сотрудник успешно обновлён" });

					return Json(new { Error = "Ошибка базы данных, сотрудник не обновлён" });
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
					var id = db.Employees
						.Where(x => x.Id == Id)
						.Delete();

					if (id > 0) return Json(new { Done = "Сотрудник успешно удалён" });

					return Json(new { Error = "Ошибка базы данных, сотрудник не удалён" });
				}
			}
			catch (Exception e)
			{
				return Json(new { Error = e.Message });
			}
		}
	}
}