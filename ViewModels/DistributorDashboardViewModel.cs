using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MedTrack.Models.ViewModels
{
    public class DistributorDashboardViewModel
        {
            public string DistributorName { get; set; }
            public int PendingRestockRequests { get; set; }
            public int ConfirmedDeliveries { get; set; }
            public List<RestockRequestViewModel> RecentRequests { get; set; } = new();
            public List<RestockRequestViewModel> AllRestockRequests { get; set; } = new();
            public List<RestockRequestViewModel> ActiveDeliveries { get; set; } = new();
            public List<RegionalDemandViewModel> RegionalDemand { get; set; } = new();
            public List<DrugDemandViewModel> TopRequestedDrugs { get; set; } = new();
            public List<string> DemandChartLabels { get; set; } = new();
            public List<int> DemandChartValues { get; set; } = new();
        }
}
