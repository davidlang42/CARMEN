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
        public string Host { get; set; } = "";
        public string Database { get; set; } = "";
        /// <summary>Null means default</summary>
        public uint? Port { get; set; }
        public string User { get; set; } = "";
        public string Password { get; set; } = "";

        public ConnectionDetails() : base(DbProvider.MySql)
        { }

        /// <summary>The database connection string</summary>
        public override string ConnectionString => Provider switch
        {
            DbProvider.MySql => new MySqlConnectionStringBuilder { Server = Host, Database = Database, UserID = User, Password = Password, Port = Port ?? 3306, SslMode = MySqlSslMode.VerifyFull }.ToString(),
            _ => throw new NotImplementedException($"Provider not implemented: {Provider}")
        };

        //TODO is this needed?
        /// <summary>The default name for the show</summary>
        public string DefaultShowName => Provider switch
        {
            DbProvider.MySql => Database,
            _ => throw new NotImplementedException($"Provider not implemented: {Provider}")
        };

        //TODO is this needed?
        public bool CheckAssessible() => Provider switch
        {
            DbProvider.MySql => TryPing(Host),
            _ => throw new NotImplementedException($"Provider not implemented: {Provider}")
        };

        private static bool TryPing(string host)
        {
            try
            {
                return new Ping().Send(host, 500).Status == IPStatus.Success;
            }
            catch (Exception ex)
            {
                Log.Warning(ex, $"Try ping failed: {host}");
                return false;
            }
        }

        //TODO is this needed?
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
