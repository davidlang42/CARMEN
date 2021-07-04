﻿using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Model
{
    public class ShowContext : DbContext
    {
        public DbSet<Applicant> Applicants => Set<Applicant>();
        public DbSet<Section> Sections => Set<Section>();
        public DbSet<Item> Items => Set<Item>();

        public ShowContext(DbContextOptions<ShowContext> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseLazyLoadingProxies();
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Ability>()
                .HasKey(c => new { c.ApplicantId, c.CriteriaId });
            modelBuilder.Entity<CountByGroup>()
                .HasKey(c => new { c.RoleId, c.CastGroupId });
        }
    }
}
