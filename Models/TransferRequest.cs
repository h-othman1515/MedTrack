using System.ComponentModel.DataAnnotations;

namespace MedTrackJordan.Models
{
    public enum TransferStatus
    {
        Pending,
        Confirmed,
        Completed,
        Cancelled
    }

    public class TransferRequest
    {
        public int Id { get; set; }

        public int SurplusPostId { get; set; }
        public int RequestingPharmacyId { get; set; }

        public TransferStatus Status { get; set; } = TransferStatus.Pending;

        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

        public DateTime? CompletedAt { get; set; }

        // Navigation properties
        public SurplusPost SurplusPost { get; set; } = null!;
        public Pharmacy RequestingPharmacy { get; set; } = null!;
    }
}