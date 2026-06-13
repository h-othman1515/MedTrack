using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MedTrack.Models.ViewModels
{
    public class SurplusSearchViewModel
        {
            public string SearchTerm { get; set; }
            public string Governorate { get; set; }
            public string Category { get; set; }
            public int? MinDaysUntilExpiry { get; set; }
            public List<string> AvailableGovernorates { get; set; } = new();
            public List<string> AvailableCategories { get; set; } = new();
            public List<SurplusListingViewModel> Results { get; set; } = new();
        }
}
