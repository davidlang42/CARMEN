using Carmen.ShowModel;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.Mobile.Models
{
    internal class ConnectionDetails : ShowConnection
    {
        public const uint DEFAULT_PORT = 3306;

        public string Host { get; set; } = "";
        public string Database { get; set; } = "";
        /// <summary>Null means default</summary>
        public uint? Port { get; set; }
        public string User { get; set; } = "";
        public string Password { get; set; } = "";

        public bool SaveLogin { get; set; } = true; // this shouldn't really be part of ConnectionDetails

        public ConnectionDetails() : base(DbProvider.MySql)
        { }

        /// <summary>The database connection string</summary>
        public override string ConnectionString => Provider switch
        {
            DbProvider.MySql => new MySqlConnectionStringBuilder {
                Server = Host,
                Database = Database,
                UserID = User,
                Password = Password,
                Port = Port ?? DEFAULT_PORT,
#if ANDROID
                SslMode = MySqlSslMode.VerifyCA // VerifyFull fails on Android
#else
                SslMode = MySqlSslMode.VerifyFull
#endif
            }.ToString(),
            _ => throw new NotImplementedException($"Provider not implemented: {Provider}")
        };

        public string Description => Provider switch
        {
            DbProvider.MySql => $"mysql://{Host}:{Port ?? DEFAULT_PORT}",
            _ => throw new NotImplementedException($"Provider not implemented: {Provider}")
        };

        /// <summary>The default name for the show</summary>
        public string DefaultShowName => Database;

        public bool TryPing()
        {
#if ANDROID || IOS
            return true; // Ping always fails on Android, and sometimes on iOS, therefore skip
#else
            try
            {
                return new Ping().Send(Host, 500).Status == IPStatus.Success;
            }
            catch (Exception ex)
            {
                Log.Warning(ex, $"Ping failed: {Host}");
                return false;
            }
#endif
        }

        public bool TryConnection(out string? error)
        {
            error = null;
            try
            {
                if (Provider == DbProvider.MySql)
                    ServerVersion.AutoDetect(ConnectionString);
                else
                    throw new NotImplementedException($"Provider not implemented: {Provider}");
            }
            catch (Exception ex)
            {
                Log.Error(ex, nameof(TryConnection));
                error = ex.Message;
            }
            return error == null;
        }
    }
}
