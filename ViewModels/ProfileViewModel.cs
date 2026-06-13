using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MedTrack.Models.ViewModels
{
    public class ProfileViewModel
        {
            public string FullName { get; set; }
            public string Email { get; set; }
            public string Phone { get; set; }
            public string PharmacyName { get; set; }
            public string LicenseNo { get; set; }
            public string Governorate { get; set; }
            public string Address { get; set; }
            public string Role { get; set; }
        }
}
