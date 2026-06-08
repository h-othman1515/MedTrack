using MedTrack.Data;
using MedTrack.Services;
using MedTrack.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MedTrack.Controllers
{
    public class SurplusController : Controller
    {
        private readonly ISurplusService _surplusService;
        private readonly ApplicationDbContext _context;

        public SurplusController(ISurplusService surplusService, ApplicationDbContext context)
        {
            _surplusService = surplusService;
            _context = context;
        }

        public async Task<IActionResult> Index(SurplusSearchViewModel search)
        {
            search.Results = await _surplusService.SearchAsync(search);
            search.AvailableGovernorates = new List<string> { "Amman", "Irbid", "Zarqa", "Balqa", "Madaba", "Mafraq", "Karak", "Aqaba" };
            search.AvailableCategories = new List<string> { "Antibiotics", "Cardiovascular", "Diabetes", "Pain Relief", "Vitamins" };
            return View(search);
        }

        public async Task<IActionResult> Create(int? drugId)
        {
            var model = new SurplusCreateViewModel
            {
                ExpiryDate = DateTime.UtcNow.AddMonths(6)
            };

            if (drugId.HasValue)
                model.DrugId = drugId.Value;

            await PopulateDrugOptionsAsync();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SurplusCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDrugOptionsAsync();
                return View(model);
            }

            var pharmacyId = GetCurrentPharmacyId();
            if (!pharmacyId.HasValue) return RedirectToAction("Login", "Account");

            await _surplusService.CreateAsync(pharmacyId.Value, model);
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateDrugOptionsAsync()
        {
            var pharmacyId = GetCurrentPharmacyId();
            var drugsQuery = _context.MedicationBatches
                .AsNoTracking()
                .Include(b => b.Drug)
                .Where(b => b.Quantity > 0);

            if (pharmacyId.HasValue)
                drugsQuery = drugsQuery.Where(b => b.PharmacyId == pharmacyId.Value);

            var batches = await drugsQuery
                .OrderBy(b => b.Drug!.GenericName)
                .ToListAsync();

            ViewBag.DrugOptions = batches
                .GroupBy(b => b.DrugId)
                .Select(g =>
                {
                    var batch = g.First();
                    return new SelectListItem
                    {
                        Value = batch.DrugId.ToString(),
                        Text = $"{batch.Drug?.GenericName} (Batch: {batch.BatchNo}) - {g.Sum(x => x.Quantity)} {batch.Unit}"
                    };
                })
                .ToList();

            if (!((List<SelectListItem>)ViewBag.DrugOptions).Any())
            {
                ViewBag.DrugOptions = await _context.Drugs
                    .AsNoTracking()
                    .OrderBy(d => d.GenericName)
                    .Select(d => new SelectListItem(d.GenericName, d.Id.ToString()))
                    .ToListAsync();
            }
        }

        private int? GetCurrentPharmacyId()
        {
            var claim = User.FindFirstValue("PharmacyId");
            return int.TryParse(claim, out var id) ? id : null;
        }
    }
}
