using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MedTrack.Models.ViewModels
{
    public class PharmacyListViewModel
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Email { get; set; }
            public string LicenseNo { get; set; }
            public string Governorate { get; set; }
            public string ManagerName { get; set; }
            public string Tier { get; set; }
            public int InventoryCount { get; set; }
            public string Status { get; set; }
            public DateTime CreatedAt { get; set; }
        }
}
