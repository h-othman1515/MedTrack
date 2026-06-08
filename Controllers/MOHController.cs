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
            return View(model);
        }

        public async Task<IActionResult> Details(string governorate)
        {
            var model = await _dashboardService.GetMOHDashboardAsync();
            ViewBag.Governorate = string.IsNullOrWhiteSpace(governorate) ? "Amman" : governorate;
            return View(model);
        }
    }
}
