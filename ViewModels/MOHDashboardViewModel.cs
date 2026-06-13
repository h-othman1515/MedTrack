using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MedTrack.Models.ViewModels
{
    public class MOHDashboardViewModel
        {
            public int TotalPharmacies { get; set; }
            public int ActiveShortages { get; set; }
            public int ResolvedThisMonth { get; set; }
            public decimal TotalWasteThisMonth { get; set; }
            public decimal TotalSavedViaTransfers { get; set; }
            public List<ShortageHotspotViewModel> ShortageHotspots { get; set; } = new();
            public List<TopShortageViewModel> TopShortages { get; set; } = new();
            public List<ExpiryWasteViewModel> ExpiryWasteByRegion { get; set; } = new();
            public List<TransferActivityViewModel> TransferActivity { get; set; } = new();
            public List<string> ShortageChartLabels { get; set; } = new();
            public List<int> ShortageChartActive { get; set; } = new();
            public List<int> ShortageChartResolved { get; set; } = new();
            public List<MOHPharmacyOverviewViewModel> RegisteredPharmacies { get; set; } = new();
            public List<MOHCategoryShortageViewModel> ShortagesByCategory { get; set; } = new();
            public List<string> ShortageTrendLabels { get; set; } = new();
            public List<int> ShortageTrendValues { get; set; } = new();
        }
}
