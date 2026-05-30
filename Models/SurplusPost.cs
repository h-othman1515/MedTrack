using System.ComponentModel.DataAnnotations;

namespace MedTrackJordan.Models
{
    public enum SurplusStatus
    {
        Available,
        Reserved,
        Completed
    }

    public class SurplusPost
    {
        public int Id { get; set; }

        public int PharmacyId { get; set; }
        public int DrugId { get; set; }

        [Required]
        public int Quantity { get; set; }

        public DateTime? ExpiryDate { get; set; }

        public SurplusStatus Status { get; set; } = SurplusStatus.Available;

        public DateTime PostedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Pharmacy Pharmacy { get; set; } = null!;
        public Drug Drug { get; set; } = null!;
        public ICollection<TransferRequest> TransferRequests { get; set; } = new List<TransferRequest>();
    }
}