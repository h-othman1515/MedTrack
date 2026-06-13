using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MedTrack.Models.ViewModels
{
    public class InventoryCreateViewModel
        {
            [Required(ErrorMessage = "Drug name is required")]
            [Display(Name = "Drug Name / Brand")]
            public string DrugName { get; set; }

            [Required(ErrorMessage = "Generic name is required")]
            [Display(Name = "Generic Name")]
            public string GenericName { get; set; }

            [Required(ErrorMessage = "Category is required")]
            [Display(Name = "Category")]
            public string Category { get; set; }

            [Required(ErrorMessage = "Batch number is required")]
            [Display(Name = "Batch Number")]
            public string BatchNo { get; set; }

            [Required(ErrorMessage = "Quantity is required")]
            [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
            [Display(Name = "Quantity")]
            public int Quantity { get; set; }

            [Required(ErrorMessage = "Unit is required")]
            [Display(Name = "Unit")]
            public string Unit { get; set; } // tablets, bottles, boxes, etc.

            [Required(ErrorMessage = "Minimum stock level is required")]
            [Range(0, int.MaxValue, ErrorMessage = "Minimum stock must be 0 or more")]
            [Display(Name = "Minimum Stock Level")]
            public int MinStockLevel { get; set; }

            [Required(ErrorMessage = "Expiry date is required")]
            [DataType(DataType.Date)]
            [Display(Name = "Expiry Date")]
            public DateTime ExpiryDate { get; set; }

            [Display(Name = "Unit Price (JOD)")]
            [Range(0, double.MaxValue, ErrorMessage = "Price must be positive")]
            public decimal UnitPrice { get; set; }
        }
}
