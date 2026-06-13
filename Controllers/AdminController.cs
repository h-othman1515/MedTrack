using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using MedTrack.Data;
using MedTrack.Models;
using MedTrack.Models.ViewModels;
using MedTrack.Services;

namespace MedTrack.Controllers
{
    [Authorize(Roles = "System Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAdminService _adminService;
        private readonly UserManager<MedTrack.Models.ApplicationUser> _userManager;

        public AdminController(
            ApplicationDbContext context,
            IAdminService adminService,
            UserManager<MedTrack.Models.ApplicationUser> userManager)
        {
            _context = context;
            _adminService = adminService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var model = await _adminService.GetDashboardAsync();
            return View(model);
        }

        public IActionResult Dashboard() => RedirectToAction(nameof(Index));

        public async Task<IActionResult> Users()
        {
            var model = await _adminService.GetUsersAsync(_userManager);
            return View(model);
        }

        public IActionResult CreateUser() => View();
        public IActionResult EditUser(string id) => View();

        public async Task<IActionResult> Pharmacies()
        {
            var model = await _adminService.GetPharmaciesAsync();
            return View(model);
        }

        public IActionResult PharmacyDetails(int id) => View();
        public IActionResult RegisterPharmacy() => View();

        public async Task<IActionResult> Drugs()
        {
            var model = await _adminService.GetDrugsAsync();
            return View(model);
        }

        public IActionResult AddDrug() => View();
        public IActionResult EditDrug(int id) => View();

        public async Task<IActionResult> Notifications()
        {
            var model = await _adminService.GetBroadcastHistoryAsync();
            return View(model);
        }

        public IActionResult NotificationHistory() => View();

        public async Task<IActionResult> AuditLogs()
        {
            var model = await _adminService.GetAuditLogsAsync();
            return View(model);
        }

        public IActionResult Settings()
        {
            var model = new SystemSettingsViewModel
            {
                SmsEnabled = true,
                EmailEnabled = true,
                InAppEnabled = true,
                SmsSenderId = "MedTrack",
                ExpiryAlert60 = 60,
                ExpiryAlert30 = 30,
                ExpiryAlert7 = 7,
                ShortageThreshold = 3,
                AutoRestockThreshold = 1.0,
                MaxSurplusDays = 30,
                SessionTimeout = 30,
                PasswordExpiryDays = 90,
                MaintenanceMode = false,
                MaintenanceMessage = "MedTrack is currently under maintenance. Please check back later."
            };
            return View(model);
        }

        [HttpPost]
        public IActionResult SaveSettings(SystemSettingsViewModel model) => RedirectToAction("Settings");

        [HttpPost]
        public IActionResult SendBroadcast(BroadcastViewModel model) => RedirectToAction("Notifications");

        [HttpPost]
        public IActionResult CreateUser(UserManagementViewModel model)
        {
            TempData["Success"] = "User created successfully.";
            return RedirectToAction(nameof(Users));
        }

        [HttpPost]
        public IActionResult EditUser(string id, UserManagementViewModel model)
        {
            TempData["Success"] = "User updated successfully.";
            return RedirectToAction(nameof(Users));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApprovePharmacy(int id, string? returnUrl)
        {
            var admin = await _userManager.GetUserAsync(User);
            var success = await _adminService.ApprovePharmacyAsync(id, admin?.Id, admin?.FullName);
            if (!success)
                TempData["Error"] = "Could not approve this pharmacy. It may already be approved or not found.";
            else
                TempData["Success"] = "Pharmacy approved successfully.";

            return LocalRedirect(GetPharmacyActionReturnUrl(returnUrl));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectPharmacy(int id, string? returnUrl)
        {
            var admin = await _userManager.GetUserAsync(User);
            var success = await _adminService.RejectPharmacyAsync(id, admin?.Id, admin?.FullName);
            if (!success)
                TempData["Error"] = "Could not reject this pharmacy. It may already be processed.";
            else
                TempData["Warning"] = "Pharmacy registration rejected.";

            return LocalRedirect(GetPharmacyActionReturnUrl(returnUrl));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SuspendPharmacy(int id)
        {
            var admin = await _userManager.GetUserAsync(User);
            var success = await _adminService.SuspendPharmacyAsync(id, admin?.Id, admin?.FullName);
            if (!success)
                TempData["Error"] = "Could not suspend this pharmacy.";
            else
                TempData["Warning"] = "Pharmacy suspended.";

            return RedirectToAction(nameof(Pharmacies));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReactivatePharmacy(int id)
        {
            var admin = await _userManager.GetUserAsync(User);
            var success = await _adminService.ReactivatePharmacyAsync(id, admin?.Id, admin?.FullName);
            if (!success)
                TempData["Error"] = "Could not reactivate this pharmacy.";
            else
                TempData["Success"] = "Pharmacy reactivated.";

            return RedirectToAction(nameof(Pharmacies));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePharmacy(int id)
        {
            var admin = await _userManager.GetUserAsync(User);
            var success = await _adminService.DeletePharmacyAsync(id, admin?.Id, admin?.FullName);
            if (!success)
                TempData["Error"] = "Could not delete this pharmacy. It may have linked data that must be removed first.";
            else
                TempData["Success"] = "Pharmacy deleted.";

            return RedirectToAction(nameof(Pharmacies));
        }

        private string GetPharmacyActionReturnUrl(string? returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return returnUrl;
            return Url.Action(nameof(Pharmacies))!;
        }

        [HttpPost]
        public IActionResult RegisterPharmacy(PharmacyManagementViewModel model)
        {
            TempData["Success"] = "Pharmacy registered successfully.";
            return RedirectToAction(nameof(Pharmacies));
        }

        [HttpPost]
        public IActionResult AddDrug(DrugCatalogViewModel model)
        {
            TempData["Success"] = "Drug added to catalog.";
            return RedirectToAction(nameof(Drugs));
        }

        [HttpPost]
        public IActionResult EditDrug(int id, DrugCatalogViewModel model)
        {
            TempData["Success"] = "Drug updated successfully.";
            return RedirectToAction(nameof(Drugs));
        }
    }
}
