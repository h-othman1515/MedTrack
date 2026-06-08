using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MedTrack.Models.ViewModels
{
    public class ApplicationUser
        {
            public string Id { get; set; }
            public string FullName { get; set; }
            public string Email { get; set; }
            public int? PharmacyId { get; set; }
            public string Role { get; set; }
        }
}
