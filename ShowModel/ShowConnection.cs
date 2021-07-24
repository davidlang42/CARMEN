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
    public struct ShowConnection//TODO does this belong in ShowModel or UI?
    {
        /// <summary>The db server type, or null if using an Sqlite file</summary>
        public DbProvider? Provider { get; set; }
        /// <summary>The db connection string</summary>
        public string ConnectionString { get; set; }
        /// <summary>The label to be shown to the user</summary>
        public string Label { get; set; }

        internal DbContextOptions<ShowContext> CreateOptions()
            => Provider switch
            {
                null => new DbContextOptionsBuilder<ShowContext>().UseSqlite(ConnectionString).Options,
                _ => throw new NotImplementedException($"Database provider {Provider} not implemented.") //TODO test & handle other DB providers
            };

        public override bool Equals(object? obj)
        {
            if (obj is not ShowConnection other)
                return false;
            if (this.Provider != other.Provider)
                return false;
            if (this.ConnectionString != other.ConnectionString)
                return false;
            // Label does not determine equality
            return true;
        }

        public override int GetHashCode()
        {
            var hc = ConnectionString.GetHashCode();
            if (Provider != null)
                hc ^= Provider.GetHashCode();
            return hc;
        }
    }

    public enum DbProvider
    {
        MySql,
        SqlServer,
        PostgreSql
    }
}
