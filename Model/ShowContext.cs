using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Model
{
    public class ShowContext : DbContext
    {
        public DbSet<Show> Shows => Set<Show>();

        //TODO public Show ShowById(int show_id) => Shows.Where(s => s.ShowId == show_id).Single();

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
