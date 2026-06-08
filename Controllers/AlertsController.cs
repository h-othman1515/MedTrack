using Microsoft.AspNetCore.Mvc;
using MedTrack.Services;
using MedTrack.Models.ViewModels;
using System.Security.Claims;

namespace MedTrack.Controllers
{
    public class AlertsController : Controller
    {
        private readonly IAlertService _alertService;

        public AlertsController(IAlertService alertService)
        {
            _alertService = alertService;
        }

        public async Task<IActionResult> Index()
        {
            var pharmacyId = GetCurrentPharmacyId();
            if (!pharmacyId.HasValue) return RedirectToAction("Login", "Account");

            var alerts = await _alertService.GetPharmacyAlertsAsync(pharmacyId.Value);
            var model = new AlertsIndexViewModel
            {
                ExpiryAlerts = alerts.Where(a => a.Type == "Expiry").ToList(),
                ShortageAlerts = alerts.Where(a => a.Type == "Shortage").ToList(),
                RestockAlerts = alerts.Where(a => a.Type == "Restock").ToList(),
                TransferAlerts = alerts.Where(a => a.Type == "Transfer").ToList(),
                TotalUnread = alerts.Count(a => !a.IsAcknowledged)
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Acknowledge(int id)
        {
            var pharmacyId = GetCurrentPharmacyId();
            if (!pharmacyId.HasValue) return RedirectToAction("Login", "Account");

            await _alertService.AcknowledgeAlertAsync(id, pharmacyId.Value);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllRead()
        {
            var pharmacyId = GetCurrentPharmacyId();
            if (!pharmacyId.HasValue) return RedirectToAction("Login", "Account");

            await _alertService.MarkAllReadAsync(pharmacyId.Value);
            TempData["Success"] = "All alerts marked as read.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ClearAcknowledged()
        {
            var pharmacyId = GetCurrentPharmacyId();
            if (!pharmacyId.HasValue) return RedirectToAction("Login", "Account");

            await _alertService.ClearAcknowledgedAsync(pharmacyId.Value);
            TempData["Success"] = "Acknowledged alerts cleared.";
            return RedirectToAction(nameof(Index));
        }

        private int? GetCurrentPharmacyId()
        {
            var claim = User.FindFirstValue("PharmacyId");
            return int.TryParse(claim, out var id) ? id : null;
        }
    }
}
