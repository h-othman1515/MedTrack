using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MedTrack.Models.ViewModels
{
    public class AlertsIndexViewModel
        {
            public List<AlertViewModel> ExpiryAlerts { get; set; } = new();
            public List<AlertViewModel> ShortageAlerts { get; set; } = new();
            public List<AlertViewModel> RestockAlerts { get; set; } = new();
            public List<AlertViewModel> TransferAlerts { get; set; } = new();
            public int TotalUnread { get; set; }
        }
}
