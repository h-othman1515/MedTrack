using MedTrack.Data;
using MedTrack.Models;
using MedTrack.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace MedTrack.Services
{
    public interface IInventoryService
    {
        Task<List<InventoryItemViewModel>> GetPharmacyInventoryAsync(int pharmacyId);
        Task<InventoryItemViewModel?> GetBatchAsync(int id);
        Task<int> CreateBatchAsync(int pharmacyId, InventoryCreateViewModel model);
        Task UpdateBatchAsync(int id, InventoryCreateViewModel model);
        Task DeleteBatchAsync(int id);
        Task<List<InventoryItemViewModel>> GetExpiringItemsAsync(int pharmacyId, int days);
        Task<List<InventoryItemViewModel>> GetCriticalStockAsync(int pharmacyId);
    }

    public class InventoryService : IInventoryService
    {
        private readonly ApplicationDbContext _context;

        public InventoryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<InventoryItemViewModel>> GetPharmacyInventoryAsync(int pharmacyId)
        {
            var batches = await _context.MedicationBatches
                .AsNoTracking()
                .Where(b => b.PharmacyId == pharmacyId)
                .Include(b => b.Drug)
                .OrderByDescending(b => b.AddedAt)
                .ToListAsync();

            return batches.Select(b => MapToViewModel(b)).ToList();
        }

        public async Task<InventoryItemViewModel?> GetBatchAsync(int id)
        {
            var batch = await _context.MedicationBatches
                .AsNoTracking()
                .Include(b => b.Drug)
                .FirstOrDefaultAsync(b => b.Id == id);

            return batch == null ? null : MapToViewModel(batch);
        }

        public async Task<int> CreateBatchAsync(int pharmacyId, InventoryCreateViewModel model)
        {
            // Find or create drug
            var drug = await _context.Drugs
                .FirstOrDefaultAsync(d => d.GenericName == model.GenericName);

            if (drug == null)
            {
                drug = new Drug
                {
                    GenericName = model.GenericName,
                    Category = model.Category,
                    DefaultMinStockLevel = model.MinStockLevel,
                    DefaultUnit = model.Unit
                };
                _context.Drugs.Add(drug);
                await _context.SaveChangesAsync();
            }

            var batch = new MedicationBatch
            {
                PharmacyId = pharmacyId,
                DrugId = drug.Id,
                Quantity = model.Quantity,
                BatchNo = model.BatchNo,
                ExpiryDate = model.ExpiryDate,
                MinStockLevel = model.MinStockLevel,
                Unit = model.Unit,
                UnitPrice = model.UnitPrice
            };

            _context.MedicationBatches.Add(batch);
            await _context.SaveChangesAsync();
            await TryCreateRestockRequestAsync(pharmacyId, drug.Id, batch.Quantity, batch.MinStockLevel);
            return batch.Id;
        }

        public async Task UpdateBatchAsync(int id, InventoryCreateViewModel model)
        {
            var batch = await _context.MedicationBatches.FindAsync(id)
                ?? throw new Exception("Batch not found");

            batch.Quantity = model.Quantity;
            batch.BatchNo = model.BatchNo;
            batch.ExpiryDate = model.ExpiryDate;
            batch.MinStockLevel = model.MinStockLevel;
            batch.Unit = model.Unit;
            batch.UnitPrice = model.UnitPrice;

            await _context.SaveChangesAsync();
            await TryCreateRestockRequestAsync(batch.PharmacyId, batch.DrugId, batch.Quantity, batch.MinStockLevel);
        }

        private async Task TryCreateRestockRequestAsync(int pharmacyId, int drugId, int quantity, int minStockLevel)
        {
            if (quantity > minStockLevel) return;

            var hasPending = await _context.RestockRequests.AnyAsync(r =>
                r.PharmacyId == pharmacyId &&
                r.DrugId == drugId &&
                r.Status == RestockStatus.Pending);

            if (hasPending) return;

            var requestedQty = Math.Max(minStockLevel * 2 - quantity, minStockLevel);

            _context.RestockRequests.Add(new RestockRequest
            {
                PharmacyId = pharmacyId,
                DrugId = drugId,
                RequestedQuantity = requestedQty,
                Status = RestockStatus.Pending,
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
        }

        public async Task DeleteBatchAsync(int id)
        {
            var batch = await _context.MedicationBatches.FindAsync(id);
            if (batch != null)
            {
                _context.MedicationBatches.Remove(batch);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<InventoryItemViewModel>> GetExpiringItemsAsync(int pharmacyId, int days)
        {
            var cutoff = DateTime.Now.AddDays(days);
            var batches = await _context.MedicationBatches
                .AsNoTracking()
                .Where(b => b.PharmacyId == pharmacyId && b.ExpiryDate <= cutoff && b.ExpiryDate > DateTime.Now)
                .Include(b => b.Drug)
                .ToListAsync();

            return batches.Select(b => MapToViewModel(b)).ToList();
        }

        public async Task<List<InventoryItemViewModel>> GetCriticalStockAsync(int pharmacyId)
        {
            var batches = await _context.MedicationBatches
                .AsNoTracking()
                .Where(b => b.PharmacyId == pharmacyId && b.Quantity <= b.MinStockLevel)
                .Include(b => b.Drug)
                .ToListAsync();

            return batches.Select(b => MapToViewModel(b)).ToList();
        }

        private static InventoryItemViewModel MapToViewModel(MedicationBatch b)
        {
            var daysUntil = (b.ExpiryDate - DateTime.Now).Days;
            var status = daysUntil <= 0 ? "Expired" :
                         daysUntil <= 7 ? "Danger" :
                         daysUntil <= 30 ? "Warning" : "Healthy";

            return new InventoryItemViewModel
            {
                Id = b.Id,
                DrugId = b.DrugId,
                DrugName = b.Drug?.GenericName ?? "Unknown",
                GenericName = b.Drug?.GenericName ?? "Unknown",
                Category = b.Drug?.Category ?? "Unknown",
                BatchNo = b.BatchNo ?? "N/A",
                Quantity = b.Quantity,
                MinStockLevel = b.MinStockLevel,
                ExpiryDate = b.ExpiryDate,
                AddedAt = b.AddedAt,
                Status = status,
                DaysUntilExpiry = daysUntil,
                Unit = b.Unit ?? "units",
                UnitPrice = b.UnitPrice ?? 0
            };
        }
    }
}
