using System.ComponentModel.DataAnnotations;

namespace MedTrackJordan.Models
{
    public enum RestockStatus
    {
        Pending,
        Sent,
        Fulfilled
    }

    public class RestockRequest
    {
        public int Id { get; set; }

        public int PharmacyId { get; set; }
        public int DrugId { get; set; }

        [Required]
        public int RequestedQuantity { get; set; }

        public RestockStatus Status { get; set; } = RestockStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Pharmacy Pharmacy { get; set; } = null!;
        public Drug Drug { get; set; } = null!;
    }
}