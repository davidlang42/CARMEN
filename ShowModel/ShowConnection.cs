using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.ShowModel
{
    /// <summary>
    /// An object containing all the details required to connect to a ShowModel database
    /// </summary>
    public abstract class ShowConnection
    {
        public enum DbProvider
        {
            MySql,
            SqlServer,
            PostgreSql
        }

        /// <summary>The type of database server, or null if a local file (sqlite)</summary>
        public DbProvider? Provider { get; private init; }
        /// <summary>The db connection string</summary>
        public abstract string ConnectionString { get; }

        public ShowConnection(DbProvider? provider)
        {
            Provider = provider;
        }
    }

    public class BasicShowConnection : ShowConnection
    {
        string connectionString;

        public override string ConnectionString => connectionString;

        public BasicShowConnection(DbProvider? provider, string connection_string)
            : base(provider)
        {
            connectionString = connection_string;
        }

        public static BasicShowConnection FromLocalFile(string filename)
            => new BasicShowConnection(null, new SqliteConnectionStringBuilder { DataSource = filename }.ToString());

        internal static BasicShowConnection InMemory()
            => new BasicShowConnection(null, "Filename=:memory:");
    }
}
