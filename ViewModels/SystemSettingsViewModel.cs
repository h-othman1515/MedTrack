using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MedTrack.Models.ViewModels
{
    public class SystemSettingsViewModel
        {
            public bool SmsEnabled { get; set; }
            public bool EmailEnabled { get; set; }
            public bool InAppEnabled { get; set; }
            public string SmsSenderId { get; set; }
            public int ExpiryAlert60 { get; set; }
            public int ExpiryAlert30 { get; set; }
            public int ExpiryAlert7 { get; set; }
            public int ShortageThreshold { get; set; }
            public double AutoRestockThreshold { get; set; }
            public int MaxSurplusDays { get; set; }
            public int SessionTimeout { get; set; }
            public int PasswordExpiryDays { get; set; }
            public bool MaintenanceMode { get; set; }
            public string MaintenanceMessage { get; set; }
        }
}
