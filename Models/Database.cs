using Dapper;
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

        public static string ToHtml(this List<string> list) => string.Join(",<br />", list.ToArray());

        public static string ToLog(this List<string> list) => string.Join(",\n", list.ToArray());
    }
}