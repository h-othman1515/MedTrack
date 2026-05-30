using System.ComponentModel.DataAnnotations;

namespace MedTrackJordan.Models
{
    public class Drug
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string GenericName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Category { get; set; }

        public int MinStockLevel { get; set; } = 0;

        [MaxLength(50)]
        public string? Unit { get; set; }

        // Navigation properties
        public ICollection<MedicationBatch> MedicationBatches { get; set; } = new List<MedicationBatch>();
        public ICollection<SurplusPost> SurplusPosts { get; set; } = new List<SurplusPost>();
        public ICollection<RestockRequest> RestockRequests { get; set; } = new List<RestockRequest>();
    }
}