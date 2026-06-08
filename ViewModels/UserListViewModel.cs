using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MedTrack.Models.ViewModels
{
    public class UserListViewModel
        {
            public string Id { get; set; }
            public string FullName { get; set; }
            public string Email { get; set; }
            public string Role { get; set; }
            public string PharmacyName { get; set; }
            public string Governorate { get; set; }
            public string Status { get; set; }
            public DateTime? LastLogin { get; set; }
            public DateTime CreatedAt { get; set; }
        }
}
