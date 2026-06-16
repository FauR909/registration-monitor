using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RegistrationMonitor.Core.Entities;
using RegistrationMonitor.Core.Models;

namespace RegistrationMonitor.Infrastructure.Data
{
    public sealed class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<StatusCheckRecord> StatusChecks => Set<StatusCheckRecord>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<StatusCheckRecord>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.CheckedAt).IsRequired().HasConversion<string>();

                entity.Property(e => e.DetectedStatus).IsRequired().HasConversion<int>();

                entity.Property(e => e.NotificationSent).IsRequired();

                entity.Property(e => e.SourceMessage).HasMaxLength(1000);

                entity.HasIndex(e => e.CheckedAt).HasDatabaseName("IX_StatusChecks_CheckedAt");

                entity.ToTable("StatusChecks");

            });
        }
    }
}
