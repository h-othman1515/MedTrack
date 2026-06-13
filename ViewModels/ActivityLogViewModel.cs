using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MedTrack.Models.ViewModels
{
    public class ActivityLogViewModel
        {
            public string Type { get; set; }
            public string Action { get; set; }
            public string Details { get; set; }
            public string PerformedBy { get; set; }
            public DateTime Timestamp { get; set; }
        }
}
