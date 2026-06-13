namespace MedTrack.Models
{
    public enum SurplusStatus
    {
        Available,
        Reserved,    // added - used when a transfer request is made
        Completed,
        Expired
    }

    public enum TransferStatus
    {
        Pending,     // renamed from Requested
        Confirmed,   // renamed from Approved
        InTransit,
        Completed,
        Cancelled
    }

    public enum RestockStatus
    {
        Pending,
        Confirmed,
        Delivered
    }

    public enum AlertLevel
    {
        _60 = 60,
        _30 = 30,
        _7 = 7
    }

    public enum PharmacyTier
    {
        Basic,
        Professional,
        Enterprise
    }

    public enum NotificationType
    {
        Expiry,
        Shortage,
        Restock,
        Transfer,
        System
    }

    public enum NotificationChannel
    {
        SMS,
        Email,
        InApp
    }
}