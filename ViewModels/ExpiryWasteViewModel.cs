using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MedTrack.Models.ViewModels
{
    public class ExpiryWasteViewModel
        {
            public string Governorate { get; set; }
            public decimal WasteValue { get; set; }
            public int ItemsWasted { get; set; }
            public decimal TrendVsLastMonth { get; set; } // percentage
        }
}
