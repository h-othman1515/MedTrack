using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using MedTrack.Models;
using MedTrack.Data;

namespace MedTrack.Controllers
{
    [Authorize(Roles = "System Admin,Pharmacy Manager,MOH Admin")]
    public class DrugController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DrugController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Drugs.OrderBy(d => d.GenericName).ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var drug = await _context.Drugs
                .Include(d => d.MedicationBatches)
                .ThenInclude(b => b.Pharmacy)
                .FirstOrDefaultAsync(d => d.Id == id);
            if (drug == null) return NotFound();
            return View(drug);
        }

        [Authorize(Roles = "System Admin,MOH Admin")]
        public IActionResult Create() => View();

        [Authorize(Roles = "System Admin,MOH Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Drug drug)
        {
            if (ModelState.IsValid)
            {
                _context.Add(drug);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Drug added to catalog.";
                return RedirectToAction(nameof(Index));
            }
            return View(drug);
        }

        [Authorize(Roles = "System Admin,MOH Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var drug = await _context.Drugs.FindAsync(id);
            if (drug == null) return NotFound();
            return View(drug);
        }

        [Authorize(Roles = "System Admin,MOH Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Drug drug)
        {
            if (id != drug.Id) return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(drug);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Drug updated.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DrugExists(drug.Id)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(drug);
        }

        [Authorize(Roles = "System Admin,MOH Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var drug = await _context.Drugs.FirstOrDefaultAsync(d => d.Id == id);
            if (drug == null) return NotFound();
            return View(drug);
        }

        [Authorize(Roles = "System Admin,MOH Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var drug = await _context.Drugs.FindAsync(id);
            if (drug != null)
            {
                _context.Drugs.Remove(drug);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Drug removed from catalog.";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool DrugExists(int id) => _context.Drugs.Any(e => e.Id == id);
    }
}