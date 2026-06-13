namespace MedTrack.Models.ViewModels
{
    public class MOHPharmacyOverviewViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Governorate { get; set; } = string.Empty;
        public int BatchCount { get; set; }
        public int ActiveShortages { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
