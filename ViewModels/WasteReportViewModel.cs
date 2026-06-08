using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MedTrack.Models.ViewModels
{
    public class WasteReportViewModel
        {
            public DateTime PeriodStart { get; set; }
            public DateTime PeriodEnd { get; set; }
            public decimal TotalWasteValue { get; set; }
            public int TotalItemsWasted { get; set; }
            public List<WasteItemViewModel> Items { get; set; } = new();
        }
}
