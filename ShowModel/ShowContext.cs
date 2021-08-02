using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Linq;
using System.Text.Json;
using ShowModel.Applicants;
using ShowModel.Criterias;
using ShowModel.Requirements;
using ShowModel.Structure;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace ShowModel
{
    public class ShowContext : DbContext
    {
        #region Database collections
        public DbSet<Applicant> Applicants => Set<Applicant>();
        public DbSet<AlternativeCast> AlternativeCasts => Set<AlternativeCast>();
        public DbSet<CastGroup> CastGroups => Set<CastGroup>();
        public DbSet<Tag> Tags => Set<Tag>();
        public DbSet<Criteria> Criterias => Set<Criteria>();
        public DbSet<Requirement> Requirements => Set<Requirement>();
        public DbSet<Node> Nodes => Set<Node>();
        public DbSet<SectionType> SectionTypes => Set<SectionType>();
        public DbSet<Image> Images => Set<Image>();
        #endregion

        /// <summary>The root node of the show structure</summary>
        public ShowRoot ShowRoot => Nodes.OfType<ShowRoot>().SingleOrDefault() ?? Add(new ShowRoot()).Entity;

        public ShowContext(DbContextOptions<ShowContext> context_options) : base(context_options)
        { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseLazyLoadingProxies(); //LATER this has a huge performance risk, if I forget to include the right objects in my queries, however the alternative is I get incorrect values (eg. nulls and empty collections), which seems worse
#if SLOW_DATABASE
            // NOTE: Using a delay of 200 seems to block unit tests
            optionsBuilder.AddInterceptors(new DelayInterceptor(200)); // simulates a 3g connection
#endif
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder) //LATER may need to use this: .UsePropertyAccessMode(PropertyAccessMode.PreferFieldDuringConstruction);
        {
            // Configure composite keys
            modelBuilder.Entity<Ability>()
                .HasKey(nameof(Ability.Applicant.ApplicantId), nameof(Ability.Criteria.CriteriaId));

            // Configure owned entities
            modelBuilder.Entity<Role>()
                .OwnsMany(r => r.CountByGroups)
                .WithOwnerCompositeKey(nameof(Role.RoleId), nameof(CountByGroup.CastGroup.CastGroupId));
            modelBuilder.Entity<Node>()
                .OwnsMany(n => n.CountByGroups)
                .WithOwnerCompositeKey(nameof(Node.NodeId), nameof(CountByGroup.CastGroup.CastGroupId));
            modelBuilder.Entity<Tag>()
                .OwnsMany(r => r.CountByGroups)
                .WithOwnerCompositeKey(nameof(Tag.TagId), nameof(CountByGroup.CastGroup.CastGroupId));

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
            modelBuilder.Entity<Tag>()
                .HasMany(t => t.Requirements)
                .WithMany(r => r.UsedByTags);
            modelBuilder.Entity<Role>()
                .HasMany(r => r.Requirements)
                .WithMany(r => r.UsedByRoles);
            modelBuilder.Entity<CombinedRequirement>()
                .HasMany(cr => cr.SubRequirements)
                .WithMany(r => r.UsedByCombinedRequirements);
            modelBuilder.Entity<Applicant>()
                .HasMany(a => a.Roles)
                .WithMany(r => r.Cast);

            // Add inheritance structure for requirements
            // Foreign keys are manually defined to avoid IndexOutOfRangeException being
            // thrown on DbSet.Load(). I suspect this is due to the object properties
            // being non-nullable on their concrete type, but nullable in the database,
            // as they need to be null for the other Requirement concrete types.
            modelBuilder.Entity<AgeRequirement>();
            modelBuilder.Entity<GenderRequirement>();
            modelBuilder.Entity<TagRequirement>()
                .HasOne(cgr => cgr.RequiredTag)
                .WithMany()
                .HasForeignKey(nameof(TagRequirement.RequiredTagId));
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

            // Auto-include entities for normal use cases
            modelBuilder.Entity<Ability>().Navigation(ab => ab.Criteria).AutoInclude();
            modelBuilder.Entity<Applicant>().Navigation(a => a.Abilities).AutoInclude();
            modelBuilder.Entity<Applicant>().Navigation(a => a.CastGroup).AutoInclude();
            modelBuilder.Entity<Applicant>().Navigation(a => a.AlternativeCast).AutoInclude();
            modelBuilder.Entity<Applicant>().Navigation(a => a.Tags).AutoInclude();
            modelBuilder.Entity<TagRequirement>().Navigation(tr => tr.RequiredTag).AutoInclude();
            modelBuilder.Entity<AbilityExactRequirement>().Navigation(aer => aer.Criteria).AutoInclude();
            modelBuilder.Entity<AbilityRangeRequirement>().Navigation(arr => arr.Criteria).AutoInclude();
            modelBuilder.Entity<Tag>().Navigation(t => t.CountByGroups).AutoInclude();
            modelBuilder.Entity<Node>().Navigation(n => n.CountByGroups).AutoInclude();
            modelBuilder.Entity<Role>().Navigation(r => r.CountByGroups).AutoInclude();
        }

#if SLOW_DATABASE
        private class DelayInterceptor : DbCommandInterceptor
        {
            private int delay;

            public DelayInterceptor(int delay)
            {
                this.delay = delay;
            }

            public override InterceptionResult<DbDataReader> ReaderExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
            {
                Thread.Sleep(delay);
                return result;
            }

            public override async ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result, CancellationToken cancellationToken = default)
            {
                await Task.Run(() => Thread.Sleep(delay));
                return result;
            }
        }
#endif
    }
}
