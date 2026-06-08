using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MedTrack.Models.ViewModels
{
    public class PharmacyManagementViewModel
        {
            public List<PharmacyListViewModel> Pharmacies { get; set; } = new();
            public int ActiveCount { get; set; }
            public int PendingCount { get; set; }
            public int SuspendedCount { get; set; }
            public int TotalCount { get; set; }
        }
}
