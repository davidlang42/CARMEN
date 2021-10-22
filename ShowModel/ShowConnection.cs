using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
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

        private DbContextOptions<ShowContext>? contextOptions;
        internal DbContextOptions<ShowContext> ContextOptions => contextOptions ??= Provider switch
        {
            null => new DbContextOptionsBuilder<ShowContext>().UseSqlite(ConnectionString).Options,
            _ => throw new NotImplementedException($"Database provider {Provider} not implemented.")
        };

        public static ShowConnection FromLocalFile(string filename)
            => new ShowConnection
            {
                Provider = null,
                ConnectionString = new SqliteConnectionStringBuilder { DataSource = filename }.ToString()
            };
    }
}
