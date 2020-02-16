using Devin.Models.Relations;
using Devin.Models.Site;
using LinqToDB;
using LinqToDB.Data;
using System;
using System.Collections.Generic;
using System.Security.Principal;

namespace Devin.Models
{
	public static class Database
    {
        public static void Log(this DataConnection db, IPrincipal user, string source, object id, string text)
        {
            db.Insert(new Activity
            {
                Date = DateTime.Now,
                Source = source,
                Id = id.ToString(),
                Text = text,
                Username = user.Identity.Name
            });
        }

        public static string ToHtml(this List<string> list) => string.Join(",<br />", list.ToArray());

        public static string ToLog(this List<string> list) => string.Join(",\n", list.ToArray());
    }

    public class SiteContext : DataConnection
    {
        public SiteContext() : base("Site") { }

        public ITable<MetalsCost> MetalsCosts
            => GetTable<MetalsCost>();

        public ITable<Constant> Constants
            => GetTable<Constant>();

        public ITable<User> Users
            => GetTable<User>();
    }

    public class DevinContext : DataConnection
    {
        public DevinContext() : base("Devin") { }

        public ITable<Activity> Activity 
            => GetTable<Activity>();

        public ITable<Elm> Elm
            => GetTable<Elm>();

        public ITable<Report> Report 
            => GetTable<Report>();

        public ITable<Item> Item
            => GetTable<Item>();

        public ITable<Cartridge> Cartridges 
            => GetTable<Cartridge>();

        public ITable<Device> Devices 
            => GetTable<Device>();

        public ITable<Folder> Folders
            => GetTable<Folder>();

        public ITable<Hardware> Hardware
           => GetTable<Hardware>();

        public ITable<Object1C> Objects1C 
            => GetTable<Object1C>();

        public ITable<Printer> Printers 
            => GetTable<Printer>();

        public ITable<Repair> Repairs 
            => GetTable<Repair>();

        public ITable<Storage> Storages 
            => GetTable<Storage>();

        public ITable<WorkPlace> WorkPlaces 
            => GetTable<WorkPlace>();

        public ITable<Writeoff> Writeoffs 
            => GetTable<Writeoff>();

		public ITable<Software> Software
			=> GetTable<Software>();

		public ITable<Employee> Employees
			=> GetTable<Employee>();

		public ITable<Official> Officials
			=> GetTable<Official>();

		// relationship tables

		public ITable<PrinterCartridge> _PrinterCartridge
            => GetTable<PrinterCartridge>();

        public ITable<TypeCartridge> _CartridgeTypes
            => GetTable<TypeCartridge>();

        public ITable<TypeDevice> _DeviceTypes
            => GetTable<TypeDevice>();

        public ITable<TypeSystem> _SystemTypes
            => GetTable<TypeSystem>();

        public ITable<TypeWriteoff> _WriteoffTypes
            => GetTable<TypeWriteoff>();

		public ITable<DeviceSoftware> _DeviceSoftware
			=> GetTable<DeviceSoftware>();

		public ITable<Relation_Official_Employee> Relation_Officials_Employees
			=> GetTable<Relation_Official_Employee>();
	}
}