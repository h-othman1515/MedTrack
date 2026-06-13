using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MedTrack.Models.ViewModels
{
    public class RegisterViewModel
        {
            [Required(ErrorMessage = "Full name is required")]
            [Display(Name = "Full Name")]
            public string FullName { get; set; }

            [Required(ErrorMessage = "Email is required")]
            [EmailAddress(ErrorMessage = "Invalid email address")]
            [Display(Name = "Email Address")]
            public string Email { get; set; }

            [Required(ErrorMessage = "Password is required")]
            [StringLength(100, ErrorMessage = "Password must be at least {2} characters long.", MinimumLength = 8)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm Password")]
            [Compare("Password", ErrorMessage = "Passwords do not match.")]
            public string ConfirmPassword { get; set; }

            [Required(ErrorMessage = "Role is required")]
            [Display(Name = "Account Type")]
            public string Role { get; set; } // Pharmacy Staff, Pharmacy Manager, Distributor, MOH Admin

            // Pharmacy-specific fields
            [Display(Name = "Pharmacy Name")]
            public string PharmacyName { get; set; }

            [Display(Name = "License Number")]
            public string LicenseNo { get; set; }

            [Display(Name = "Governorate")]
            public string Governorate { get; set; }

            [Display(Name = "Phone Number")]
            public string Phone { get; set; }

            [Display(Name = "Address")]
            public string Address { get; set; }
        }
}
