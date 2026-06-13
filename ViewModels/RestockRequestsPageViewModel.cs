namespace MedTrack.Models.ViewModels
{
    public class RestockRequestsPageViewModel
    {
        public int PendingCount { get; set; }
        public List<RestockRequestViewModel> Requests { get; set; } = new();
        public string? StatusFilter { get; set; }
        public string? GovernorateFilter { get; set; }
        public List<string> Governorates { get; set; } = new();
    }
}
