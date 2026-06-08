using System.Globalization;
using System.Security.Claims;
using CsvHelper;
using CsvHelper.Configuration;
using MedTrack.Models.ViewModels;
using MedTrack.Services;
using Microsoft.AspNetCore.Mvc;

namespace MedTrack.Controllers
{
    public class InventoryController : Controller
    {
        private readonly IInventoryService _inventoryService;

        public InventoryController(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        public async Task<IActionResult> Index()
        {
            var pharmacyId = GetCurrentPharmacyId();
            if (!pharmacyId.HasValue) return RedirectToAction("Login", "Account");

            var model = await _inventoryService.GetPharmacyInventoryAsync(pharmacyId.Value);
            return View(model);
        }

        public IActionResult Create() => View(new InventoryCreateViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InventoryCreateViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var pharmacyId = GetCurrentPharmacyId();
            if (!pharmacyId.HasValue) return RedirectToAction("Login", "Account");

            await _inventoryService.CreateBatchAsync(pharmacyId.Value, model);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var batch = await _inventoryService.GetBatchAsync(id);
            if (batch == null) return NotFound();

            var model = new InventoryCreateViewModel
            {
                DrugName = batch.DrugName,
                GenericName = batch.GenericName,
                Category = batch.Category,
                BatchNo = batch.BatchNo,
                Quantity = batch.Quantity,
                Unit = batch.Unit,
                MinStockLevel = batch.MinStockLevel,
                ExpiryDate = batch.ExpiryDate,
                UnitPrice = batch.UnitPrice
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, InventoryCreateViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            await _inventoryService.UpdateBatchAsync(id, model);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _inventoryService.DeleteBatchAsync(id);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Import() => View(new CsvImportViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Import(IFormFile csvFile)
        {
            if (csvFile == null || csvFile.Length == 0)
            {
                ModelState.AddModelError(string.Empty, "Please select a CSV file.");
                return View(new CsvImportViewModel());
            }

            var pharmacyId = GetCurrentPharmacyId();
            if (!pharmacyId.HasValue) return RedirectToAction("Login", "Account");

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null,
                HeaderValidated = null
            };

            using var reader = new StreamReader(csvFile.OpenReadStream());
            using var csv = new CsvReader(reader, config);
            var rows = csv.GetRecords<InventoryCsvRow>().ToList();
            var imported = 0;

            foreach (var row in rows)
            {
                if (string.IsNullOrWhiteSpace(row.GenericName)) continue;

                await _inventoryService.CreateBatchAsync(pharmacyId.Value, new InventoryCreateViewModel
                {
                    DrugName = row.DrugName ?? row.GenericName,
                    GenericName = row.GenericName,
                    Category = row.Category ?? "Other",
                    BatchNo = row.BatchNo ?? $"IMP-{DateTime.UtcNow:yyyyMMdd}-{imported + 1}",
                    Quantity = row.Quantity > 0 ? row.Quantity : 1,
                    Unit = row.Unit ?? "Tablets",
                    MinStockLevel = row.MinStockLevel,
                    UnitPrice = row.UnitPrice,
                    ExpiryDate = row.ExpiryDate == default ? DateTime.UtcNow.AddMonths(12) : row.ExpiryDate
                });
                imported++;
            }

            TempData["Success"] = imported > 0
                ? $"Successfully imported {imported} inventory item(s)."
                : "No valid rows found in the CSV file.";
            return RedirectToAction(nameof(Index));
        }

        private int? GetCurrentPharmacyId()
        {
            var claim = User.FindFirstValue("PharmacyId");
            return int.TryParse(claim, out var id) ? id : null;
        }

        private sealed class InventoryCsvRow
        {
            public string? DrugName { get; set; }
            public string GenericName { get; set; } = string.Empty;
            public string? Category { get; set; }
            public string? BatchNo { get; set; }
            public int Quantity { get; set; }
            public string? Unit { get; set; }
            public int MinStockLevel { get; set; }
            public decimal UnitPrice { get; set; }
            public DateTime ExpiryDate { get; set; }
        }
    }
}
