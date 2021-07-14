using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Model;
using Model.Requirements;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.Database
{
    public class RelationshipTests
    {
        //TODO add test data for all entities, testing every relationship
        //TODO make sure many relationships create at least 2
        //TODO add/update relationship tests for every relationship, and test in both directions
        readonly DbContextOptions<ShowContext> contextOptions = new DbContextOptionsBuilder<ShowContext>()
            .UseSqlite($"Filename={nameof(RelationshipTests)}.db").Options;

        [SetUp]
        public void Setup()
        {
            using var context = new ShowContext(contextOptions);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
            using var test_data = new TestDataGenerator(context, 0);
            test_data.AddIdentifiers();
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

        [Test]
        public void Role_CanLoadRequirement()
        {
            using var context = new ShowContext(contextOptions);
            var items = context.ShowRoot.ItemsInOrder();
            var item = items.First();
            var role = item.Roles.First();
            var req = role.Requirements.First();
            req.RequirementId.Should().NotBe(0);
        }

        [Test]
        public void CastGroup_CanSaveAndLoadRequirement()
        {
            int req_id;
            using (var context = new ShowContext(contextOptions))
            {
                //TODO put this into test data generation, so running tests only reads DB not writes
                var req = context.Requirements.Skip(1).First();
                req_id = req.RequirementId;
                req_id.Should().NotBe(0);
                var group = context.CastGroups.First();
                group.Requirements.Should().BeEmpty();
                group.Requirements.Add(req);
                context.SaveChanges();
            }
            using (var context = new ShowContext(contextOptions))
            {
                var group = context.CastGroups.First();
                var loaded_req = group.Requirements.First();
                loaded_req.RequirementId.Should().Be(req_id);
            }
        }

        [Test]
        public void NotRequirement_CanLoadRequirement()
        {
            using var context = new ShowContext(contextOptions);
            var not_req = context.Requirements.OfType<NotRequirement>().First();
            not_req.RequirementId.Should().NotBe(0);
            var sub_req = not_req.SubRequirement;
            sub_req.Should().NotBeNull();
            sub_req.RequirementId.Should().NotBe(0);
            sub_req.RequirementId.Should().NotBe(not_req.RequirementId);
        }

        [Test]
        public void CombinedRequirement_CanLoadRequirement()
        {
            using var context = new ShowContext(contextOptions);
            var req = context.Requirements.OfType<CombinedRequirement>().First();
            req.RequirementId.Should().NotBe(0);
            req.SubRequirements.Count.Should().BeGreaterOrEqualTo(2);
            var sub_req = req.SubRequirements.First();
            sub_req.RequirementId.Should().NotBe(0);
            sub_req.RequirementId.Should().NotBe(req.RequirementId);
        }

        [Test]
        public void Identifier_CanSaveAndLoadRequirement()
        {
            int req_id;
            using (var context = new ShowContext(contextOptions))
            {
                //TODO put this into test data generation, so running tests only reads DB not writes
                var req = context.Requirements.Skip(2).First();
                req_id = req.RequirementId;
                req_id.Should().NotBe(0);
                var identifier = context.Identifiers.First();
                identifier.Requirements.Should().BeEmpty();
                identifier.Requirements.Add(req);
                context.SaveChanges();
            }
            using (var context = new ShowContext(contextOptions))
            {
                var identifier = context.Identifiers.First();
                var loaded_req = identifier.Requirements.First();
                loaded_req.RequirementId.Should().Be(req_id);
            }
        }
    }
}
