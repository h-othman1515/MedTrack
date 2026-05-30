using System.ComponentModel.DataAnnotations;

namespace MedTrackJordan.Models
{
    public class MedicationBatch
    {
        public int Id { get; set; }

        public int PharmacyId { get; set; }
        public int DrugId { get; set; }

        [Required]
        public int Quantity { get; set; }

        [MaxLength(100)]
        public string? BatchNo { get; set; }

        [Required]
        public DateTime ExpiryDate { get; set; }

        public DateTime AddedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Pharmacy Pharmacy { get; set; } = null!;
        public Drug Drug { get; set; } = null!;
        public ICollection<ExpiryAlert> ExpiryAlerts { get; set; } = new List<ExpiryAlert>();
    }
}