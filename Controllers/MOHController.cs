using Microsoft.AspNetCore.Mvc;
using MedTrack.Services;
using MedTrack.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace MedTrack.Controllers
{
    [Authorize(Roles = "MOH Admin,System Admin")]
    public class MOHController : Controller
    {
        private readonly IDashboardService _dashboardService;

        public MOHController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        public async Task<IActionResult> Dashboard()
        {
            var model = await _dashboardService.GetMOHDashboardAsync();
            return View(model);
        }

        public async Task<IActionResult> Shortages()
        {
            var model = await _dashboardService.GetMOHDashboardAsync();
            return View(model);
        }

        public async Task<IActionResult> Analytics()
        {
            var model = await _dashboardService.GetMOHDashboardAsync();
            return View(model);
        }

        public async Task<IActionResult> Pharmacies()
        {
            var model = await _dashboardService.GetMOHDashboardAsync();
            return View(model);
        }

        public async Task<IActionResult> Reports()
        {
            var model = await _dashboardService.GetMOHDashboardAsync();
            ViewBag.Governorates = _dashboardService.GetMohGovernorates();
            ViewBag.DefaultFrom = DateTime.Now.AddMonths(-1).ToString("yyyy-MM-dd");
            ViewBag.DefaultTo = DateTime.Now.ToString("yyyy-MM-dd");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateReport(
            string reportType,
            DateTime? from,
            DateTime? to,
            string? governorate)
        {
            var (content, filename) = await _dashboardService.GenerateMOHReportCsvAsync(
                reportType, from, to, governorate);
            return File(content, "text/csv", filename);
        }

        public async Task<IActionResult> Details(string governorate)
        {
            var gov = string.IsNullOrWhiteSpace(governorate) ? "Amman" : governorate;
            var dashboard = await _dashboardService.GetMOHDashboardAsync();
            var model = await _dashboardService.GetMOHGovernorateDetailsAsync(gov);
            ViewBag.Hotspot = dashboard.ShortageHotspots.FirstOrDefault(h => h.Governorate == gov);
            return View(model);
        }
    }
}
