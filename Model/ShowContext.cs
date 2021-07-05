using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Model
{
    public class ShowContext : DbContext
    {
        public DbSet<Applicant> Applicants => Set<Applicant>();
        public DbSet<CastGroup> CastGroups => Set<CastGroup>();
        public DbSet<Node> Nodes => Set<Node>();

        public IEnumerable<Node> RootNodes => Nodes.Where(n => n.Parent == null).InOrder();

        public IEnumerable<Item> ItemsInOrder() => RootNodes.SelectMany(n => n.ItemsInOrder());

        public ShowContext(DbContextOptions<ShowContext> options) : base(options)
        { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseLazyLoadingProxies();
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Ability>()
                .HasKey(a => new { a.ApplicantId, a.CriteriaId });
            modelBuilder.Entity<CountByGroup>()
                .HasKey(c => new { c.RoleId, c.CastGroupId });
            modelBuilder.Entity<Node>();
            modelBuilder.Entity<Item>();
            modelBuilder.Entity<Section>();
        }
    }
}
