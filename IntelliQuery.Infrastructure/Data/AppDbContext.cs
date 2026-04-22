using System;
using System.Collections.Generic;
using System.Text;
using IntelliQuery.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IntelliQuery.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Dataset> Datasets => Set<Dataset>();
        public DbSet<DatasetFile> DatasetFiles => Set<DatasetFile>();
        public DbSet<ImportJob> ImportJobs => Set<ImportJob>();
        public DbSet<DatasetColumn> DatasetColumns => Set<DatasetColumn>();
        public DbSet<DatasetRow> DatasetRows => Set<DatasetRow>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }
    }
}
