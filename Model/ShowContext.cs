﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Linq;
using System.Text.Json;
using Model.Applicants;
using Model.Criterias;
using Model.Requirements;
using Model.Structure;

namespace Model
{
    public class ShowContext : DbContext
    {
        #region Database collections
        public DbSet<Applicant> Applicants => Set<Applicant>();
        public DbSet<Identifier> Identifiers => Set<Identifier>();
        public DbSet<CastGroup> CastGroups => Set<CastGroup>();
        public DbSet<Criteria> Criterias => Set<Criteria>();
        public DbSet<Requirement> Requirements => Set<Requirement>();
        public DbSet<Node> Nodes => Set<Node>();
        public DbSet<SectionType> SectionTypes => Set<SectionType>();
        public DbSet<Image> Images => Set<Image>();
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
            // Configure composite keys
            modelBuilder.Entity<Ability>()
                .HasKey(nameof(Ability.Applicant.ApplicantId), nameof(Ability.Criteria.CriteriaId));

            // Configure owned entities
            modelBuilder.Entity<Applicant>()
                .OwnsMany(a => a.Identities)
                .WithOwnerCompositeKey(nameof(Applicant.ApplicantId), nameof(Identity.Identifier.IdentifierId));
            modelBuilder.Entity<Role>()
                .OwnsMany(r => r.CountByGroups)
                .WithOwnerCompositeKey(nameof(Role.RoleId), nameof(CountByGroup.CastGroup.CastGroupId));
            modelBuilder.Entity<Node>()
                .OwnsMany(n => n.CountByGroups)
                .WithOwnerCompositeKey(nameof(Node.NodeId), nameof(CountByGroup.CastGroup.CastGroupId));

            // Add inheritance structure for item tree
            modelBuilder.Entity<Item>();
            modelBuilder.Entity<Section>();
            modelBuilder.Entity<ShowRoot>();

            // Configure many-many relationships
            // Navigation collections must exist in both directions, otherwise loading will
            // fail with ArgumentNullException "Value cannot be null. (Parameter 'member')".
            modelBuilder.Entity<CastGroup>()
                .HasMany(g => g.Requirements)
                .WithMany(r => r.UsedByCastGroups);
            modelBuilder.Entity<Role>()
                .HasMany(r => r.Requirements)
                .WithMany(r => r.UsedByRoles);
            modelBuilder.Entity<CombinedRequirement>()
                .HasMany(cr => cr.SubRequirements)
                .WithMany(r => r.UsedByCombinedRequirements);
            modelBuilder.Entity<Identifier>()
                .HasMany(id => id.Requirements)
                .WithMany(r => r.UsedByIdentifiers);

            // Add inheritance structure for requirements
            // Foreign keys are manually defined to avoid IndexOutOfRangeException being
            // thrown on DbSet.Load(). I suspect this is due to the object properties
            // being non-nullable on their concrete type, but nullable in the database,
            // as they need to be null for the other Requirement concrete types.
            modelBuilder.Entity<AgeRequirement>();
            modelBuilder.Entity<GenderRequirement>();
            modelBuilder.Entity<CastGroupRequirement>()
                .HasOne(cgr => cgr.RequiredGroup)
                .WithMany()
                .HasForeignKey(nameof(CastGroupRequirement.RequiredGroupId));
            modelBuilder.Entity<AbilityExactRequirement>()
                .CommonProperty(nameof(AbilityExactRequirement.Criteria.CriteriaId));
            modelBuilder.Entity<AbilityRangeRequirement>()
                .CommonProperty(nameof(AbilityRangeRequirement.Criteria.CriteriaId));
            modelBuilder.Entity<NotRequirement>()
                .HasOne(nr => nr.SubRequirement)
                .WithMany()
                .HasForeignKey(nameof(NotRequirement.SubRequirementId));
            modelBuilder.Entity<AndRequirement>()
                .CommonProperty(nameof(AndRequirement.AverageSuitability));
            modelBuilder.Entity<OrRequirement>()
                .CommonProperty(nameof(OrRequirement.AverageSuitability));
            modelBuilder.Entity<XorRequirement>();

            // Add inheritance structure for critiera
            modelBuilder.Entity<NumericCriteria>();
            modelBuilder.Entity<SelectCriteria>()
                .Property(sc => sc.Options)
                .HasConversion(obj => JsonSerializer.Serialize(obj, null), // store array as json
                      json => JsonSerializer.Deserialize<string[]>(json, null) ?? SelectCriteria.DEFAULT_OPTIONS);
            modelBuilder.Entity<BooleanCriteria>();
        }
    }
}
