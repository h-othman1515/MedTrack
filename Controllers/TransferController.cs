using Microsoft.AspNetCore.Mvc;
using MedTrack.Services;
using MedTrack.Models;
using MedTrack.Models.ViewModels;
using System.Security.Claims;

namespace MedTrack.Controllers
{
    public class TransferController : Controller
    {
        private readonly ITransferService _transferService;

        public TransferController(ITransferService transferService)
        {
            _transferService = transferService;
        }

        public async Task<IActionResult> Index()
        {
            var pharmacyId = GetCurrentPharmacyId();
            if (!pharmacyId.HasValue) return RedirectToAction("Login", "Account");

            var model = await _transferService.GetPharmacyTransfersAsync(pharmacyId.Value);
            return View(model);
        }

        public async Task<IActionResult> Details(int id)
        {
            var pharmacyId = GetCurrentPharmacyId();
            if (!pharmacyId.HasValue) return RedirectToAction("Login", "Account");

            var transfer = await _transferService.GetTransferAsync(id, pharmacyId.Value);
            if (transfer == null) return NotFound();

            return View(transfer);
        }

        public IActionResult Create(int surplusId) => View(new TransferCreateViewModel { SurplusPostId = surplusId });

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TransferCreateViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var pharmacyId = GetCurrentPharmacyId();
            if (!pharmacyId.HasValue) return RedirectToAction("Login", "Account");

            await _transferService.CreateAsync(pharmacyId.Value, model);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            if (Enum.TryParse<TransferStatus>(status, out var transferStatus))
                await _transferService.UpdateStatusAsync(id, transferStatus);

            return RedirectToAction(nameof(Index));
        }

        private int? GetCurrentPharmacyId()
        {
            var claim = User.FindFirstValue("PharmacyId");
            return int.TryParse(claim, out var id) ? id : null;
        }
    }
}