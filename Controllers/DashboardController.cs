using Microsoft.AspNetCore.Mvc;
using MedTrack.Services;
using MedTrack.Models.ViewModels;
using System.Security.Claims;

namespace MedTrack.Controllers
{
    [Route("[controller]")]
    public class DashboardController : Controller
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        public async Task<IActionResult> Index()
        {
            var pharmacyId = GetCurrentPharmacyId();
            if (!pharmacyId.HasValue)
                return RedirectToAction("Login", "Account");

            var model = await _dashboardService.GetPharmacyDashboardAsync(pharmacyId.Value);
            return View(model);
        }

        private int? GetCurrentPharmacyId()
        {
            var claim = User.FindFirstValue("PharmacyId");
            return int.TryParse(claim, out var id) ? id : null;
        }
    }
}
