using Microsoft.Data.Sqlite;
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
    public class RecentShow : ShowConnection
    {
        
        /// <summary>The label to be shown to the user</summary>
        public string Label { get; set; } = "";
        /// <summary>The default name for the show</summary>
        public string DefaultShowName { get; set; } = "";
        /// <summary>The last time the user opened this show</summary>
        public DateTime LastOpened { get; set; } = DateTime.Now;

        public string IconPath
            => Provider == null ? @"\Icons\OpenFile.png" : @"\Icons\CloudDatabase.png";

        private RecentShow(DbProvider? provider, string connection_string)
            : base(provider, connection_string)
        { }

        public static new RecentShow FromLocalFile(string filename)
            => new RecentShow(null, new SqliteConnectionStringBuilder { DataSource = filename }.ToString())
            {
                Label = Path.GetFileName(filename),
                DefaultShowName = Path.GetFileNameWithoutExtension(filename)
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