using MedTrack.Data;
using MedTrack.Models;
using MedTrack.Models.ViewModels;
using MedTrack.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MedTrack.ViewComponents
{
    public class NavbarNotificationsViewComponent : ViewComponent
    {
        private readonly IAlertService _alertService;
        private readonly IDashboardService _dashboardService;
        private readonly ApplicationDbContext _context;

        public NavbarNotificationsViewComponent(
            IAlertService alertService,
            IDashboardService dashboardService,
            ApplicationDbContext context)
        {
            _alertService = alertService;
            _dashboardService = dashboardService;
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(bool badgeOnly = false)
        {
            var model = await BuildModelAsync();
            return badgeOnly ? View("Badge", model) : View(model);
        }

        private async Task<NavbarNotificationsViewModel> BuildModelAsync()
        {
            var user = HttpContext.User;
            if (user.Identity?.IsAuthenticated != true)
                return new NavbarNotificationsViewModel();

            if (user.IsInRole("Distributor") && !HasPharmacyId(user))
                return await BuildDistributorModelAsync();

            var pharmacyId = GetPharmacyId(user);
            if (pharmacyId.HasValue)
                return await BuildPharmacyModelAsync(pharmacyId.Value);

            return new NavbarNotificationsViewModel();
        }

        private async Task<NavbarNotificationsViewModel> BuildPharmacyModelAsync(int pharmacyId)
        {
            var alerts = await _alertService.GetUnreadAlertsAsync(pharmacyId);

            return new NavbarNotificationsViewModel
            {
                UnreadCount = alerts.Count,
                ViewAllUrl = "/Alerts",
                Items = alerts
                    .Take(5)
                    .Select(a => new NavbarNotificationItemViewModel
                    {
                        Title = GetAlertTitle(a.Type),
                        Body = a.Message,
                        SentAt = a.SentAt,
                        DotColor = GetAlertDotColor(a.Type)
                    })
                    .ToList()
            };
        }

        private async Task<NavbarNotificationsViewModel> BuildDistributorModelAsync()
        {
            var pendingCount = await _dashboardService.GetPendingRestockCountAsync();
            var pending = await _context.RestockRequests
                .AsNoTracking()
                .Include(r => r.Drug)
                .Include(r => r.Pharmacy)
                .Where(r => r.Status == RestockStatus.Pending)
                .OrderByDescending(r => r.CreatedAt)
                .Take(5)
                .ToListAsync();

            return new NavbarNotificationsViewModel
            {
                UnreadCount = pendingCount,
                ViewAllUrl = "/Distributor/RestockRequests",
                Items = pending.Select(r => new NavbarNotificationItemViewModel
                {
                    Title = "Restock Request",
                    Body = $"{r.Pharmacy?.Name}: {r.Drug?.GenericName} — {r.RequestedQuantity} units needed",
                    SentAt = r.CreatedAt,
                    DotColor = "var(--mt-amber)"
                }).ToList()
            };
        }

        private static int? GetPharmacyId(ClaimsPrincipal user)
        {
            var claim = user.FindFirstValue("PharmacyId");
            return int.TryParse(claim, out var id) ? id : null;
        }

        private static bool HasPharmacyId(ClaimsPrincipal user) => GetPharmacyId(user).HasValue;

        private static string GetAlertTitle(string type) => type switch
        {
            "Expiry" => "Expiry Alert",
            "Shortage" => "Low Stock Warning",
            "Restock" => "Restock Request",
            "Transfer" => "Transfer Update",
            _ => "Alert"
        };

        private static string GetAlertDotColor(string type) => type switch
        {
            "Expiry" => "var(--mt-red)",
            "Shortage" => "var(--mt-amber)",
            "Restock" => "var(--mt-amber)",
            "Transfer" => "var(--mt-teal)",
            _ => "var(--mt-teal)"
        };
    }
}
