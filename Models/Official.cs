using LinqToDB;
using LinqToDB.Mapping;
using System.Collections.Generic;
using System.Linq;

namespace Devin.Models
{
	[Table(Name = "Officials")]
	public class Official
	{
		[Column, PrimaryKey, Identity]
		public int Id { get; set; }

		[Column]
		public string Name { get; set; }

		[Column]
		public string Title { get; set; }

		[Column]
		public string ReplaceTitle { get; set; }

		[Column]
		public string Division { get; set; }

		public static Dictionary<string, Employee> Load()
		{
			using (var db = new DevinContext())
			{
				// дальше идёт невъебенная магия
				// определяем имеющиеся на работе должностные лица
				var _officials = from o in db.Officials
								 from r in db.Relation_Officials_Employees.InnerJoin(x => x.OfficialId == o.Id)
								 from e in db.Employees.InnerJoin(x => x.Id == r.EmployeeId)
								 where e.OnWork
								 orderby o.Id, r.Weight descending
								 select new
								 {
									 o.Name,
									 o.Title,
									 o.ReplaceTitle,
									 r.Weight,
									 e.Id,
									 e.Surname,
									 e.Initials,
									 e.Division,
								 };

				var officials = _officials
					.ToList()
					.GroupBy(x => new { x.Name, x.Title, x.ReplaceTitle })
					.Select(g => new
					{
						g.Key.Name,
						g.Key.Title,
						g.Key.ReplaceTitle,
						Employee = g
							.OrderByDescending(x => x.Weight)
							.Select(x => new
							{
								x.Weight,
								x.Id,
								x.Surname,
								x.Initials,
								x.Division,
							})
							.First()
					});

				var people = new Dictionary<string, Employee>();

				foreach (var o in officials)
				{
					people.Add(o.Name, new Employee
					{
						Surname = o.Employee.Surname,
						Initials = o.Employee.Initials,
						Division = o.Employee.Division,
						Title = o.Employee.Weight == 10 ? o.Title : o.ReplaceTitle,
					});
				}

				return people;
			}
		}
	}
}