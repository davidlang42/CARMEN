using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.ShowModel
{
    /// <summary>
    /// An object containing all the details required to connect to a ShowModel database
    /// </summary>
    public class ShowConnection
    {
        public enum DbProvider
        {
            MySql,
            SqlServer,
            PostgreSql
        }

        /// <summary>The type of database server, or null if a local file (sqlite)</summary>
        public DbProvider? Provider { get; init; }
        /// <summary>The db connection string</summary>
        public string ConnectionString { get; init; } = "";

        public static ShowConnection FromLocalFile(string filename)
            => new ShowConnection
            {
                Provider = null,
                ConnectionString = new SqliteConnectionStringBuilder { DataSource = filename }.ToString()
            };

        public static ShowConnection FromMySql(string host, string database, string user, string pass, uint? port = null)
        {
            var connection = new MySqlConnectionStringBuilder { Server = host, Database = database, UserID = user, Password = pass };
            if (port.HasValue)
                connection.Port = port.Value;
            return new ShowConnection
            {
                Provider = DbProvider.MySql,
                ConnectionString = connection.ToString()
            };
        }
    }
}
