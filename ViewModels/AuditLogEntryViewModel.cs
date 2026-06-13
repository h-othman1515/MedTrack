using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MedTrack.Models.ViewModels
{
    public class AuditLogEntryViewModel
        {
            public DateTime Timestamp { get; set; }
            public string UserName { get; set; }
            public string UserRole { get; set; }
            public string Action { get; set; }
            public string EntityType { get; set; }
            public string Details { get; set; }
            public string IpAddress { get; set; }
            public string Severity { get; set; }
        }
}
