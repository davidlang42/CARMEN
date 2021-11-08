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
using System.Net.NetworkInformation;

namespace CarmenUI.ViewModels
{
    /// <summary>
    /// An item in the recent shows list, with all details required to reconnect to the database
    /// </summary>
    public class RecentShow : ShowConnection
    {
        /// <summary>The label to be shown to the user</summary>
        public string Label { get; set; } = "";
        /// <summary>The tooltip to be shown to the user</summary>
        public string Tooltip => Filename;
        /// <summary>The default name for the show</summary>
        public string DefaultShowName { get; set; } = "";
        /// <summary>The last time the user opened this show</summary>
        public DateTime LastOpened { get; set; } = DateTime.Now;
        /// <summary>The icon shown to the user</summary>
        public ImageSource IconSource
            => ImageAwesome.CreateImageSource(Provider == null ? FontAwesomeIcon.FolderOutlinepenOutline : FontAwesomeIcon.Database, Brushes.Black);

        #region Set if Provider == null
        public string Filename { get; set; } = "";
        #endregion

        #region Set if Provider != null
        public string Host { get; set; } = "";
        public string Database { get; set; } = "";
        /// <summary>Null means default</summary>
        public uint? Port { get; set; }
        public string User { get; set; } = "";
        public string Password { get; set; } = "";
        #endregion

        public override string ConnectionString => Provider switch
        {
            null => new SqliteConnectionStringBuilder { DataSource = Filename }.ToString(),
            DbProvider.MySql => new MySqlConnectionStringBuilder { Server = Host, Database = Database, UserID = User, Password = Password, Port = Port ?? 3306 }.ToString(),
            _ => throw new NotImplementedException($"Provider not implemented: {Provider}")
        };

        public bool CheckAssessible() => Provider switch
        {
            null => File.Exists(Filename),
            DbProvider.MySql => new Ping().Send(Host, 500).Status == IPStatus.Success,
            _ => throw new NotImplementedException($"Provider not implemented: {Provider}")
        };

        public void CreateBackupIfFile()
        {
            if (Provider == null)
                File.Copy(Filename, Filename + "_backup", true);
        }

        public RecentShow(string filename)
            : base(null)
        {
            Filename = filename;
            Label = Path.GetFileName(filename);
            DefaultShowName = Path.GetFileNameWithoutExtension(filename);
        }

        public RecentShow(DbProvider provider, string host, string database, string user, string pass, uint? port = null)
            : base(provider)
        {
            Host = host;
            Database = database;
            User = user;
            Password = pass;
            Port = port;
            Label = $"'{database}' on {host}";
            DefaultShowName = database;
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