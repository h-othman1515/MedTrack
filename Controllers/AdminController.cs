using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using MedTrackJordan.Data;

namespace MedTrackJordan.Controllers
{
    [Authorize(Roles = "Admin,MOHAdmin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Dashboard()
        {
            var totalPharmacies = await _context.Pharmacies.CountAsync();
            var totalDrugs = await _context.Drugs.CountAsync();
            var totalBatches = await _context.MedicationBatches.CountAsync();
            var totalSurplus = await _context.SurplusPosts.CountAsync();
            var pendingTransfers = await _context.TransferRequests
                .CountAsync(t => t.Status == Models.TransferStatus.Pending);
            var lowStock = await _context.MedicationBatches
                .Include(b => b.Drug)
                .Where(b => b.Quantity < b.Drug.MinStockLevel)
                .CountAsync();

            var expiringSoon = await _context.MedicationBatches
                .Include(b => b.Drug)
                .Include(b => b.Pharmacy)
                .Where(b => b.ExpiryDate <= DateTime.UtcNow.AddDays(30))
                .OrderBy(b => b.ExpiryDate)
                .Take(10)
                .ToListAsync();

            ViewBag.TotalPharmacies = totalPharmacies;
            ViewBag.TotalDrugs = totalDrugs;
            ViewBag.TotalBatches = totalBatches;
            ViewBag.TotalSurplus = totalSurplus;
            ViewBag.PendingTransfers = pendingTransfers;
            ViewBag.LowStock = lowStock;
            ViewBag.ExpiringSoon = expiringSoon;

            return View();
        }
    }
}