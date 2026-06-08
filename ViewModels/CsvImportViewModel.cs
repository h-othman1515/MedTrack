using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MedTrack.Models.ViewModels
{
    public class CsvImportViewModel
        {
            [Required(ErrorMessage = "Please select a CSV file")]
            [Display(Name = "CSV File")]
            public string FileName { get; set; }
            public bool HasHeaders { get; set; } = true;
        }
}
