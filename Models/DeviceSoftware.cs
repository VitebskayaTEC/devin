using LinqToDB.Mapping;
using System;

namespace Devin.Models
{
	public class DeviceSoftware
	{
		[PrimaryKey, Identity]
		public int Id { get; set; }

		public int SoftwareId { get; set; }

		public int DeviceId { get; set; }

		public DateTime Date { get; set; }

		[NotNull]
		public string Creator { get; set; }
	}
}