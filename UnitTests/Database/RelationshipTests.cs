using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ShowModel;
using ShowModel.Requirements;
using ShowModel.Structure;
using NUnit.Framework;
using System.Linq;

namespace UnitTests.Database
{
    public class RelationshipTests
    {
        readonly DbContextOptions<ShowContext> contextOptions = new DbContextOptionsBuilder<ShowContext>()
            .UseSqlite($"Filename={nameof(RelationshipTests)}.db").Options;

        [OneTimeSetUp]
        public void CreateDatabase()
        {
            using var context = new ShowContext(contextOptions);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
            using var test_data = new TestDataGenerator(context, 0);
            test_data.AddPrimaryCastGroups(4);
            context.SaveChanges();
            test_data.AddShowStructure(30, 6, 1, include_items_at_every_depth: false); // after cast groups committed
            context.SaveChanges();
            test_data.AddCriteriaAndRequirements(); // after cast groups committed
            context.SaveChanges();
            test_data.AddIdentifiers(); // after requirements committed
            test_data.AddSecondaryCastGroups(1); // after requirements committed
            context.SaveChanges();
            test_data.AddApplicants(100); // after criteria committed
            context.SaveChanges();
            test_data.AddRoles(5); // after items, cast groups, requirements committed
            test_data.AddImages(); // after applicants, cast groups committed
            context.SaveChanges();
        }

        [Test]
        public void LoadEntities_WithoutCrash()
        {
            using var context = new ShowContext(contextOptions);
            context.Applicants.Load();
            context.CastGroups.Load();
            context.Nodes.Load();
            context.Criterias.Load();
            context.Images.Load();
            context.SectionTypes.Load();
            context.Requirements.Load();
            context.Identifiers.Load();
        }

        #region Applicants
        [Test]
        public void Applicant_Image_ManyToOne()
        {
            using var context = new ShowContext(contextOptions);
            var applicant = context.Applicants.Where(a => a.Photo != null).First();
            applicant.ApplicantId.Should().NotBe(0);
            var image = applicant.Photo;
            image.Should().NotBeNull();
            image!.ImageId.Should().NotBe(0);
            // Reverse navigation not defined
        }

        [Test]
        public void Applicant_Ability_OneToMany()
        {
            using var context = new ShowContext(contextOptions);
            var applicant = context.Applicants.First();
            applicant.ApplicantId.Should().NotBe(0);
            var ability = applicant.Abilities.First();
            ability.Criteria.Should().NotBeNull();
            ability.Applicant.Should().Be(applicant);
        }

        [Test]
        public void Applicant_CastGroup_ManyToMany()
        {
            using var context = new ShowContext(contextOptions);
            var applicant = context.Applicants.First();
            applicant.ApplicantId.Should().NotBe(0);
            var group = applicant.CastGroups.First();
            group.CastGroupId.Should().NotBe(0);
            group.Members.Should().Contain(applicant);
        }

        [Test]
        public void Applicant_Identity_OneToMany()
        {
            using var context = new ShowContext(contextOptions);
            var applicant = context.Applicants.First();
            applicant.ApplicantId.Should().NotBe(0);
            var identity = applicant.Identities.First();
            identity.Identifier.Should().NotBeNull();
            // Reverse navigation not defined
        }

        [Test]
        public void CastGroup_Image_ManyToOne()
        {
            using var context = new ShowContext(contextOptions);
            var group = context.CastGroups.Where(g => g.Icon != null).First();
            group.CastGroupId.Should().NotBe(0);
            var image = group.Icon;
            image.Should().NotBeNull();
            image!.ImageId.Should().NotBe(0);
            // Reverse navigation not defined
        }

        [Test]
        public void Identity_Identifier_ManyToOne()
        {
            using var context = new ShowContext(contextOptions);
            var identity = context.Applicants.First().Identities.First();
            var identifier = identity.Identifier;
            identifier.Should().NotBeNull();
            identifier.IdentifierId.Should().NotBe(0);
            // Reverse navigation not defined
        }
        #endregion

        #region Criterias
        [Test]
        public void Criteria_Ability_OneToMany()
        {
            using var context = new ShowContext(contextOptions);
            var criteria = context.Criterias.First();
            criteria.CriteriaId.Should().NotBe(0);
            var ability = criteria.Abilities.First();
            ability.Applicant.Should().NotBeNull();
            ability.Criteria.Should().Be(criteria);
        }
        #endregion

        #region Requirements
        [Test]
        public void Requirement_Role_ManyToMany()
        {
            using var context = new ShowContext(contextOptions);
            var role = context.ShowRoot.ItemsInOrder().First().Roles.First();
            role.RoleId.Should().NotBe(0);
            var req = role.Requirements.First();
            req.RequirementId.Should().NotBe(0);
            req.UsedByRoles.Should().Contain(role);
        }

        [Test]
        public void Requirement_CastGroup_ManyToMany()
        {
            using var context = new ShowContext(contextOptions);
            var group = context.CastGroups.Where(g => !g.Primary).First();
            group.CastGroupId.Should().NotBe(0);
            var req = group.Requirements.First();
            req.RequirementId.Should().NotBe(0);
            req.UsedByCastGroups.Should().Contain(group);
        }

