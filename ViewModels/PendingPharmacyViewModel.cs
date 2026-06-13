using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MedTrack.Models.ViewModels
{
    public class PendingPharmacyViewModel
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string ManagerName { get; set; }
            public string LicenseNo { get; set; }
            public string Governorate { get; set; }
            public DateTime RegisteredAt { get; set; }
        }
}
