using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MedTrack.Models.ViewModels
{
    public class InventoryItemViewModel
        {
            public int Id { get; set; }
            public int DrugId { get; set; }
            public string DrugName { get; set; }
            public string GenericName { get; set; }
            public string Category { get; set; }
            public string BatchNo { get; set; }
            public int Quantity { get; set; }
            public int MinStockLevel { get; set; }
            public DateTime ExpiryDate { get; set; }
            public DateTime AddedAt { get; set; }
            public string Status { get; set; } // Healthy, Warning, Danger, Expired
            public int DaysUntilExpiry { get; set; }
            public string Unit { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal TotalValue => Quantity * UnitPrice;
        }
}
