using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MedTrack.Models.ViewModels
{
    public class MOHReportViewModel
        {
            public DateTime ReportDate { get; set; }
            public string ReportType { get; set; }
            public string Governorate { get; set; }
            public DateTime? DateFrom { get; set; }
            public DateTime? DateTo { get; set; }
            public List<MOHDashboardViewModel> ReportData { get; set; } = new();
        }
}
