using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Linq;
using System.Text.Json;
using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Criterias;
using Carmen.ShowModel.Requirements;
using Carmen.ShowModel.Structure;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;
using System.IO;
using System;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Carmen.ShowModel
{
    public class ShowContext : DbContext
    {
        public enum DatabaseState
        {
            Empty,
            UpToDate,
            SavedWithPreviousVersion,
            SavedWithFutureVersion
        }

        #region Database collections
        /// <summary>Auto includes ShowRoot, Abilities, CastGroup, AlternativeCast, Tags.
        /// Remember to include Roles, SameCastSet, Image.</summary>
        public DbSet<Applicant> Applicants => Set<Applicant>();

        /// <summary>Remember to include Members.</summary>
        public DbSet<AlternativeCast> AlternativeCasts => Set<AlternativeCast>();

        /// <summary>Remember to include Members, Requirements.</summary>
        public DbSet<CastGroup> CastGroups => Set<CastGroup>();

        /// <summary>Auto includes Applicants.</summary>
        public DbSet<SameCastSet> SameCastSets => Set<SameCastSet>();

        /// <summary>Auto includes CountByGroups, CountByGroup.CastGroup.
        /// Remember to include Image, Members, Requirements.</summary>
        public DbSet<Tag> Tags => Set<Tag>();

        /// <summary>Remember to include Abilities.</summary>
        public DbSet<Criteria> Criterias => Set<Criteria>();

        /// <summary>Auto includes TagRequirement.RequiredTag, AbilityExactRequirement.Criteria, AbilityRangeRequirement.Criteria.
        /// Remember to include UsedByRoles, UsedByCastGroups, UsedByCombinedRequirements, UsedByTags, CombinedRequirement.SubRequirements, NotRequirement.SubRequirement.</summary>
        public DbSet<Requirement> Requirements => Set<Requirement>();

        /// <summary>Auto includes CountByGroups, CountByGroup.CastGroup, Section.SectionType.
        /// Remember to include Parent, InnerNode.Children, Item.Roles, ShowRoot.Logo, ShowRoot.CastNumberOrderBy.</summary>
        public DbSet<Node> Nodes => Set<Node>();
        
        /// <summary>Remember to include Sections.</summary>
        public DbSet<SectionType> SectionTypes => Set<SectionType>();

        /// <summary>Nothing to include.</summary>
        public DbSet<Image> Images => Set<Image>();

        /// <summary>Auto includes CountByGroups, CountByGroup.CastGroup, Requirements.
        /// Remember to include Items, Cast.</summary>
        public DbSet<Role> Roles => Set<Role>();

        /// <summary>Auto includes Criteria.
        /// Remember to include Applicant.</summary>
        public DbSet<Ability> Abilities => Set<Ability>();
        #endregion

        private ShowRoot? showRoot;
        /// <summary>The root node of the show structure</summary>
        public ShowRoot ShowRoot => showRoot ??= Nodes.OfType<ShowRoot>().SingleOrDefault() ?? Add(new ShowRoot()).Entity;

        public ShowConnection Connection { get; private init; }

        public ShowContext(ShowConnection show_connection)
        {
            Connection = show_connection;
        }

        /// <summary>Accessing any model or entity related property on the context causes the model to be build,
        /// which takes ~1s synchronously. This runs it in a separate thread to avoid a synchronous delay.</summary>
        public async Task PreloadModel()
        {
            await Task.Run(() => _ = this.Model);
        }

        public async Task CreateNewDatabase(string default_show_name)
        {
            await Task.Run(() => // see EntityFrameworkQueryableExtensionsWithGuaranteedAsync
            {
                Database.EnsureDeleted();
                Database.Migrate(); // instead of EnsureCreated(), so that history table gets created
                SetDefaultShowSettings(default_show_name);
                SaveChanges();
            });
        }

        public async Task<DatabaseState> CheckDatabaseState()
        {
            return await Task.Run(() => // see EntityFrameworkQueryableExtensionsWithGuaranteedAsync
            {
                var all_migrations = Database.GetMigrations().ToList();
                var applied_migrations = Database.GetAppliedMigrations().ToList();
                if (all_migrations.Any() && !applied_migrations.Any())
                    return DatabaseState.Empty;
                var pending_migrations = all_migrations.Except(applied_migrations);
                var future_migrations = applied_migrations.Except(all_migrations);
                if (future_migrations.Any())
                    return DatabaseState.SavedWithFutureVersion;
                if (pending_migrations.Any())
                    return DatabaseState.SavedWithPreviousVersion;
                return DatabaseState.UpToDate;
            });
        }

        public async Task UpgradeDatabase()
        {
            await Task.Run(() => Database.Migrate()); // see EntityFrameworkQueryableExtensionsWithGuaranteedAsync
        }

        /// <summary>Detect which DataObjects have changed in the current context since the last save</summary>
        public DataObjects DataChanges()
        {
            var changes = DataObjects.None;
            foreach (var entry in ChangeTracker.Entries().Where(e => e.State != EntityState.Unchanged))
            {
                if (entry.Entity is Applicant)
                    changes |= DataObjects.Applicants;
                else if (entry.Entity is Ability)
                    changes |= DataObjects.Abilities;
                else if (entry.Entity is AlternativeCast)
                    changes |= DataObjects.AlternativeCasts;
                else if (entry.Entity is CastGroup)
                    changes |= DataObjects.CastGroups;
                else if (entry.Entity is SameCastSet)
                    changes |= DataObjects.SameCastSets;
                else if (entry.Entity is Tag)
                    changes |= DataObjects.Tags;
                else if (entry.Entity is Criteria)
                    changes |= DataObjects.Criterias;
                else if (entry.Entity is Requirement)
                    changes |= DataObjects.Requirements;
                else if (entry.Entity is Node)
                    changes |= DataObjects.Nodes;
                else if (entry.Entity is Role)
                    changes |= DataObjects.Roles;
                else if (entry.Entity is SectionType)
                    changes |= DataObjects.SectionTypes;
                else if (entry.Entity is Image)
                    changes |= DataObjects.Images;
                else if (entry.Entity is CountByGroup)
                    changes |= entry.Metadata.DefiningEntityType.ClrType switch
                    {
                        Type t when t == typeof(Node) => DataObjects.Nodes,
                        Type t when t == typeof(Role) => DataObjects.Roles,
                        Type t when t == typeof(Tag) => DataObjects.Tags,
                        _ => throw new NotImplementedException($"Owner of CountByGroup not handled: {entry.Metadata.DefiningEntityType.ClrType.Name}")
                    };
                else if (entry.Entity is Dictionary<string, object> linking_table)
                {
                    if (linking_table.ContainsKey(nameof(Roles) + nameof(Role.RoleId)))
                        changes |= DataObjects.Roles;
                    if (linking_table.ContainsKey(nameof(Tags) + nameof(Tag.TagId)))
                        changes |= DataObjects.Tags;
                }
            }
            return changes;
        }

        /// <summary>For any current members of this alternative cast, this sets their alternative cast to null.
        /// If deleting this alternative cast brings the total number below 2, then any CastGroup which has
        /// AlternateCasts set to true, will have it set to false, and the alternative cast of their current
        /// members also set to null.</summary>
        public void DeleteAlternativeCast(AlternativeCast alternative_cast)
        {
            foreach (var member in alternative_cast.Members.ToArray())
                member.AlternativeCast = null;
            var alternative_casts = AlternativeCasts.ToArray();
            if (alternative_casts.Length <= 2)
            {
                foreach (var cast_group in CastGroups.ToArray())
                {
                    if (cast_group.AlternateCasts)
                    {
                        foreach (var other_cast_member in cast_group.Members)
                            other_cast_member.AlternativeCast = null;
                        cast_group.AlternateCasts = false;
                    }
                }
            }
            AlternativeCasts.Remove(alternative_cast);
        }

        /// <summary>Also deletes any sections currently set to this section type, any items or sections under
        /// those sections, any roles which are no longer in any items, and unsets the cast allocated to those roles.</summary>
        public void DeleteSectionType(SectionType section_type)
        {
            while (section_type.Sections.Any())
            {
                // Deleting this section may have also deleted sub-sections which are of this section type,
                // therefore after every delete, re-check the section_type for any sections and take the first.
                DeleteNode(section_type.Sections.First()); 
            }
            SectionTypes.Remove(section_type);
        }

        /// <summary>Also deletes any child nodes under this node, any roles which are no longer in any items, and unsets the
        /// cast allocated to those roles.</summary>
        public void DeleteNode(Node node)
        {
            if (node is ShowRoot)
                throw new ArgumentException("Cannot delete ShowRoot.");
            else if (node is InnerNode inner)
            {
                foreach (var child in inner.Children.ToArray())
                    DeleteNode(child);
            }
            else if (node is Item item)
            {
                foreach (var role in item.Roles.ToArray())
                    RemoveRole(role, item);
            }
            else
                throw new NotImplementedException($"Node type not handled: {node.GetType().Name}");
            var parent = node.Parent ?? throw new ApplicationException("Non-ShowRoot node must have a parent.");
            parent.Children.Remove(node);
            Nodes.Remove(node);
            if (node is Section section)
                section.SectionType.Sections.Remove(section);
        }

        /// <summary>Removes the role from this item only, but if the role is no longer in any items, this also
        /// deletes the roles and unallocates any cast from it.</summary>
        public void RemoveRole(Role role, Item item)
        {
            role.Items.Remove(item);
            item.Roles.Remove(role);
            if (!role.Items.Any())
            {
                var entry = Entry(role);
                if (entry.State != EntityState.Detached)
                    entry.State = EntityState.Deleted;
            }
        }

        /// <summary>Also deletes any NOT requirements which use this requirement, because a NOT requirement must have a
        /// sub-requirement set.</summary>
        public void DeleteRequirement(Requirement requirement)
        {
            var used_by_not_requirements = Requirements.Local.OfType<NotRequirement>().Where(nr => nr.SubRequirement == requirement).ToArray();
            foreach (var not_requirement in used_by_not_requirements)
                DeleteRequirement(not_requirement);
            Requirements.Remove(requirement);
        }

        /// <summary>Also sets the SameCastSet of all applicants in the set to null</summary>
        public void DeleteSameCastSet(SameCastSet same_cast_set)
        {
            foreach (var applicant in same_cast_set.Applicants.ToArray())
                applicant.SameCastSet = null;
            SameCastSets.Remove(same_cast_set);
        }

        /// <summary>Configures the default AlternativeCasts, CastGroups, Tags, Criterias, Requirements, SectionTypes and ShowRoot.
        /// Must match the logic of CheckDefaultShowSettings().</summary>
        public void SetDefaultShowSettings(string default_show_name, bool load_required = true)
        {
            if (load_required)
            {
                AlternativeCasts.Load();
                CastGroups.Load();
                Tags.Load();
                Criterias.Load();
                Requirements.Load();
                SectionTypes.Load();
            }
            AlternativeCasts.Local.Clear();
            CastGroups.Local.Clear();
            CastGroups.Add(new CastGroup { Name = "Cast" });
            Tags.Local.Clear();
            Criterias.Local.Clear();
            Requirements.Local.Clear();
            SectionTypes.Local.Clear();
            SectionTypes.Add(new SectionType { Name = "Section" });
            ShowRoot.Name = default_show_name;
            ShowRoot.ShowDate = null;
            ShowRoot.AllowConsecutiveItems = false;
            ShowRoot.CastNumberOrderBy = null;
            ShowRoot.CastNumberOrderDirection = ListSortDirection.Ascending;
            ShowRoot.Logo = null;
        }

        /// <summary>Returns true if show settings match the default values.
        /// Must match the logic of SetDefaultShowSettings().</summary>
        public bool CheckDefaultShowSettings(string default_show_name, bool load_required = true)
        {
            var show_root_match = ShowRoot.Name == default_show_name
                && ShowRoot.ShowDate == null
                && ShowRoot.AllowConsecutiveItems == false
                && ShowRoot.CastNumberOrderBy == null
                && ShowRoot.CastNumberOrderDirection == ListSortDirection.Ascending
                && ShowRoot.Logo == null;
            if (!show_root_match)
                return false;
            if (load_required)
                AlternativeCasts.Load();
            if (AlternativeCasts.Local.Any())
                return false;
            if (load_required)
                CastGroups.Load();
            var cast_group_match = CastGroups.Local.SingleOrDefaultSafe() is CastGroup single_cast_group
                && single_cast_group.Name == "Cast"
                && single_cast_group.Abbreviation == "Cast"
                && single_cast_group.AlternateCasts == false
                && single_cast_group.RequiredCount == null
                && single_cast_group.Requirements.Count == 0;
            if (!cast_group_match)
                return false;
            if (load_required)
                Tags.Load();
            if (Tags.Local.Any())
                return false;
            if (load_required)
                Criterias.Load();
            if (Criterias.Local.Any())
                return false;
            if (load_required)
                Requirements.Load();
            if (Requirements.Local.Any())
                return false;
            if (load_required)
                SectionTypes.Load();
            var section_type_match = SectionTypes.Local.SingleOrDefaultSafe() is SectionType single_section_type
                && single_section_type.Name == "Section"
                && single_section_type.AllowConsecutiveItems == true
                && single_section_type.AllowMultipleRoles == false
                && single_section_type.AllowNoRoles == false;
            return section_type_match;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (Connection.Provider == null) // Sqlite
                optionsBuilder
                    .UseSqlite(Connection.ConnectionString)
                    .ReplaceService<IHistoryRepository, CustomSqliteHistoryRepository>();
            else if (Connection.Provider == DbProvider.MySql) // MySql or MariaDb
                optionsBuilder
                    .UseMySql(Connection.ConnectionString, ServerVersion.AutoDetect(Connection.ConnectionString))
                    .ReplaceService<IHistoryRepository, CustomMySqlHistoryRepository>();
            else
                throw new NotImplementedException($"Database provider {Connection.Provider} not implemented.");

            // Using LazyLoadingProxies will have a huge performance hit if I forget to include
            // the right objects in my queries, however the alternative is that if I forget to
            // include something then I get incorrect values (eg. nulls and empty collections)
            optionsBuilder.UseLazyLoadingProxies();

#if SLOW_DATABASE
            // Due to risks of LazyLoadingProxies above, it is best to always debug with SLOW_DATABASE
            // so that there is a noticeable delay if I forget to include the right objects, however
            // its worth noting that a delay of 200ms blocks unit tests from completing.
            optionsBuilder.AddInterceptors(new DelayInterceptor(200)); // simulates a 3g connection
#endif
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure composite keys
            modelBuilder.Entity<Ability>()
                .HasKey(nameof(Ability.Applicant.ApplicantId), nameof(Ability.Criteria.CriteriaId));

            // Configure owned entities
            var role_cbg = modelBuilder.Entity<Role>().OwnsMany(r => r.CountByGroups);
            role_cbg.WithOwnerCompositeKey(nameof(Role.RoleId), nameof(CountByGroup.CastGroup.CastGroupId));
            role_cbg.Navigation(cbg => cbg.CastGroup).AutoInclude();
            var node_cbg = modelBuilder.Entity<Node>().OwnsMany(n => n.CountByGroups);
            node_cbg.WithOwnerCompositeKey(nameof(Node.NodeId), nameof(CountByGroup.CastGroup.CastGroupId));
            node_cbg.Navigation(cbg => cbg.CastGroup).AutoInclude();
            var tag_cbg = modelBuilder.Entity<Tag>().OwnsMany(r => r.CountByGroups);
            tag_cbg.WithOwnerCompositeKey(nameof(Tag.TagId), nameof(CountByGroup.CastGroup.CastGroupId));
            tag_cbg.Navigation(cbg => cbg.CastGroup).AutoInclude();

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
                .CommonProperty(nameof(AbilityExactRequirement.Criteria.CriteriaId))
                .CommonProperty(nameof(AbilityExactRequirement.ExistingRoleCost));
            modelBuilder.Entity<AbilityRangeRequirement>()
                .CommonProperty(nameof(AbilityRangeRequirement.Criteria.CriteriaId))
                .CommonProperty(nameof(AbilityRangeRequirement.ExistingRoleCost));
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
            modelBuilder.Entity<Applicant>().Navigation(a => a.ShowRoot).AutoInclude();
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
            modelBuilder.Entity<Role>().Navigation(r => r.Requirements).AutoInclude();
            modelBuilder.Entity<Section>().Navigation(s => s.SectionType).AutoInclude();
            modelBuilder.Entity<SameCastSet>().Navigation(set => set.Applicants).AutoInclude();
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
                //LATER to avoid/detect unnessesary lazy loading, I could throw an exception when ReaderExecuting syncrhonously,
                // because I always (or should always) use Async when I mean to load from the database. This may be helpful as a diagnostic tool
                // to be enabled whenever building for debug.
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
