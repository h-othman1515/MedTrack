using MedTrackJordan.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace MedTrackJordan.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Pharmacy> Pharmacies { get; set; }
        public DbSet<Drug> Drugs { get; set; }
        public DbSet<MedicationBatch> MedicationBatches { get; set; }
        public DbSet<ExpiryAlert> ExpiryAlerts { get; set; }
        public DbSet<SurplusPost> SurplusPosts { get; set; }
        public DbSet<TransferRequest> TransferRequests { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<RestockRequest> RestockRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ── Pharmacy ────────────────────────────────────────────────
            builder.Entity<Pharmacy>(entity =>
            {
                entity.HasIndex(p => p.LicenseNo).IsUnique();

                entity.Property(p => p.Name).IsRequired().HasMaxLength(200);
                entity.Property(p => p.LicenseNo).IsRequired().HasMaxLength(100);
                entity.Property(p => p.Governorate).IsRequired().HasMaxLength(100);
                entity.Property(p => p.Address).HasMaxLength(300);
                entity.Property(p => p.ContactEmail).HasMaxLength(200);
                entity.Property(p => p.Phone).HasMaxLength(20);
            });

            // ── Drug ─────────────────────────────────────────────────────
            builder.Entity<Drug>(entity =>
            {
                entity.Property(d => d.GenericName).IsRequired().HasMaxLength(200);
                entity.Property(d => d.Category).HasMaxLength(100);
                entity.Property(d => d.Unit).HasMaxLength(50);
                entity.Property(d => d.MinStockLevel).HasDefaultValue(0);
            });

            // ── ApplicationUser → Pharmacy ───────────────────────────────
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(u => u.FullName).HasMaxLength(200);

                entity.HasOne(u => u.Pharmacy)
                      .WithMany(p => p.Users)
                      .HasForeignKey(u => u.PharmacyId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // ── MedicationBatch ──────────────────────────────────────────
            builder.Entity<MedicationBatch>(entity =>
            {
                entity.Property(b => b.BatchNo).HasMaxLength(100);
                entity.Property(b => b.AddedAt).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(b => b.Pharmacy)
                      .WithMany(p => p.MedicationBatches)
                      .HasForeignKey(b => b.PharmacyId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(b => b.Drug)
                      .WithMany(d => d.MedicationBatches)
                      .HasForeignKey(b => b.DrugId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ── ExpiryAlert ──────────────────────────────────────────────
            builder.Entity<ExpiryAlert>(entity =>
            {
                entity.Property(a => a.AlertLevel)
                      .HasConversion<int>();

                entity.Property(a => a.Channel)
                      .HasConversion<string>()
                      .HasMaxLength(20);

                entity.Property(a => a.IsAcknowledged).HasDefaultValue(false);

                entity.HasOne(a => a.MedicationBatch)
                      .WithMany(b => b.ExpiryAlerts)
                      .HasForeignKey(a => a.BatchId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ── SurplusPost ──────────────────────────────────────────────
            builder.Entity<SurplusPost>(entity =>
            {
                entity.Property(s => s.Status)
                      .HasConversion<string>()
                      .HasMaxLength(20);

                entity.Property(s => s.PostedAt).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(s => s.Pharmacy)
                      .WithMany(p => p.SurplusPosts)
                      .HasForeignKey(s => s.PharmacyId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(s => s.Drug)
                      .WithMany(d => d.SurplusPosts)
                      .HasForeignKey(s => s.DrugId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ── TransferRequest ──────────────────────────────────────────
            builder.Entity<TransferRequest>(entity =>
            {
                entity.Property(t => t.Status)
                      .HasConversion<string>()
                      .HasMaxLength(20);

                entity.Property(t => t.RequestedAt).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(t => t.SurplusPost)
                      .WithMany(s => s.TransferRequests)
                      .HasForeignKey(t => t.SurplusPostId)
                      .OnDelete(DeleteBehavior.Cascade);

                // NoAction to avoid multiple cascade paths from Pharmacy
                entity.HasOne(t => t.RequestingPharmacy)
                      .WithMany(p => p.TransferRequests)
                      .HasForeignKey(t => t.RequestingPharmacyId)
                      .OnDelete(DeleteBehavior.NoAction);
            });

            // ── Notification ─────────────────────────────────────────────
            builder.Entity<Notification>(entity =>
            {
                entity.Property(n => n.Type)
                      .HasConversion<string>()
                      .HasMaxLength(30);

                entity.Property(n => n.Channel)
                      .HasConversion<string>()
                      .HasMaxLength(20);

                entity.Property(n => n.Message).HasMaxLength(1000);
                entity.Property(n => n.SentAt).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(n => n.Pharmacy)
                      .WithMany(p => p.Notifications)
                      .HasForeignKey(n => n.PharmacyId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ── RestockRequest ───────────────────────────────────────────
            builder.Entity<RestockRequest>(entity =>
            {
                entity.Property(r => r.Status)
                      .HasConversion<string>()
                      .HasMaxLength(20);

                entity.Property(r => r.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(r => r.Pharmacy)
                      .WithMany(p => p.RestockRequests)
                      .HasForeignKey(r => r.PharmacyId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(r => r.Drug)
                      .WithMany(d => d.RestockRequests)
                      .HasForeignKey(r => r.DrugId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}