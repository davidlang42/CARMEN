using Microsoft.EntityFrameworkCore;
using Model;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    public class DatabaseTests
    {
        readonly DbContextOptions<ShowContext> contextOptions = new DbContextOptionsBuilder<ShowContext>()
            .UseSqlite($"Filename={nameof(DatabaseTests)}.db").Options;

        [SetUp]
        public void Setup()
        {
            using var context = new ShowContext(contextOptions);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
            using var test_data = new TestDataGenerator(context, 0);
            test_data.AddShowStructure(30, 6, 1, include_items_at_every_depth: false);
            test_data.AddCastGroups(4);
            context.SaveChanges();
            test_data.AddCriteriaAndRequirements(); // after cast groups committed
            context.SaveChanges();
            test_data.AddApplicants(100); // after criteria committed
            context.SaveChanges();
            test_data.AddRoles(5); // after items, cast groups, requirements committed
            context.SaveChanges();
        }

        [Test]
        public void LoadEntities()
        {
            using var context = new ShowContext(contextOptions);
            context.Applicants.Load();
            context.CastGroups.Load();
            context.Nodes.Load();
            context.Criterias.Load();
            context.Images.Load();
            context.SectionTypes.Load();
            context.Requirements.Load();
        }
    }
}
