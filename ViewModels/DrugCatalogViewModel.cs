using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MedTrack.Models.ViewModels
{
    public class DrugCatalogViewModel
        {
            public List<DrugListViewModel> Drugs { get; set; } = new();
            public List<string> Categories { get; set; } = new();
            public int TotalDrugs { get; set; }
        }
}
