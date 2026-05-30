using System.ComponentModel.DataAnnotations;

namespace MedTrackJordan.Models
{
    public enum NotificationType
    {
        ExpiryAlert,
        TransferUpdate,
        RestockAlert
    }

    public class Notification
    {
        public int Id { get; set; }

        public int PharmacyId { get; set; }

        public NotificationType Type { get; set; }

        public AlertChannel Channel { get; set; }

        [MaxLength(1000)]
        public string? Message { get; set; }

        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public Pharmacy Pharmacy { get; set; } = null!;
    }
}