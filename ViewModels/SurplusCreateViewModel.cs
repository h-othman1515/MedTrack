using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MedTrack.Models.ViewModels
{
    public class SurplusCreateViewModel
        {
            [Required(ErrorMessage = "Drug is required")]
            [Display(Name = "Drug")]
            public int DrugId { get; set; }

            [Required(ErrorMessage = "Quantity is required")]
            [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
            [Display(Name = "Quantity Available")]
            public int Quantity { get; set; }

            [Required(ErrorMessage = "Expiry date is required")]
            [DataType(DataType.Date)]
            [Display(Name = "Expiry Date")]
            public DateTime ExpiryDate { get; set; }

            [Display(Name = "Condition Notes")]
            [StringLength(500)]
            public string Condition { get; set; }

            [Display(Name = "Price (JOD) - Leave empty for free")]
            [Range(0, double.MaxValue)]
            public decimal? Price { get; set; }
        }
}
