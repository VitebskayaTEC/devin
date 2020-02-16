using LinqToDB.Mapping;
using System;

namespace Devin.Models
{
	public class Software
	{
		[PrimaryKey, Identity]
		public int Id { get; set; }

		[NotNull]
		public string Inventory { get; set; }

		public string Name { get; set; }

		[NotNull]
		public string FullName { get; set; }

		public string Description { get; set; }

		public DateTime DateAdd { get; set; }

		public int Count { get; set; }

		[NotNull]
		public string Mol { get; set; }

		public string Contract { get; set; } 
	}
}