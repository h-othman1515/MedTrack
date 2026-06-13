using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using MedTrack.Data;
using MedTrack.Models;

namespace MedTrack.Controllers
{
    [Authorize]
    public class SurplusPostController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SurplusPostController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var posts = await _context.SurplusPosts
                .Include(s => s.Drug)
                .Include(s => s.Pharmacy)
                .Include(s => s.TransferRequests)
                .Where(s => s.Status == SurplusStatus.Available || s.Status == SurplusStatus.Reserved)
                .OrderByDescending(s => s.PostedAt)
                .ToListAsync();

            return View(posts);
        }

        public async Task<IActionResult> MyPosts()
        {
            var posts = await _context.SurplusPosts
                .Include(s => s.Drug)
                .Include(s => s.TransferRequests)
                .ThenInclude(t => t.RequestingPharmacy)
                .OrderByDescending(s => s.PostedAt)
                .ToListAsync();

            return View(posts);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Drugs = await _context.Drugs.OrderBy(d => d.GenericName).ToListAsync();
            ViewBag.Pharmacies = await _context.Pharmacies.OrderBy(p => p.Name).ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SurplusPost post)
        {
            if (ModelState.IsValid)
            {
                post.PostedAt = DateTime.UtcNow;
                post.Status = SurplusStatus.Available;
                _context.Add(post);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Surplus posted successfully.";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Drugs = await _context.Drugs.OrderBy(d => d.GenericName).ToListAsync();
            ViewBag.Pharmacies = await _context.Pharmacies.OrderBy(p => p.Name).ToListAsync();
            return View(post);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var post = await _context.SurplusPosts
                .Include(s => s.Drug)
                .Include(s => s.Pharmacy)
                .FirstOrDefaultAsync(s => s.Id == id);
            if (post == null) return NotFound();
            return View(post);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var post = await _context.SurplusPosts.FindAsync(id);
            if (post != null)
            {
                _context.SurplusPosts.Remove(post);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Post removed.";
            }
            return RedirectToAction(nameof(MyPosts));
        }
    }
}