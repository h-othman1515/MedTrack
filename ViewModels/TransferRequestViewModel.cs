using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MedTrack.Models.ViewModels
{
    public class TransferRequestViewModel
        {
            public int Id { get; set; }
            public int SurplusPostId { get; set; }
            public string DrugName { get; set; }
            public int Quantity { get; set; }
            public string FromPharmacy { get; set; }
            public string FromGovernorate { get; set; }
            public string ToPharmacy { get; set; }
            public string ToGovernorate { get; set; }
            public string Status { get; set; } // Requested, Approved, InTransit, Completed, Cancelled
            public DateTime RequestedAt { get; set; }
            public DateTime? CompletedAt { get; set; }
            public string Notes { get; set; }
        }
}
