using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MedTrack.Models.ViewModels
{
    public class RegionalDemandViewModel
        {
            public string Governorate { get; set; }
            public string DrugName { get; set; }
            public int PharmaciesBelowMin { get; set; }
            public int TotalQuantityNeeded { get; set; }
            public int CurrentRegionalStock { get; set; }
            public string Urgency { get; set; } // Low, Medium, High
        }
}
