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
    /// A factory used by EF migrations to construct a ShowContext
    /// </summary>
    internal class MigrationFactory : IDesignTimeDbContextFactory<ShowContext>
    {
        private class ShowInMemory : ShowConnection
        {
            protected override DbContextOptions<ShowContext> CreateOptions()
            {
                var connection = new SqliteConnection("Filename=:memory:");
                connection.Open(); // this connection is never closed
                var builder = new DbContextOptionsBuilder<ShowContext>();
                builder.UseSqlite(connection);
                return builder.Options;
            }
        }

        public ShowContext CreateDbContext(string[] args)
        {
            if (args.FirstOrDefault() is string filename)
                return new ShowContext(ShowConnection.FromLocalFile(filename));
            return new ShowContext(new ShowInMemory());
        }
    }
}
