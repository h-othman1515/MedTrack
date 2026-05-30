using System.ComponentModel.DataAnnotations;

namespace MedTrackJordan.Models
{
    public enum AlertLevel
    {
        Days60 = 60,
        Days30 = 30,
        Days7 = 7
    }

    public enum AlertChannel
    {
        SMS,
        Email,
        InApp
    }

    public class ExpiryAlert
    {
        public int Id { get; set; }

        public int BatchId { get; set; }

        [Required]
        public AlertLevel AlertLevel { get; set; }

        public DateTime? SentAt { get; set; }

        public AlertChannel Channel { get; set; }

        public bool IsAcknowledged { get; set; } = false;

        // Navigation property
        public MedicationBatch MedicationBatch { get; set; } = null!;
    }
}