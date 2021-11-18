using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.ShowModel
{
    /// <summary>
    /// A factory used by EF migrations to construct a SqliteShowContext
    /// </summary>
    internal class SqliteMigrationFactory : IDesignTimeDbContextFactory<SqliteShowContext>
    {
        public SqliteShowContext CreateDbContext(string[] args)
        {
            if (args.FirstOrDefault() is string filename)
                return new SqliteShowContext(new SqliteConnectionStringBuilder { DataSource = filename }.ToString());
            return new SqliteShowContext("Filename=:memory:");
        }
    }

    /// <summary>
    /// A factory used by EF migrations to construct a MySqlShowContext
    /// </summary>
    internal class MySqlMigrationFactory : IDesignTimeDbContextFactory<MySqlShowContext>
    {
        public MySqlShowContext CreateDbContext(string[] args)
        {
            return new MySqlShowContext("Filename=:memory:");//TODO
        }
    }
}
