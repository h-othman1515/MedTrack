using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace MedTrackJordan.Models
{
    public class ApplicationUser : IdentityUser
    {
        [MaxLength(200)]
        public string? FullName { get; set; }

        public int? PharmacyId { get; set; }

        // Navigation property
        public Pharmacy? Pharmacy { get; set; }
    }
}