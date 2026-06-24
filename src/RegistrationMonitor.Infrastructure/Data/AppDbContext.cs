using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
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

        public DbSet<Subscriber> Subscribers => Set<Subscriber>();

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

            modelBuilder.Entity<Subscriber>(entity =>
            {
                entity.HasKey(s => s.Id);

                entity.Property(s => s.ChatId).IsRequired();

                entity.Property(s => s.SubscribedAt).IsRequired().HasConversion<string>();

                entity.Property(s => s.IsActive).IsRequired();

                entity.Property(s => s.Username).HasMaxLength(100);

                entity.HasIndex(s => s.ChatId).IsUnique();

                entity.ToTable("Subscribers");
            });
        }
    }
}
