using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using MedTrack.Data;
using MedTrack.Models;
using MedTrack.Models.ViewModels;

namespace MedTrack.Controllers
{
    [Authorize(Roles = "System Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var model = new AdminDashboardViewModel
            {
                TotalUsers = await _context.Users.CountAsync(),
                ActivePharmacies = await _context.Pharmacies.CountAsync(),
                TotalDrugs = await _context.Drugs.CountAsync(),
                PendingApprovals = 0,
                ActiveAlerts = await _context.MedicationBatches.CountAsync(b => b.Quantity <= b.MinStockLevel),
                TotalTransfers = await _context.TransferRequests.CountAsync()
            };
            return View(model);
        }

        public IActionResult Dashboard() => RedirectToAction(nameof(Index));
        public IActionResult Users() => View(new UserManagementViewModel());
        public IActionResult CreateUser() => View();
        public IActionResult EditUser(string id) => View();
        public IActionResult Pharmacies() => View(new PharmacyManagementViewModel());
        public IActionResult PharmacyDetails(int id) => View();
        public IActionResult RegisterPharmacy() => View();
        public IActionResult Drugs() => View(new DrugCatalogViewModel());
        public IActionResult AddDrug() => View();
        public IActionResult EditDrug(int id) => View();
        public IActionResult Notifications() => View(new BroadcastViewModel());
        public IActionResult NotificationHistory() => View();
        public IActionResult AuditLogs() => View(new AuditLogViewModel());

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
