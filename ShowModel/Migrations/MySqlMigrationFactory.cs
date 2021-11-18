using Microsoft.EntityFrameworkCore.Design;
using MySqlConnector;
using System;

namespace Carmen.ShowModel
{
    /// <summary>
    /// A factory used by EF migrations to construct a MySqlShowContext
    /// </summary>
    internal class MySqlMigrationFactory : IDesignTimeDbContextFactory<MySqlShowContext>
    {
        public MySqlShowContext CreateDbContext(string[] args)
        {
            if (args.Length != 1)
                throw new ApplicationException("Test database credentials should be provided as 1 argument");
            if (!Uri.TryCreate(args[0], UriKind.RelativeOrAbsolute, out var uri)
                || uri.Scheme != "mysql"
                || !uri.LocalPath.StartsWith("/")
                || !uri.UserInfo.Contains(":")
                || (!uri.IsDefaultPort && uri.Port < 1))
                throw new ApplicationException("Test database credentials must be in form \"mysql://user:pass@host[:port]/db_name\"");
            var user_info = uri.UserInfo.Split(":");
            var connection = new MySqlConnectionStringBuilder
            {
                Server = uri.Host,
                Database = uri.LocalPath.Substring(1),
                UserID = user_info[0],
                Password = user_info[1]
            };
            if (!uri.IsDefaultPort)
                connection.Port = (uint)uri.Port;
            return new MySqlShowContext(connection.ToString());
        }
    }
}
