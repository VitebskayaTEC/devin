using LinqToDB.Mapping;

namespace Devin.Models
{
	[Table(Name = "Employees")]
	public class Employee
	{
		[Column, PrimaryKey, Identity]
		public int Id { get; set; }

		[Column]
		public string Division { get; set; }

		[Column]
		public string Surname { get; set; }

		[Column]
		public string Initials { get; set; }

		[Column]
		public bool OnWork { get; set; }

		public string Title { get; set; }
	}
}