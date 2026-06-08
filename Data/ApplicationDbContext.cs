using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MedTrack.Models;

namespace MedTrack.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Pharmacy> Pharmacies => Set<Pharmacy>();
        public DbSet<Drug> Drugs => Set<Drug>();
        public DbSet<MedicationBatch> MedicationBatches => Set<MedicationBatch>();
        public DbSet<ExpiryAlert> ExpiryAlerts => Set<ExpiryAlert>();
        public DbSet<SurplusPost> SurplusPosts => Set<SurplusPost>();
        public DbSet<TransferRequest> TransferRequests => Set<TransferRequest>();
        public DbSet<RestockRequest> RestockRequests => Set<RestockRequest>();
        public DbSet<Notification> Notifications => Set<Notification>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Pharmacy>(e =>
            {
                e.HasIndex(p => p.LicenseNo).IsUnique();
                e.HasIndex(p => p.Governorate);
                e.HasIndex(p => p.IsApproved);
            });

            builder.Entity<Drug>(e =>
            {
                e.HasIndex(d => d.GenericName);
                e.HasIndex(d => d.Category);
                e.HasIndex(d => d.IsActive);
            });

            builder.Entity<MedicationBatch>(e =>
            {
                e.HasIndex(b => b.PharmacyId);
                e.HasIndex(b => b.DrugId);
                e.HasIndex(b => b.ExpiryDate);
                e.HasIndex(b => b.Quantity);
            });

            builder.Entity<ExpiryAlert>(e =>
            {
                e.HasIndex(a => a.BatchId);
                e.HasIndex(a => a.AlertLevel);
                e.HasIndex(a => a.IsAcknowledged);
            });

            builder.Entity<SurplusPost>(e =>
            {
                e.HasIndex(s => s.PharmacyId);
                e.HasIndex(s => s.DrugId);
                e.HasIndex(s => s.Status);
                e.HasIndex(s => s.PostedAt);
            });

            builder.Entity<TransferRequest>(e =>
            {
                e.HasIndex(t => t.SurplusPostId);
                e.HasIndex(t => t.RequestingPharmacyId);
                e.HasIndex(t => t.Status);

                // FIX: Break multiple cascade paths from Pharmacies to TransferRequests
                e.HasOne(t => t.RequestingPharmacy)
                 .WithMany() // Adjust to .WithMany(p => p.TransferRequests) if your Pharmacy model has that collection property
                 .HasForeignKey(t => t.RequestingPharmacyId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<RestockRequest>(e =>
            {
                e.HasIndex(r => r.PharmacyId);
                e.HasIndex(r => r.DrugId);
                e.HasIndex(r => r.Status);
            });

            builder.Entity<Notification>(e =>
            {
                e.HasIndex(n => n.PharmacyId);
                e.HasIndex(n => n.IsRead);
                e.HasIndex(n => n.SentAt);
            });

            builder.Entity<AuditLog>(e =>
            {
                e.HasIndex(a => a.Timestamp);
                e.HasIndex(a => a.UserId);
                e.HasIndex(a => a.EntityType);
            });
        }
    }
}