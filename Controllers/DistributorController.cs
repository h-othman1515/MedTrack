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
            var pharmacyId = GetCurrentPharmacyId();
            var model = await _dashboardService.GetDistributorDashboardAsync(pharmacyId);
            return View(model);
        }

        public async Task<IActionResult> RestockRequests()
        {
            var pharmacyId = GetCurrentPharmacyId();
            var model = await _dashboardService.GetDistributorDashboardAsync(pharmacyId);
            return View(model);
        }

        public async Task<IActionResult> Deliveries()
        {
            var pharmacyId = GetCurrentPharmacyId();
            var model = await _dashboardService.GetDistributorDashboardAsync(pharmacyId);
            return View(model);
        }

        public async Task<IActionResult> Analytics()
        {
            var pharmacyId = GetCurrentPharmacyId();
            var model = await _dashboardService.GetDistributorDashboardAsync(pharmacyId);
            return View(model);
        }

        private int? GetCurrentPharmacyId()
        {
            var claim = User.FindFirstValue("PharmacyId");
            return int.TryParse(claim, out var id) ? id : null;
        }
    }
}
