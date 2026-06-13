using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MedTrack.Models.ViewModels
{
    public class TransferCreateViewModel
        {
            [Required]
            public int SurplusPostId { get; set; }

            [Required(ErrorMessage = "Quantity is required")]
            [Range(1, int.MaxValue)]
            [Display(Name = "Quantity Requested")]
            public int Quantity { get; set; }

            [Display(Name = "Notes")]
            [StringLength(500)]
            public string Notes { get; set; }
        }
}
