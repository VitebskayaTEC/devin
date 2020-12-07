using LinqToDB.Mapping;

namespace Devin.Models
{
	[Table(Name="AidaDescriptions")]
	public class AidaDescription
	{
		[Column]
		public string Name { get; set; }

		[Column]
		public string Description { get; set; }
	}
}