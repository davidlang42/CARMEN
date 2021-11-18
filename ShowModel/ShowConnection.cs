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
        /// <summary>The type of database server, or null if a local file (sqlite)</summary>
        public DbProvider? Provider { get; set; }
        /// <summary>The db connection string</summary>
        public abstract string ConnectionString { get; }

        public ShowConnection(DbProvider? provider)
        {
            Provider = provider;
        }
    }

    public enum DbProvider
    {
        MySql,
        //SqlServer,
        //PostgreSql
    }

    public class LocalShowConnection : ShowConnection
    {
        public string Filename { get; }

        public override string ConnectionString => new SqliteConnectionStringBuilder { DataSource = Filename }.ToString();

        public LocalShowConnection(string filename)
            : base(null)
        {
            Filename = filename;
        }
    }
}
