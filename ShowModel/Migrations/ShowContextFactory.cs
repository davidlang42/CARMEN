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
        public ShowContext CreateDbContext(string[] args)
        {
            if (args.FirstOrDefault() is string filename)
                return new ShowContext(BasicShowConnection.FromLocalFile(filename));
            return new ShowContext(BasicShowConnection.InMemory());
        }
    }
}
