using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using MedTrackJordan.Data;
using MedTrackJordan.Models;

namespace MedTrackJordan.Controllers
{
    [Authorize]
    public class MedicationBatchController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MedicationBatchController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var batches = await _context.MedicationBatches
                .Include(b => b.Drug)
                .Include(b => b.Pharmacy)
                .OrderBy(b => b.ExpiryDate)
                .ToListAsync();

            return View(batches);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Drugs = await _context.Drugs.OrderBy(d => d.GenericName).ToListAsync();
            ViewBag.Pharmacies = await _context.Pharmacies.OrderBy(p => p.Name).ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MedicationBatch batch)
        {
            if (ModelState.IsValid)
            {
                batch.AddedAt = DateTime.UtcNow;
                _context.Add(batch);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Batch added to inventory.";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Drugs = await _context.Drugs.OrderBy(d => d.GenericName).ToListAsync();
            ViewBag.Pharmacies = await _context.Pharmacies.OrderBy(p => p.Name).ToListAsync();
            return View(batch);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var batch = await _context.MedicationBatches.FindAsync(id);
            if (batch == null) return NotFound();
            ViewBag.Drugs = await _context.Drugs.OrderBy(d => d.GenericName).ToListAsync();
            ViewBag.Pharmacies = await _context.Pharmacies.OrderBy(p => p.Name).ToListAsync();
            return View(batch);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MedicationBatch batch)
        {
            if (id != batch.Id) return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(batch);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Batch updated.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BatchExists(batch.Id)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Drugs = await _context.Drugs.OrderBy(d => d.GenericName).ToListAsync();
            ViewBag.Pharmacies = await _context.Pharmacies.OrderBy(p => p.Name).ToListAsync();
            return View(batch);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var batch = await _context.MedicationBatches
                .Include(b => b.Drug)
                .Include(b => b.Pharmacy)
                .FirstOrDefaultAsync(b => b.Id == id);
            if (batch == null) return NotFound();
            return View(batch);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var batch = await _context.MedicationBatches.FindAsync(id);
            if (batch != null)
            {
                _context.MedicationBatches.Remove(batch);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Batch removed from inventory.";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool BatchExists(int id) => _context.MedicationBatches.Any(e => e.Id == id);
    }
}