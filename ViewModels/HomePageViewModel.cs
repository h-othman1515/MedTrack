using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MedTrack.Models.ViewModels
{
    public class HomePageViewModel
        {
            public int RegisteredPharmacies { get; set; }
            public int MedicationsTracked { get; set; }
            public decimal WastePrevented { get; set; }
            public int ShortagesResolved { get; set; }
        }
}
