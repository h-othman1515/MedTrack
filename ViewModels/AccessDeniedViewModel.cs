using System.Security.Claims;

namespace MedTrack.Models.ViewModels
{
    public class AccessDeniedViewModel
    {
        public bool IsAuthenticated { get; set; }
        public string? RoleLabel { get; set; }
        public string? RoleBadgeClass { get; set; }
        public string? DashboardUrl { get; set; }
        public string? DashboardLabel { get; set; }
        public string? UserEmail { get; set; }
        public string? RequestedPath { get; set; }
    }
}
