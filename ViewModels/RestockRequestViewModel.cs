using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MedTrack.Models.ViewModels
{
    public class RestockRequestViewModel
        {
            public int Id { get; set; }
            public string PharmacyName { get; set; }
            public string Governorate { get; set; }
            public string DrugName { get; set; }
            public int RequestedQuantity { get; set; }
            public int CurrentStock { get; set; }
            public int MinStockLevel { get; set; }
            public string Status { get; set; } // Pending, Confirmed, Delivered
            public DateTime CreatedAt { get; set; }
            public DateTime? ConfirmedAt { get; set; }
        }
}
