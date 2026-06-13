namespace MedTrack.Models.ViewModels
{
    public class ChartTrendViewModel
    {
        public List<string> Labels { get; set; } = new();
        public List<decimal> WasteValues { get; set; } = new();
        public List<decimal> SavingsValues { get; set; } = new();
    }
}
