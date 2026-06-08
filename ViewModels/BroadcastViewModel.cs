using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MedTrack.Models.ViewModels
{
    public class BroadcastViewModel
        {
            public List<BroadcastHistoryViewModel> RecentBroadcasts { get; set; } = new();
        }
}
