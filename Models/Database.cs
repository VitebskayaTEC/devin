using System.Data.SqlClient;
using System.Web.Configuration;

namespace Devin.Models
{
    public class Database
    {
        public static SqlConnection Connection(string name = "Devin")
        {
            SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings[name].ConnectionString);
            conn.Open();
            return conn;
        }
    }
}