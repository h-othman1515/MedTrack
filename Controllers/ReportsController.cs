using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MedTrack.Models.ViewModels;
using MedTrack.Services;
using System.Security.Claims;

namespace MedTrack.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly IReportsService _reportsService;

        public ReportsController(IReportsService reportsService)
        {
            _reportsService = reportsService;
        }

        public async Task<IActionResult> Index(DateTime? from, DateTime? to)
        {
            var pharmacyId = GetCurrentPharmacyId();
            if (!pharmacyId.HasValue)
                return RedirectToAction("Login", "Account");

            var model = await _reportsService.GetWasteReportAsync(pharmacyId.Value, from, to);
            return View(model);
        }

        private int? GetCurrentPharmacyId()
        {
            var claim = User.FindFirstValue("PharmacyId");
            return int.TryParse(claim, out var id) ? id : null;
        }
    }
}
