using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MedTrack.Models.ViewModels
{
    public class SurplusListingViewModel
        {
            public int Id { get; set; }
            public string DrugName { get; set; }
            public string GenericName { get; set; }
            public string Category { get; set; }
            public int Quantity { get; set; }
            public string Unit { get; set; }
            public DateTime ExpiryDate { get; set; }
            public int DaysUntilExpiry { get; set; }
            public string PharmacyName { get; set; }
            public string Governorate { get; set; }
            public string Status { get; set; } // Available, Pending, Completed
            public DateTime PostedAt { get; set; }
            public string Condition { get; set; }
            public decimal? Price { get; set; } // Optional: free or priced
        }
}
