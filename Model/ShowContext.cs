using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Model.Structure;
using Model.Requirements;
using Model.Criterias;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Model
{
    public class ShowContext : DbContext
    {
        #region Database collections
        public DbSet<Applicant> Applicants => Set<Applicant>();
        public DbSet<CastGroup> CastGroups => Set<CastGroup>();
        public DbSet<Node> Nodes => Set<Node>();
        public DbSet<Criteria> Criteria => Set<Criteria>();
        public DbSet<Requirement> Requirements => Set<Requirement>();
        public DbSet<Image> Images => Set<Image>();
        public DbSet<SectionType> SectionTypes => Set<SectionType>();
        #endregion

        /// <summary>The root node of the show structure</summary>
        public ShowRoot ShowRoot => Nodes.OfType<ShowRoot>().SingleOrDefault() ?? Add(new ShowRoot()).Entity;

        public ShowContext(DbContextOptions<ShowContext> options) : base(options)
        { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseLazyLoadingProxies();
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //TODO pull IDS out of POCOs, keep in the EF (maybe?)
            // Configure owned entities
            modelBuilder.Entity<Ability>()//TODO ability owned by applicant
                .HasKey(a => new { a.ApplicantId, a.CriteriaId });
            modelBuilder.Entity<Role>().OwnsMany(
                r => r.CountByGroups, cbg => ConfigureCountByGroup(cbg, nameof(Role.RoleId)));
            modelBuilder.Entity<SectionType>().OwnsMany(
                st => st.CountByGroups, cbg => ConfigureCountByGroup(cbg, nameof(SectionType.SectionTypeId)));
            modelBuilder.Entity<Node>().OwnsMany(
                n => n.CountByGroups, cbg => ConfigureCountByGroup(cbg, nameof(Node.NodeId)));
            // Add inheritance structure for item tree
            modelBuilder.Entity<Item>();
            modelBuilder.Entity<Section>();
            modelBuilder.Entity<ShowRoot>();
            // Add inheritance structure for requirements
            modelBuilder.Entity<AgeRequirement>();
            modelBuilder.Entity<GenderRequirement>();
            modelBuilder.Entity<CastGroupRequirement>();
            modelBuilder.Entity<AbilityExactRequirement>();
            modelBuilder.Entity<AbilityRangeRequirement>();
            modelBuilder.Entity<AndRequirement>();
            modelBuilder.Entity<OrRequirement>();
            modelBuilder.Entity<XorRequirement>();
            modelBuilder.Entity<NotRequirement>();
            // Add inheritance structure for critiera
            modelBuilder.Entity<NumericCriteria>();
            modelBuilder.Entity<SelectCriteria>()
                .Property(sc => sc.Options)
                .HasConversion(obj => JsonSerializer.Serialize(obj, null),
                      json => JsonSerializer.Deserialize<string[]>(json, null) ?? SelectCriteria.DEFAULT_OPTIONS);
            modelBuilder.Entity<BooleanCriteria>();
        }

        private void ConfigureCountByGroup<T>(OwnedNavigationBuilder<T, CountByGroup> cbg, string foreign_key) where T : class
        {
            cbg.WithOwner().HasForeignKey(foreign_key);
            cbg.HasKey(foreign_key, nameof(CountByGroup.CastGroup.CastGroupId));
            cbg.Property(CountByGroup.CountExpression) // store private nullable property for Count/Everyone
                .HasColumnName(nameof(CountByGroup.Count));
        }
    }
}