        [Test]
        public void Requirement_CombinedRequirement_ManyToMany()
        {
            using var context = new ShowContext(contextOptions);
            var req = context.Requirements.OfType<CombinedRequirement>().First();
            req.RequirementId.Should().NotBe(0);
            req.SubRequirements.Count.Should().BeGreaterOrEqualTo(2);
            var sub_req = req.SubRequirements.First();
            sub_req.RequirementId.Should().NotBe(0);
            sub_req.RequirementId.Should().NotBe(req.RequirementId);
            sub_req.UsedByCombinedRequirements.Should().Contain(req);
        }

        [Test]
        public void Requirement_Identifier_ManyToMany()
        {
            using var context = new ShowContext(contextOptions);
            var identifier = context.Identifiers.First();
            identifier.IdentifierId.Should().NotBe(0);
            var req = identifier.Requirements.First();
            req.RequirementId.Should().NotBe(0);
            req.UsedByIdentifiers.Should().Contain(identifier);
        }

        [Test]
        public void CastGroupRequirement_CastGroup_ManyToOne()
        {
            using var context = new ShowContext(contextOptions);
            var req = context.Requirements.OfType<CastGroupRequirement>().First();
            req.RequirementId.Should().NotBe(0);
            var group = req.RequiredGroup;
            group.CastGroupId.Should().NotBe(0);
            // Reverse navigation not defined
        }

        [Test]
        public void AbilityRangeRequirement_Criteria_ManyToOne()
        {
            using var context = new ShowContext(contextOptions);
            var req = context.Requirements.OfType<AbilityRangeRequirement>().First();
            req.RequirementId.Should().NotBe(0);
            var criteria = req.Criteria;
            criteria.CriteriaId.Should().NotBe(0);
            // Reverse navigation not defined
        }

        [Test]
        public void AbilityExactRequirement_Criteria_ManyToOne()
        {
            using var context = new ShowContext(contextOptions);
            var req = context.Requirements.OfType<AbilityExactRequirement>().First();
            req.RequirementId.Should().NotBe(0);
            var criteria = req.Criteria;
            criteria.CriteriaId.Should().NotBe(0);
            // Reverse navigation not defined
        }

        [Test]
        public void NotRequirement_Requirement_ManyToOne()
        {
            using var context = new ShowContext(contextOptions);
            var not_req = context.Requirements.OfType<NotRequirement>().First();
            not_req.RequirementId.Should().NotBe(0);
            var sub_req = not_req.SubRequirement;
            sub_req.Should().NotBeNull();
            sub_req.RequirementId.Should().NotBe(0);
            sub_req.RequirementId.Should().NotBe(not_req.RequirementId);
            // Reverse navigation not defined
        }
        #endregion

        #region Structure
        [Test]
        public void ShowRoot_Image_ManyToOne()
        {
            using var context = new ShowContext(contextOptions);
            var show = context.ShowRoot;
            show.NodeId.Should().NotBe(0);
            var image = show.Logo;
            image.Should().NotBeNull();
            image!.ImageId.Should().NotBe(0);
            // Reverse navigation not defined
        }

        [Test]
        public void InnerNode_Node_OneToMany()
        {
            using var context = new ShowContext(contextOptions);
            var inner_node = context.Nodes.OfType<InnerNode>().First();
            inner_node.NodeId.Should().NotBe(0);
            var node = inner_node.Children.First();
            node.NodeId.Should().NotBe(0);
            node.Parent.Should().Be(inner_node);
        }

        [Test]
        public void Node_CountByGroup_OneToMany()
        {
            using var context = new ShowContext(contextOptions);
            var node = context.ShowRoot;
            var cbg = node.CountByGroups.First();
            cbg.CastGroup.Should().NotBeNull();
            // Reverse navigation not defined
        }

        [Test]
        public void Section_SectionType_ManyToOne()
        {
            using var context = new ShowContext(contextOptions);
            var section = context.Nodes.OfType<Section>().First();
            section.NodeId.Should().NotBe(0);
            var st = section.SectionType;
            st.SectionTypeId.Should().NotBe(0);
            // Reverse navigation not defined
        }

        [Test]
        public void Item_Role_ManyToMany()
        {
            using var context = new ShowContext(contextOptions);
            var item = context.ShowRoot.ItemsInOrder().First();
            item.NodeId.Should().NotBe(0);
            var role = item.Roles.First();
            role.RoleId.Should().NotBe(0);
            role.Items.Should().Contain(item);
        }

        [Test]
        public void Role_CountByGroup_OneToMany()
        {
            using var context = new ShowContext(contextOptions);
            var role = context.ShowRoot.ItemsInOrder().First().Roles.First();
            var cbg = role.CountByGroups.First();
            cbg.CastGroup.Should().NotBeNull();
            // Reverse navigation not defined
        }

        [Test]
        public void CountByGroup_CastGroup_ManyToOne()
        {
            using var context = new ShowContext(contextOptions);
            var cbg = context.ShowRoot.ItemsInOrder().First().Roles.First().CountByGroups.First();
            var group = cbg.CastGroup;
            group.CastGroupId.Should().NotBe(0);
            // Reverse navigation not defined
        }
        #endregion
    }
}
