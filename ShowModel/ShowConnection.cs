using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShowModel
{
    /// <summary>
    /// The database connection settings required to open a show database.
    /// </summary>
    public struct ShowConnection
    {
        /// <summary>The db server type, or null if using an Sqlite file</summary>
        public DbProvider? Provider;
        /// <summary>The db connection string</summary>
        public string ConnectionString;
        /// <summary>The label to be shown to the user</summary>
        public string Label;

        internal DbContextOptions<ShowContext> CreateOptions()
            => Provider switch
            {
                null => new DbContextOptionsBuilder<ShowContext>().UseSqlite(ConnectionString).Options,
                _ => throw new NotImplementedException($"Database provider {Provider} not implemented.") //TODO test & handle other DB providers
            };
    }

    public enum DbProvider
    {
        MySql,
        SqlServer,
        PostgreSql
    }
}
