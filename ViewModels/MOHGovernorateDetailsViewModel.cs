namespace MedTrack.Models.ViewModels
{
    public class MOHGovernorateDetailsViewModel
    {
        public string Governorate { get; set; } = string.Empty;
        public List<TopShortageViewModel> Shortages { get; set; } = new();
        public List<TransferRequestViewModel> RecentTransfers { get; set; } = new();
    }
}
