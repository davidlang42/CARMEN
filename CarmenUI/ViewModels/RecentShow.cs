using Microsoft.Data.Sqlite;
using Carmen.ShowModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FontAwesome.WPF;
using System.Windows.Media;
using MySqlConnector;

namespace CarmenUI.ViewModels
{
    /// <summary>
    /// An item in the recent shows list, with all details required to reconnect to the database
    /// </summary>
    public class RecentShow : ShowConnection
    {
        /// <summary>The label to be shown to the user</summary>
        public string Label { get; set; } = "";
        /// <summary>The details to be shown to the user (as tooltip)</summary>
        public string Details { get; set; } = "";
        /// <summary>The default name for the show</summary>
        public string DefaultShowName { get; set; } = "";
        /// <summary>The last time the user opened this show</summary>
        public DateTime LastOpened { get; set; } = DateTime.Now;

        public ImageSource IconSource
            => ImageAwesome.CreateImageSource(Provider == null ? FontAwesomeIcon.FolderOutlinepenOutline : FontAwesomeIcon.Database, Brushes.Black);

        public bool IsAssessible => Provider switch
        {
            null => File.Exists(Details),
            _ => throw new NotImplementedException($"Proivder not implemented: {Provider}")
        };

        public void CreateBackup()
        {
            if (Provider == null)
                File.Copy(Details, Details + "_backup", true);
            else
                throw new NotImplementedException($"Proivder not implemented: {Provider}");
        }

        public static new RecentShow FromLocalFile(string filename)
            => new RecentShow
            {
                Provider = null,
                ConnectionString = new SqliteConnectionStringBuilder { DataSource = filename }.ToString(),
                Label = Path.GetFileName(filename),
                Details = filename,
                DefaultShowName = Path.GetFileNameWithoutExtension(filename)
            };

        public static new ShowConnection FromMySql(string host, string database, string user, string pass, uint? port = null)
        {
            var connection = new MySqlConnectionStringBuilder { Server = host, Database = database, UserID = user, Password = pass };
            if (port.HasValue)
                connection.Port = port.Value;
            return new RecentShow
            {
                Provider = DbProvider.MySql,
                ConnectionString = connection.ToString(),
                Label = $"'{database}' on {host}",
                Details = $"mysql://{user}@{host}:{connection.Port}/{database}",
                DefaultShowName = database
            };
        }

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