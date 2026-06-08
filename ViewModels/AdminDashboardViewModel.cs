using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MedTrack.Models.ViewModels
{
    public class AdminDashboardViewModel
        {
            public int TotalUsers { get; set; }
            public int ActivePharmacies { get; set; }
            public int PendingApprovals { get; set; }
            public int TotalDrugs { get; set; }
            public int ActiveAlerts { get; set; }
            public int TotalTransfers { get; set; }
            public List<PendingPharmacyViewModel> PendingPharmacies { get; set; } = new();
            public List<ActivityLogViewModel> RecentActivity { get; set; } = new();
            public int SmsSentToday { get; set; }
            public int EmailsSentToday { get; set; }
            public int InAppNotificationsToday { get; set; }
            public int FailedDeliveries { get; set; }
            public decimal NotificationSuccessRate { get; set; }
            public decimal TotalInventoryValue { get; set; }
            public decimal WastePreventedMonth { get; set; }
            public int ActiveSubscriptions { get; set; }
            public decimal MonthlyRevenue { get; set; }
            public decimal AvgPharmacyUptime { get; set; }
        }
}
