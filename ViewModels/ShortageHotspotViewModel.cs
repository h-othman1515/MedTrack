using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MedTrack.Models.ViewModels
{
    public class ShortageHotspotViewModel
        {
            public string Governorate { get; set; }
            public int ActiveShortages { get; set; }
            public int PharmaciesAffected { get; set; }
            public string Severity { get; set; } // Low, Medium, High, Critical
            public double Latitude { get; set; }
            public double Longitude { get; set; }
        }
}
