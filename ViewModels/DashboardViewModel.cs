using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MedTrack.Models.ViewModels
{
    public class DashboardViewModel
        {
            public int TotalItems { get; set; }
            public int HealthyStock { get; set; }
            public int ExpiringSoon { get; set; } // 30-60 days
            public int CriticalStock { get; set; } // < 30 days or below min
            public int ExpiredItems { get; set; }
            public decimal TotalInventoryValue { get; set; }
            public List<InventoryItemViewModel> RecentItems { get; set; } = new();
            public List<AlertViewModel> RecentAlerts { get; set; } = new();
            public List<SurplusListingViewModel> RecentSurplus { get; set; } = new();
        }
}
