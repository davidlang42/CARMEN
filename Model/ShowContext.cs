using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Model.Structure;
using Model.Requirements;
using Model.Criterias;
using System.Text.Json;

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
            // Create composite keys
            modelBuilder.Entity<Ability>()
                .HasKey(a => new { a.ApplicantId, a.CriteriaId });
            // Store private properties
            modelBuilder.Entity<CountByGroup>()
                .Property(CountByGroup.CountExpression);
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
                .Property(s => s.Options)
                .HasConversion(obj => JsonSerializer.Serialize(obj, null),
                      json => JsonSerializer.Deserialize<string[]>(json, null) ?? SelectCriteria.DEFAULT_OPTIONS);
            modelBuilder.Entity<BooleanCriteria>();
        }
    }
}
