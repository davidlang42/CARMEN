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
using Microsoft.EntityFrameworkCore;

namespace CarmenUI.ViewModels
{
    /// <summary>
    /// An item in the recent shows list, with all details required to reconnect to the database
    /// </summary>
    public class RecentShow : ShowConnection
    {
        /// <summary>The last time the user opened this show</summary>
        public DateTime LastOpened { get; set; } = DateTime.Now;

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

        /// <summary>The icon shown to the user</summary>
        public ImageSource IconSource
            => ImageAwesome.CreateImageSource(Provider == null ? FontAwesomeIcon.FolderOutlinepenOutline : FontAwesomeIcon.Database, Brushes.Black);

        /// <summary>The database connection string</summary>
        public override string ConnectionString => Provider switch
        {
            null => new SqliteConnectionStringBuilder { DataSource = Filename }.ToString(),
            DbProvider.MySql => new MySqlConnectionStringBuilder { Server = Host, Database = Database, UserID = User, Password = Password, Port = Port ?? 3306, SslMode = Properties.Settings.Default.ConnectionSslMode }.ToString(),
            _ => throw new NotImplementedException($"Provider not implemented: {Provider}")
        };

        /// <summary>The label to be shown to the user</summary>
        public string Label => Provider switch
        {
            null => Path.GetFileName(Filename),
            DbProvider.MySql => $"'{Database}' on {Host}",
            _ => throw new NotImplementedException($"Provider not implemented: {Provider}")
        };
        /// <summary>The tooltip to be shown to the user</summary>
        public string Tooltip => Provider switch
        {
            null => Filename,
            DbProvider.MySql => $"mysql://{User}@{Host}{(Port.HasValue ? $":{Port}" : "")}/{Database}",
            _ => throw new NotImplementedException($"Provider not implemented: {Provider}")
        };
        /// <summary>The default name for the show</summary>
        public string DefaultShowName => Provider switch
        {
            null => Path.GetFileNameWithoutExtension(Filename),
            DbProvider.MySql => Database,
            _ => throw new NotImplementedException($"Provider not implemented: {Provider}")
        };

        /// <summary>Parameterless constructor required for serialization, defaults to Sqlite</summary>
        public RecentShow()
            : base(null)
        { }

        public bool CheckAssessible() => Provider switch
        {
            null => File.Exists(Filename),
            DbProvider.MySql => new Ping().Send(Host, 500).Status == IPStatus.Success, //TODO this should check server is connectable (ie. user & pass) using ServerVersion.AutoDetect or similar, and handle blanks/malformed
            _ => throw new NotImplementedException($"Provider not implemented: {Provider}")
        };

        public bool TryConnection(out string? error)
        {
            error = null;
            try
            {
                if (Provider == null)
                    _ = File.ReadAllBytes(Filename);
                else if (Provider == DbProvider.MySql)
                    ServerVersion.AutoDetect(ConnectionString);
                else
                    throw new NotImplementedException($"Provider not implemented: {Provider}");
            } catch (Exception ex)
            {
                error = ex.Message;
            }
            return error == null;
        }

        public void CreateBackupIfFile()
        {
            if (Provider == null)
                File.Copy(Filename, Filename + "_backup", true);
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