using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ShowModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarmenUI.ViewModels
{
    /// <summary>
    /// An item in the recent shows list, with all details required to reconnect to the database
    /// </summary>
    public class RecentShow //TODO this whole class needs a refactor once I know how I'm connecting to external dbs
    {
        public enum DbProvider
        {
            MySql,
            SqlServer,
            PostgreSql
        }

        /// <summary>The type of database server, or null if a local file (sqlite)</summary>
        public DbProvider? Provider { get; set; }
        /// <summary>The db connection string</summary>
        public string ConnectionString { get; set; } = "";
        /// <summary>The label to be shown to the user</summary>
        public string Label { get; set; } = "";
        /// <summary>The last time the user opened this show</summary>
        public DateTime LastOpened { get; set; } = DateTime.Now;

        public string IconPath
            => Provider == null ? @"\Icons\OpenFile_16x.png" : @"\Icons\Database_16x.png";

        public static RecentShow FromLocalFile(string filename)
            => new RecentShow
            {
                Label = Path.GetFileName(filename),
                ConnectionString = new SqliteConnectionStringBuilder { DataSource = filename }.ToString()
            };

        internal DbContextOptions<ShowContext> CreateOptions()
            => Provider switch
            {
                null => new DbContextOptionsBuilder<ShowContext>().UseSqlite(ConnectionString).Options,
                _ => throw new NotImplementedException($"Database provider {Provider} not implemented.") //TODO test & handle other DB providers
            };

        //TODO are these needed?
        //public override bool Equals(object? obj)
        //{
        //    if (obj is not ShowConnection other)
        //        return false;
        //    if (this.Provider != other.Provider)
        //        return false;
        //    if (this.ConnectionString != other.ConnectionString)
        //        return false;
        //    // Label does not determine equality
        //    return true;
        //}

        //public override int GetHashCode()
        //{
        //    var hc = ConnectionString.GetHashCode();
        //    if (Provider != null)
        //        hc ^= Provider.GetHashCode();
        //    return hc;
        //}
    }
}