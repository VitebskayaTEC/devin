using Dapper;
using LinqToDB;
using LinqToDB.Data;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Security.Principal;
using System.Web.Configuration;

namespace Devin.Models
{
    public static class Database
    {
        public static SqlConnection Connection(string name = "Devin")
        {
            SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings[name].ConnectionString);
            conn.Open();
            return conn;
        }

        public static void Log(this SqlConnection conn, IPrincipal user, string source, object id, string text)
        {
            conn.Execute("INSERT INTO Activity (Date, Source, Id, Text, Username) VALUES (@Date, @Source, @Id, @Text, @Username)", new Activity
            {
                Date = DateTime.Now,
                Source = source,
                Id = id.ToString(),
                Text = text,
                Username = user.Identity.Name
            });
        }

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

    public class DbDevin : DataConnection
    {
        public DbDevin() : base("Devin") { }

        public ITable<Activity> Activity 
            => GetTable<Activity>();

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
    }
}