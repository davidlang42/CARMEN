using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Carmen.ShowModel;
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
    public class RecentShow
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
        /// <summary>The default name for the show</summary>
        public string DefaultShowName { get; set; } = "";
        /// <summary>The last time the user opened this show</summary>
        public DateTime LastOpened { get; set; } = DateTime.Now;

        public string IconPath
            => Provider == null ? @"\Icons\OpenFile.png" : @"\Icons\CloudDatabase.png";

        public static RecentShow FromLocalFile(string filename)
            => new RecentShow
            {
                Label = Path.GetFileName(filename),
                DefaultShowName = Path.GetFileNameWithoutExtension(filename),
                ConnectionString = new SqliteConnectionStringBuilder { DataSource = filename }.ToString()
            };

        internal DbContextOptions<ShowContext> CreateOptions()
            => Provider switch
            {
                null => new DbContextOptionsBuilder<ShowContext>().UseSqlite(ConnectionString).Options,
                _ => throw new NotImplementedException($"Database provider {Provider} not implemented.")
            };

        public override bool Equals(object? obj)
        {
            if (obj is not RecentShow other)
                return false;
            if (this.Provider != other.Provider)
                return false;
            if (this.ConnectionString != other.ConnectionString)
                return false;
            // Label,LastOpened,DefaultShowName don't matter
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
}