namespace MedTrack.Models.ViewModels
{
    public class NavbarNotificationItemViewModel
    {
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
        public string DotColor { get; set; } = "var(--mt-teal)";
    }

    public class NavbarNotificationsViewModel
    {
        public List<NavbarNotificationItemViewModel> Items { get; set; } = new();
        public int UnreadCount { get; set; }
        public string ViewAllUrl { get; set; } = "/Alerts";
    }
}
