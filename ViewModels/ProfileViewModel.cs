using System.ComponentModel.DataAnnotations;

namespace MedTrackJordan.ViewModels
{
    public class ProfileViewModel
    {
        [Required, MaxLength(200)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;
    }
}