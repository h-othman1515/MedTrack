using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedTrack.Models
{
    public class Pharmacy
    {
        [Key] public int Id { get; set; }
        [Required, StringLength(200)] public string Name { get; set; } = string.Empty;
        [Required, StringLength(50)] public string LicenseNo { get; set; } = string.Empty;
        [Required, StringLength(100)] public string Governorate { get; set; } = string.Empty;
        [StringLength(500)] public string? Address { get; set; }
        [StringLength(200)] public string? ContactEmail { get; set; }
        [StringLength(20)] public string? Phone { get; set; }
        public PharmacyTier Tier { get; set; } = PharmacyTier.Basic;
        public bool IsApproved { get; set; } = false;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<ApplicationUser> Staff { get; set; } = new List<ApplicationUser>();
        public ICollection<MedicationBatch> MedicationBatches { get; set; } = new List<MedicationBatch>();
        public ICollection<SurplusPost> SurplusPosts { get; set; } = new List<SurplusPost>();
        public ICollection<RestockRequest> RestockRequests { get; set; } = new List<RestockRequest>();
    }

    public class Drug
    {
        [Key] public int Id { get; set; }
        [Required, StringLength(200)] public string GenericName { get; set; } = string.Empty;
        [StringLength(500)] public string? BrandNames { get; set; }
        [StringLength(100)] public string? Category { get; set; }
        public int DefaultMinStockLevel { get; set; } = 10;
        [StringLength(50)] public string? DefaultUnit { get; set; } = "Tablets";
        public bool RequiresPrescription { get; set; } = false;
        public bool IsCritical { get; set; } = false;
        public bool IsActive { get; set; } = true;
        public ICollection<MedicationBatch> MedicationBatches { get; set; } = new List<MedicationBatch>();
    }

    public class MedicationBatch
    {
        [Key] public int Id { get; set; }
        [Required] public int PharmacyId { get; set; }
        [ForeignKey("PharmacyId")] public Pharmacy Pharmacy { get; set; } = null!;
        [Required] public int DrugId { get; set; }
        [ForeignKey("DrugId")] public Drug Drug { get; set; } = null!;
        [Required] public int Quantity { get; set; }
        [StringLength(100)] public string? BatchNo { get; set; }
        [Required] public DateTime ExpiryDate { get; set; }
        public int MinStockLevel { get; set; } = 10;
        [StringLength(50)] public string? Unit { get; set; } = "Tablets";
        [Column(TypeName = "decimal(18,2)")] public decimal? UnitPrice { get; set; }
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
        public ICollection<ExpiryAlert> ExpiryAlerts { get; set; } = new List<ExpiryAlert>();
    }

    public class ExpiryAlert
    {
        [Key] public int Id { get; set; }
        [Required] public int BatchId { get; set; }
        [ForeignKey("BatchId")] public MedicationBatch MedicationBatch { get; set; } = null!;
        public AlertLevel AlertLevel { get; set; } = AlertLevel._60;
        public DateTime? SentAt { get; set; }
        public NotificationChannel? Channel { get; set; }
        public bool IsAcknowledged { get; set; } = false;
        public DateTime? AcknowledgedAt { get; set; }
    }

    public class SurplusPost
    {
        [Key] public int Id { get; set; }
        [Required] public int PharmacyId { get; set; }
        [ForeignKey("PharmacyId")] public Pharmacy Pharmacy { get; set; } = null!;
        [Required] public int DrugId { get; set; }
        [ForeignKey("DrugId")] public Drug Drug { get; set; } = null!;
        [Required] public int Quantity { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public SurplusStatus Status { get; set; } = SurplusStatus.Available;
        [StringLength(500)] public string? Condition { get; set; }
        [Column(TypeName = "decimal(18,2)")] public decimal? Price { get; set; }
        public DateTime PostedAt { get; set; } = DateTime.UtcNow;
        public ICollection<TransferRequest> TransferRequests { get; set; } = new List<TransferRequest>();
    }

    public class TransferRequest
    {
        [Key] public int Id { get; set; }
        [Required] public int SurplusPostId { get; set; }
        [ForeignKey("SurplusPostId")] public SurplusPost SurplusPost { get; set; } = null!;
        [Required] public int RequestingPharmacyId { get; set; }
        [ForeignKey("RequestingPharmacyId")] public Pharmacy RequestingPharmacy { get; set; } = null!;
        public int Quantity { get; set; }
        public TransferStatus Status { get; set; } = TransferStatus.Pending;
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
        [StringLength(500)] public string? Notes { get; set; }
    }

    public class RestockRequest
    {
        [Key] public int Id { get; set; }
        [Required] public int PharmacyId { get; set; }
        [ForeignKey("PharmacyId")] public Pharmacy Pharmacy { get; set; } = null!;
        [Required] public int DrugId { get; set; }
        [ForeignKey("DrugId")] public Drug Drug { get; set; } = null!;
        public int RequestedQuantity { get; set; }
        public RestockStatus Status { get; set; } = RestockStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ConfirmedAt { get; set; }
    }

    public class Notification
    {
        [Key] public int Id { get; set; }
        [Required] public int PharmacyId { get; set; }
        [ForeignKey("PharmacyId")] public Pharmacy Pharmacy { get; set; } = null!;
        public NotificationType Type { get; set; } = NotificationType.Expiry;
        public NotificationChannel? Channel { get; set; }
        [StringLength(500)] public string Message { get; set; } = string.Empty;
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; } = false;
        public DateTime? ReadAt { get; set; }
    }

    public class AuditLog
    {
        [Key] public int Id { get; set; }
        [StringLength(100)] public string? UserId { get; set; }
        [StringLength(100)] public string? UserName { get; set; }
        [StringLength(50)] public string Action { get; set; } = string.Empty;
        [StringLength(50)] public string EntityType { get; set; } = string.Empty;
        [StringLength(500)] public string Details { get; set; } = string.Empty;
        [StringLength(50)] public string? IpAddress { get; set; }
        [StringLength(20)] public string Severity { get; set; } = "Low";
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}