using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MedTrack.Models.ViewModels
{
    public class AlertViewModel
        {
            public int Id { get; set; }
            public int? BatchId { get; set; }
            public int? DrugId { get; set; }
            public string Type { get; set; } // Expiry, Shortage, Restock, Transfer
            public string Level { get; set; } // 60, 30, 7 days for expiry; Low, Medium, High for shortages
            public string DrugName { get; set; }
            public string BatchNo { get; set; }
            public string Message { get; set; }
            public DateTime SentAt { get; set; }
            public string Channel { get; set; } // SMS, Email, InApp
            public bool IsAcknowledged { get; set; }
            public string SuggestedAction { get; set; }
        }
}
