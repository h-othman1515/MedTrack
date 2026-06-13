using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MedTrack.Models.ViewModels
{
    public class WasteItemViewModel
        {
            public string DrugName { get; set; } = string.Empty;
            public string BatchNo { get; set; } = string.Empty;
            public int Quantity { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal TotalValue { get; set; }
            public DateTime ExpiryDate { get; set; }
            public string DisposalMethod { get; set; } = string.Empty;
        }
}
