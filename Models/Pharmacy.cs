using System.ComponentModel.DataAnnotations;

namespace MedTrackJordan.Models
{
    public class Pharmacy
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string LicenseNo { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Governorate { get; set; } = string.Empty;

        [MaxLength(300)]
        public string? Address { get; set; }

        [EmailAddress, MaxLength(200)]
        public string? ContactEmail { get; set; }

        [MaxLength(20)]
        public string? Phone { get; set; }

        // Navigation properties
        public ICollection<MedicationBatch> MedicationBatches { get; set; } = new List<MedicationBatch>();
        public ICollection<SurplusPost> SurplusPosts { get; set; } = new List<SurplusPost>();
        public ICollection<TransferRequest> TransferRequests { get; set; } = new List<TransferRequest>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public ICollection<RestockRequest> RestockRequests { get; set; } = new List<RestockRequest>();
        public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
    }
}