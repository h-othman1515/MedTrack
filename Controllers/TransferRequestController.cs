using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using MedTrack.Data;
using MedTrack.Models;

namespace MedTrack.Controllers
{
    [Authorize]
    public class TransferRequestController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TransferRequestController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var requests = await _context.TransferRequests
                .Include(t => t.SurplusPost)
                .ThenInclude(s => s.Drug)
                .Include(t => t.SurplusPost)
                .ThenInclude(s => s.Pharmacy)
                .Include(t => t.RequestingPharmacy)
                .OrderByDescending(t => t.RequestedAt)
                .ToListAsync();

            return View(requests);
        }

        public async Task<IActionResult> Create(int surplusPostId)
        {
            var post = await _context.SurplusPosts
                .Include(s => s.Drug)
                .Include(s => s.Pharmacy)
                .FirstOrDefaultAsync(s => s.Id == surplusPostId);

            if (post == null || post.Status != SurplusStatus.Available)
            {
                TempData["Error"] = "This surplus is no longer available.";
                return RedirectToAction("Index", "SurplusPost");
            }

            ViewBag.SurplusPost = post;
            ViewBag.Pharmacies = await _context.Pharmacies.OrderBy(p => p.Name).ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TransferRequest request)
        {
            var post = await _context.SurplusPosts.FindAsync(request.SurplusPostId);
            if (post == null || post.Status != SurplusStatus.Available)
            {
                TempData["Error"] = "This surplus is no longer available.";
                return RedirectToAction("Index", "SurplusPost");
            }

            if (ModelState.IsValid)
            {
                request.RequestedAt = DateTime.UtcNow;
                request.Status = TransferStatus.Pending;
                post.Status = SurplusStatus.Reserved;
                _context.Add(request);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Transfer request sent.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.SurplusPost = await _context.SurplusPosts
                .Include(s => s.Drug)
                .FirstOrDefaultAsync(s => s.Id == request.SurplusPostId);
            ViewBag.Pharmacies = await _context.Pharmacies.OrderBy(p => p.Name).ToListAsync();
            return View(request);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(int id)
        {
            var request = await _context.TransferRequests.FindAsync(id);
            if (request == null) return NotFound();

            request.Status = TransferStatus.Confirmed;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Transfer confirmed.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(int id)
        {
            var request = await _context.TransferRequests
                .Include(t => t.SurplusPost)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (request == null) return NotFound();

            request.Status = TransferStatus.Completed;
            request.CompletedAt = DateTime.UtcNow;
            request.SurplusPost.Status = SurplusStatus.Completed;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Transfer completed.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var request = await _context.TransferRequests
                .Include(t => t.SurplusPost)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (request == null) return NotFound();

            request.Status = TransferStatus.Cancelled;
            request.SurplusPost.Status = SurplusStatus.Available;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Transfer cancelled.";
            return RedirectToAction(nameof(Index));
        }
    }
}