using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using MedTrack.Data;
using MedTrack.Models;

namespace MedTrack.Controllers
{
    [Authorize(Roles = "System Admin,Pharmacy Manager,MOH Admin")]
    public class PharmacyController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PharmacyController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Pharmacies.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var pharmacy = await _context.Pharmacies
                .Include(p => p.Staff)
                .Include(p => p.MedicationBatches)
                .ThenInclude(b => b.Drug)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (pharmacy == null) return NotFound();
            return View(pharmacy);
        }

        [Authorize(Roles = "System Admin,MOH Admin")]
        public IActionResult Create() => View();

        [Authorize(Roles = "System Admin,MOH Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Pharmacy pharmacy)
        {
            if (ModelState.IsValid)
            {
                _context.Add(pharmacy);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Pharmacy created successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(pharmacy);
        }

        [Authorize(Roles = "System Admin,MOH Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var pharmacy = await _context.Pharmacies.FindAsync(id);
            if (pharmacy == null) return NotFound();
            return View(pharmacy);
        }

        [Authorize(Roles = "System Admin,MOH Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Pharmacy pharmacy)
        {
            if (id != pharmacy.Id) return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(pharmacy);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Pharmacy updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PharmacyExists(pharmacy.Id)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(pharmacy);
        }

        [Authorize(Roles = "System Admin,MOH Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var pharmacy = await _context.Pharmacies.FirstOrDefaultAsync(p => p.Id == id);
            if (pharmacy == null) return NotFound();
            return View(pharmacy);
        }

        [Authorize(Roles = "System Admin,MOH Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var pharmacy = await _context.Pharmacies.FindAsync(id);
            if (pharmacy != null)
            {
                _context.Pharmacies.Remove(pharmacy);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Pharmacy deleted successfully.";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool PharmacyExists(int id) => _context.Pharmacies.Any(e => e.Id == id);
    }
}