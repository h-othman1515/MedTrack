using Microsoft.AspNetCore.Mvc;
using MedTrack.Services;
using MedTrack.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace MedTrack.Controllers
{
    [Authorize(Roles = "Distributor,System Admin")]
    public class DistributorController : Controller
    {
        private readonly IDashboardService _dashboardService;

        public DistributorController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        public async Task<IActionResult> Index()
        {
            await SetPendingBadgeAsync();
            var pharmacyId = GetCurrentPharmacyId();
            var model = await _dashboardService.GetDistributorDashboardAsync(pharmacyId);
            return View(model);
        }

        public async Task<IActionResult> RestockRequests(string? status, string? governorate)
        {
            await SetPendingBadgeAsync();
            var model = await _dashboardService.GetRestockRequestsPageAsync(status, governorate);
            return View(model);
        }

        public async Task<IActionResult> Deliveries()
        {
            await SetPendingBadgeAsync();
            var pharmacyId = GetCurrentPharmacyId();
            var model = await _dashboardService.GetDistributorDashboardAsync(pharmacyId);
            return View(model);
        }

        public async Task<IActionResult> Analytics()
        {
            await SetPendingBadgeAsync();
            var pharmacyId = GetCurrentPharmacyId();
            var model = await _dashboardService.GetDistributorDashboardAsync(pharmacyId);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmRequest(int id)
        {
            var success = await _dashboardService.ConfirmRestockRequestAsync(id);
            if (!success)
                TempData["Error"] = "Could not confirm this request. It may already be processed.";
            else
                TempData["Success"] = "Restock request confirmed. Shipment scheduled.";

            return RedirectToAction(nameof(RestockRequests));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkDelivered(int id)
        {
            var success = await _dashboardService.MarkRestockDeliveredAsync(id);
            if (!success)
                TempData["Error"] = "Could not mark delivery complete. Confirm the request first.";
            else
                TempData["Success"] = "Delivery marked complete and pharmacy stock updated.";

            return RedirectToAction(nameof(Deliveries));
        }

        private async Task SetPendingBadgeAsync()
        {
            ViewBag.PendingRestockCount = await _dashboardService.GetPendingRestockCountAsync();
        }

        private int? GetCurrentPharmacyId()
        {
            var claim = User.FindFirstValue("PharmacyId");
            return int.TryParse(claim, out var id) ? id : null;
        }
    }
}
