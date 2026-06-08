using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MedTrack.Models.ViewModels
{
    public class DrugListViewModel
        {
            public int Id { get; set; }
            public string GenericName { get; set; }
            public string ScientificName { get; set; }
            public string BrandNames { get; set; }
            public string Category { get; set; }
            public int DefaultMinStock { get; set; }
            public string DefaultUnit { get; set; }
            public bool RequiresPrescription { get; set; }
            public bool IsCritical { get; set; }
            public bool IsActive { get; set; }
        }
}
