using MedTrack.Data;
using MedTrack.Models;
using MedTrack.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace MedTrack.Services
{
    public interface IDashboardService
    {
        Task<DashboardViewModel> GetPharmacyDashboardAsync(int pharmacyId);
        Task<MOHDashboardViewModel> GetMOHDashboardAsync();
        Task<DistributorDashboardViewModel> GetDistributorDashboardAsync(int? distributorPharmacyId);
    }

    public class DashboardService : IDashboardService
    {
        private readonly ApplicationDbContext _context;

        private static readonly Dictionary<string, (double Lat, double Lng)> GovernorateCoords = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Amman"] = (31.9454, 35.9284),
            ["Irbid"] = (32.5568, 35.8469),
            ["Zarqa"] = (32.0602, 36.0870),
            ["Balqa"] = (32.0349, 35.7259),
            ["Madaba"] = (31.7193, 35.7933),
            ["Mafraq"] = (32.3417, 36.2020),
            ["Karak"] = (31.1856, 35.7040),
            ["Aqaba"] = (29.5267, 35.0078),
            ["Jerash"] = (32.2722, 35.8993),
            ["Ajloun"] = (32.3326, 35.7517),
            ["Tafilah"] = (30.8375, 35.6044),
            ["Ma'an"] = (30.1927, 35.7360)
        };

        public DashboardService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardViewModel> GetPharmacyDashboardAsync(int pharmacyId)
        {
            var batches = await _context.MedicationBatches
                .AsNoTracking()
                .Where(b => b.PharmacyId == pharmacyId)
                .Include(b => b.Drug)
                .ToListAsync();

            var now = DateTime.Now;

            return new DashboardViewModel
            {
                TotalItems = batches.Count,
                HealthyStock = batches.Count(b => (b.ExpiryDate - now).Days > 30 && b.Quantity > b.MinStockLevel),
                ExpiringSoon = batches.Count(b => (b.ExpiryDate - now).Days is > 7 and <= 30),
                CriticalStock = batches.Count(b => (b.ExpiryDate - now).Days is > 0 and <= 7 || b.Quantity <= b.MinStockLevel),
                ExpiredItems = batches.Count(b => b.ExpiryDate <= now),
                TotalInventoryValue = batches.Sum(b => b.Quantity * (b.UnitPrice ?? 0)),
                RecentItems = batches.OrderByDescending(b => b.AddedAt).Take(5).Select(b => new InventoryItemViewModel
                {
                    Id = b.Id,
                    DrugName = b.Drug?.GenericName ?? "Unknown",
                    Quantity = b.Quantity,
                    ExpiryDate = b.ExpiryDate,
                    DaysUntilExpiry = (b.ExpiryDate - now).Days,
                    Status = (b.ExpiryDate - now).Days <= 0 ? "Expired"
                           : (b.ExpiryDate - now).Days <= 7 ? "Danger"
                           : (b.ExpiryDate - now).Days <= 30 ? "Warning"
                           : "Healthy"
                }).ToList()
            };
        }

        public async Task<MOHDashboardViewModel> GetMOHDashboardAsync()
        {
            var pharmacies = await _context.Pharmacies.AsNoTracking().ToListAsync();
            var batches = await _context.MedicationBatches
                .AsNoTracking()
                .Include(b => b.Drug)
                .Include(b => b.Pharmacy)
                .ToListAsync();

            var now = DateTime.Now;
            var monthStart = new DateTime(now.Year, now.Month, 1);
            var lastMonthStart = monthStart.AddMonths(-1);

            var govGroups = pharmacies.GroupBy(p => p.Governorate);
            var hotspots = new List<ShortageHotspotViewModel>();

            foreach (var gov in govGroups)
            {
                var govPharmacyIds = gov.Select(p => p.Id).ToHashSet();
                var govBatches = batches.Where(b => govPharmacyIds.Contains(b.PharmacyId)).ToList();
                var shortBatches = govBatches.Where(b => b.Quantity <= b.MinStockLevel).ToList();
                var shortCount = shortBatches.Count;
                var affected = shortBatches.Select(b => b.PharmacyId).Distinct().Count();
                var coords = GovernorateCoords.GetValueOrDefault(gov.Key, (Lat: 31.95, Lng: 35.93));

                hotspots.Add(new ShortageHotspotViewModel
                {
                    Governorate = gov.Key,
                    ActiveShortages = shortCount,
                    PharmaciesAffected = affected,
                    Severity = shortCount > 10 ? "Critical" : shortCount > 5 ? "High" : shortCount > 2 ? "Medium" : "Low",
                    Latitude = coords.Lat,
                    Longitude = coords.Lng
                });
            }

            var lowStockBatches = batches.Where(b => b.Quantity <= b.MinStockLevel).ToList();
            var topShortages = lowStockBatches
                .GroupBy(b => b.DrugId)
                .Select(g =>
                {
                    var drug = g.First().Drug;
                    var firstSeen = g.Min(b => b.AddedAt);
                    return new TopShortageViewModel
                    {
                        DrugName = drug?.GenericName ?? "Unknown",
                        GenericName = drug?.GenericName ?? "Unknown",
                        Category = drug?.Category ?? "Other",
                        PharmaciesShort = g.Select(b => b.PharmacyId).Distinct().Count(),
                        GovernoratesAffected = g.Select(b => b.Pharmacy?.Governorate ?? "").Where(x => x != "").Distinct().Count(),
                        DaysSinceFirstReported = Math.Max(0, (int)(now - firstSeen).TotalDays)
                    };
                })
                .OrderByDescending(t => t.PharmaciesShort)
                .Take(20)
                .ToList();

            var expiredThisMonth = batches.Where(b => b.ExpiryDate >= monthStart && b.ExpiryDate <= now).ToList();
            var expiredLastMonth = batches.Where(b => b.ExpiryDate >= lastMonthStart && b.ExpiryDate < monthStart).ToList();

            var expiryWasteByRegion = govGroups.Select(gov =>
            {
                var govPharmacyIds = gov.Select(p => p.Id).ToHashSet();
                var thisMonth = expiredThisMonth.Where(b => govPharmacyIds.Contains(b.PharmacyId)).ToList();
                var lastMonth = expiredLastMonth.Where(b => govPharmacyIds.Contains(b.PharmacyId)).ToList();
                var wasteValue = thisMonth.Sum(b => b.Quantity * (b.UnitPrice ?? 0));
                var lastValue = lastMonth.Sum(b => b.Quantity * (b.UnitPrice ?? 0));
                var trend = lastValue > 0 ? Math.Round((double)((wasteValue - lastValue) / lastValue * 100), 1) : 0;

                return new ExpiryWasteViewModel
                {
                    Governorate = gov.Key,
                    WasteValue = wasteValue,
                    ItemsWasted = thisMonth.Sum(b => b.Quantity),
                    TrendVsLastMonth = (decimal)trend
                };
            })
            .Where(w => w.ItemsWasted > 0 || w.WasteValue > 0)
            .OrderByDescending(w => w.WasteValue)
            .ToList();

            var completedTransfers = await _context.TransferRequests
                .AsNoTracking()
                .Include(t => t.SurplusPost)
                .Where(t => t.Status == TransferStatus.Completed && t.CompletedAt != null)
                .ToListAsync();

            var transferActivity = completedTransfers
                .GroupBy(t => new { t.CompletedAt!.Value.Year, t.CompletedAt!.Value.Month })
                .OrderByDescending(g => g.Key.Year).ThenByDescending(g => g.Key.Month)
                .Take(6)
                .Select(g => new TransferActivityViewModel
                {
                    Month = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"),
                    TransfersCompleted = g.Count(),
                    ValueTransferred = g.Sum(t => t.Quantity * (t.SurplusPost?.Price ?? 0)),
                    ItemsSaved = g.Sum(t => t.Quantity)
                })
                .OrderByDescending(t => t.Month)
                .ToList();

            var resolvedRestocks = await _context.RestockRequests
                .CountAsync(r => r.Status == RestockStatus.Delivered &&
                                 r.ConfirmedAt >= monthStart);

            var totalSaved = completedTransfers.Sum(t => t.Quantity * (t.SurplusPost?.Price ?? 0));

            return new MOHDashboardViewModel
            {
                TotalPharmacies = pharmacies.Count,
                ActiveShortages = lowStockBatches.Count,
                ResolvedThisMonth = resolvedRestocks,
                TotalWasteThisMonth = expiredThisMonth.Sum(b => b.Quantity * (b.UnitPrice ?? 0)),
                TotalSavedViaTransfers = totalSaved,
                ShortageHotspots = hotspots.OrderByDescending(h => h.ActiveShortages).ToList(),
                TopShortages = topShortages,
                ExpiryWasteByRegion = expiryWasteByRegion,
                TransferActivity = transferActivity
            };
        }

        public async Task<DistributorDashboardViewModel> GetDistributorDashboardAsync(int? distributorPharmacyId)
        {
            var pending = await _context.RestockRequests
                .AsNoTracking()
                .CountAsync(r => r.Status == RestockStatus.Pending);

            var confirmed = await _context.RestockRequests
                .AsNoTracking()
                .CountAsync(r => r.Status == RestockStatus.Delivered);

            var recentRequests = await _context.RestockRequests
                .AsNoTracking()
                .Include(r => r.Drug)
                .Include(r => r.Pharmacy)
                .OrderByDescending(r => r.CreatedAt)
                .Take(15)
                .ToListAsync();

            var pharmacyIds = recentRequests.Select(r => r.PharmacyId).Distinct().ToList();
            var drugIds = recentRequests.Select(r => r.DrugId).Distinct().ToList();
            var stockLookup = await _context.MedicationBatches
                .AsNoTracking()
                .Where(b => pharmacyIds.Contains(b.PharmacyId) && drugIds.Contains(b.DrugId))
                .ToListAsync();

            var lowStock = await _context.MedicationBatches
                .AsNoTracking()
                .Where(b => b.Quantity <= b.MinStockLevel)
                .Include(b => b.Drug)
                .Include(b => b.Pharmacy)
                .ToListAsync();

            var regionalDemand = lowStock
                .GroupBy(b => new { b.Pharmacy!.Governorate, b.DrugId })
                .Select(g =>
                {
                    var first = g.First();
                    var needed = g.Sum(b => Math.Max(b.MinStockLevel - b.Quantity, 0));
                    var pharmaciesBelow = g.Select(b => b.PharmacyId).Distinct().Count();
                    return new RegionalDemandViewModel
                    {
                        Governorate = g.Key.Governorate,
                        DrugName = first.Drug?.GenericName ?? "Unknown",
                        PharmaciesBelowMin = pharmaciesBelow,
                        TotalQuantityNeeded = needed,
                        CurrentRegionalStock = g.Sum(b => b.Quantity),
                        Urgency = pharmaciesBelow >= 5 ? "High" : pharmaciesBelow >= 3 ? "Medium" : "Low"
                    };
                })
                .OrderByDescending(r => r.PharmaciesBelowMin)
                .Take(20)
                .ToList();

            return new DistributorDashboardViewModel
            {
                DistributorName = "Jordan Pharma Distribution",
                PendingRestockRequests = pending,
                ConfirmedDeliveries = confirmed,
                RecentRequests = recentRequests.Select(r =>
                {
                    var batch = stockLookup
                        .Where(b => b.PharmacyId == r.PharmacyId && b.DrugId == r.DrugId)
                        .OrderByDescending(b => b.AddedAt)
                        .FirstOrDefault();

                    return new RestockRequestViewModel
                    {
                        Id = r.Id,
                        PharmacyName = r.Pharmacy?.Name ?? "Unknown",
                        Governorate = r.Pharmacy?.Governorate ?? "Unknown",
                        DrugName = r.Drug?.GenericName ?? "Unknown",
                        RequestedQuantity = r.RequestedQuantity,
                        CurrentStock = batch?.Quantity ?? 0,
                        MinStockLevel = batch?.MinStockLevel ?? 0,
                        Status = r.Status.ToString(),
                        CreatedAt = r.CreatedAt,
                        ConfirmedAt = r.ConfirmedAt
                    };
                }).ToList(),
                RegionalDemand = regionalDemand
            };
        }
    }
}
