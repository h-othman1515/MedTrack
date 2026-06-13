using Microsoft.AspNetCore.Identity;

namespace MedTrack.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public int? PharmacyId { get; set; }
        public Pharmacy? Pharmacy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
    }
}
