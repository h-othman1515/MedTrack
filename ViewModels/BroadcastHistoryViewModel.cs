using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MedTrack.Models.ViewModels
{
    public class BroadcastHistoryViewModel
        {
            public string Subject { get; set; }
            public string Type { get; set; }
            public string Message { get; set; }
            public DateTime SentAt { get; set; }
            public int RecipientsCount { get; set; }
        }
}
