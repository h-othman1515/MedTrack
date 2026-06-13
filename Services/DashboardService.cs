using System.Text;
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
        Task<MOHGovernorateDetailsViewModel> GetMOHGovernorateDetailsAsync(string governorate);
        Task<int> GetPendingRestockCountAsync();
        Task<RestockRequestsPageViewModel> GetRestockRequestsPageAsync(string? status, string? governorate);
        Task<bool> ConfirmRestockRequestAsync(int id);
        Task<bool> MarkRestockDeliveredAsync(int id);
        Task<(byte[] Content, string Filename)> GenerateMOHReportCsvAsync(
            string reportType, DateTime? from, DateTime? to, string? governorate);
        IReadOnlyList<string> GetMohGovernorates();
    }

    public class DashboardService : IDashboardService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAlertService _alertService;

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

        public DashboardService(ApplicationDbContext context, IAlertService alertService)
        {
            _context = context;
            _alertService = alertService;
        }

        public async Task<DashboardViewModel> GetPharmacyDashboardAsync(int pharmacyId)
        {
            var batches = await _context.MedicationBatches
                .AsNoTracking()
                .Where(b => b.PharmacyId == pharmacyId)
                .Include(b => b.Drug)
                .ToListAsync();

            var now = DateTime.Now;

            var recentAlerts = await _alertService.GetPharmacyAlertsAsync(pharmacyId);

            var completedTransfers = await _context.TransferRequests
                .AsNoTracking()
                .Include(t => t.SurplusPost)
                .Where(t => t.Status == TransferStatus.Completed &&
                            t.CompletedAt != null &&
                            (t.RequestingPharmacyId == pharmacyId || t.SurplusPost.PharmacyId == pharmacyId))
                .ToListAsync();

            var expiryTrend = BuildPharmacyExpiryTrend(batches, completedTransfers);

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
                    DrugId = b.DrugId,
                    DrugName = b.Drug?.GenericName ?? "Unknown",
                    GenericName = b.Drug?.GenericName ?? "Unknown",
                    Category = b.Drug?.Category ?? "Other",
                    BatchNo = b.BatchNo ?? "N/A",
                    Quantity = b.Quantity,
                    MinStockLevel = b.MinStockLevel,
                    Unit = b.Unit ?? "Units",
                    UnitPrice = b.UnitPrice ?? 0,
                    ExpiryDate = b.ExpiryDate,
                    AddedAt = b.AddedAt,
                    DaysUntilExpiry = (b.ExpiryDate - now).Days,
                    Status = (b.ExpiryDate - now).Days <= 0 ? "Expired"
                           : (b.ExpiryDate - now).Days <= 7 ? "Danger"
                           : (b.ExpiryDate - now).Days <= 30 ? "Warning"
                           : "Healthy"
                }).ToList(),
                RecentAlerts = recentAlerts.Take(5).ToList(),
                ExpiryTrend = expiryTrend
            };
        }

        private static ChartTrendViewModel BuildPharmacyExpiryTrend(
            List<MedicationBatch> batches,
            List<TransferRequest> completedTransfers)
        {
            var now = DateTime.Now;
            var labels = new List<string>();
            var wasteValues = new List<decimal>();
            var savingsValues = new List<decimal>();

            for (var i = 5; i >= 0; i--)
            {
                var monthStart = new DateTime(now.Year, now.Month, 1).AddMonths(-i);
                var monthEnd = monthStart.AddMonths(1);
                labels.Add(monthStart.ToString("MMM"));

                var waste = batches
                    .Where(b => b.ExpiryDate >= monthStart && b.ExpiryDate < monthEnd)
                    .Sum(b => b.Quantity * (b.UnitPrice ?? 0));
                wasteValues.Add(waste);

                var saved = completedTransfers
                    .Where(t => t.CompletedAt >= monthStart && t.CompletedAt < monthEnd)
                    .Sum(t => t.Quantity * (t.SurplusPost?.Price ?? 0));
                savingsValues.Add(saved);
            }

            return new ChartTrendViewModel
            {
                Labels = labels,
                WasteValues = wasteValues,
                SavingsValues = savingsValues
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

            var resolvedRestocksCount = await _context.RestockRequests
                .CountAsync(r => r.Status == RestockStatus.Delivered &&
                                 r.ConfirmedAt >= monthStart);

            var resolvedByGov = await _context.RestockRequests
                .AsNoTracking()
                .Include(r => r.Pharmacy)
                .Where(r => r.Status == RestockStatus.Delivered && r.ConfirmedAt >= monthStart)
                .GroupBy(r => r.Pharmacy!.Governorate)
                .Select(g => new { Governorate = g.Key, Count = g.Count() })
                .ToDictionaryAsync(g => g.Governorate, g => g.Count);

            var completedTransfers = await _context.TransferRequests
                .AsNoTracking()
                .Include(t => t.SurplusPost)
                .Where(t => t.Status == TransferStatus.Completed && t.CompletedAt != null)
                .ToListAsync();

            var totalSaved = completedTransfers.Sum(t => t.Quantity * (t.SurplusPost?.Price ?? 0));

            foreach (var gov in govGroups)
            {
                var govPharmacyIds = gov.Select(p => p.Id).ToHashSet();
                var govBatches = batches.Where(b => govPharmacyIds.Contains(b.PharmacyId)).ToList();
                var shortBatches = govBatches.Where(b => b.Quantity <= b.MinStockLevel).ToList();
                var shortCount = shortBatches.Count;
                var affected = shortBatches.Select(b => b.PharmacyId).Distinct().Count();
                var coords = GovernorateCoords.GetValueOrDefault(gov.Key, (Lat: 31.95, Lng: 35.93));
                var topDrugGroup = shortBatches
                    .GroupBy(b => b.DrugId)
                    .OrderByDescending(g => g.Count())
                    .FirstOrDefault();

                hotspots.Add(new ShortageHotspotViewModel
                {
                    Governorate = gov.Key,
                    ActiveShortages = shortCount,
                    PharmaciesAffected = affected,
                    Severity = shortCount > 10 ? "Critical" : shortCount > 5 ? "High" : shortCount > 2 ? "Medium" : "Low",
                    Latitude = coords.Lat,
                    Longitude = coords.Lng,
                    TopShortDrug = topDrugGroup?.First().Drug?.GenericName ?? "—",
                    ResolvedThisMonth = resolvedByGov.GetValueOrDefault(gov.Key, 0)
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

            var completedTransfersForActivity = completedTransfers;

            var transferActivity = completedTransfersForActivity
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

            var orderedHotspots = hotspots.OrderByDescending(h => h.ActiveShortages).ToList();

            var registeredPharmacies = pharmacies
                .Select(p =>
                {
                    var pBatches = batches.Where(b => b.PharmacyId == p.Id).ToList();
                    var shortCount = pBatches.Count(b => b.Quantity <= b.MinStockLevel);
                    return new MOHPharmacyOverviewViewModel
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Governorate = p.Governorate,
                        BatchCount = pBatches.Count,
                        ActiveShortages = shortCount,
                        Status = !p.IsApproved && p.IsActive ? "Pending"
                            : !p.IsApproved && !p.IsActive ? "Rejected"
                            : !p.IsActive ? "Suspended"
                            : "Active"
                    };
                })
                .OrderBy(p => p.Name)
                .ToList();

            var shortagesByCategory = lowStockBatches
                .GroupBy(b => b.Drug?.Category ?? "Other")
                .Select(g => new MOHCategoryShortageViewModel { Category = g.Key, Count = g.Count() })
                .OrderByDescending(c => c.Count)
                .ToList();

            var shortageTrendLabels = new List<string>();
            var shortageTrendValues = new List<int>();
            for (var i = 5; i >= 0; i--)
            {
                var trendMonthStart = monthStart.AddMonths(-i);
                var trendMonthEnd = trendMonthStart.AddMonths(1);
                shortageTrendLabels.Add(trendMonthStart.ToString("MMM"));
                var monthCount = await _context.RestockRequests
                    .CountAsync(r => r.CreatedAt >= trendMonthStart && r.CreatedAt < trendMonthEnd);
                shortageTrendValues.Add(monthCount);
            }

            return new MOHDashboardViewModel
            {
                TotalPharmacies = pharmacies.Count,
                ActiveShortages = lowStockBatches.Count,
                ResolvedThisMonth = resolvedRestocksCount,
                TotalWasteThisMonth = expiredThisMonth.Sum(b => b.Quantity * (b.UnitPrice ?? 0)),
                TotalSavedViaTransfers = totalSaved,
                ShortageHotspots = orderedHotspots,
                TopShortages = topShortages,
                ExpiryWasteByRegion = expiryWasteByRegion,
                TransferActivity = transferActivity,
                ShortageChartLabels = orderedHotspots.Select(h => h.Governorate).ToList(),
                ShortageChartActive = orderedHotspots.Select(h => h.ActiveShortages).ToList(),
                ShortageChartResolved = orderedHotspots.Select(h => h.ResolvedThisMonth).ToList(),
                RegisteredPharmacies = registeredPharmacies,
                ShortagesByCategory = shortagesByCategory,
                ShortageTrendLabels = shortageTrendLabels,
                ShortageTrendValues = shortageTrendValues
            };
        }

        public async Task<MOHGovernorateDetailsViewModel> GetMOHGovernorateDetailsAsync(string governorate)
        {
            var gov = governorate.Trim();
            var pharmacyIds = await _context.Pharmacies
                .AsNoTracking()
                .Where(p => p.Governorate == gov)
                .Select(p => p.Id)
                .ToListAsync();

            var now = DateTime.Now;

            var shortages = await _context.MedicationBatches
                .AsNoTracking()
                .Where(b => pharmacyIds.Contains(b.PharmacyId) && b.Quantity <= b.MinStockLevel)
                .Include(b => b.Drug)
                .ToListAsync();

            var shortageList = shortages
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
                        GovernoratesAffected = 1,
                        DaysSinceFirstReported = Math.Max(0, (int)(now - firstSeen).TotalDays)
                    };
                })
                .OrderByDescending(s => s.PharmaciesShort)
                .ToList();

            var transfers = await _context.TransferRequests
                .AsNoTracking()
                .Include(t => t.SurplusPost).ThenInclude(s => s.Drug)
                .Include(t => t.SurplusPost).ThenInclude(s => s.Pharmacy)
                .Include(t => t.RequestingPharmacy)
                .Where(t => pharmacyIds.Contains(t.RequestingPharmacyId) ||
                            pharmacyIds.Contains(t.SurplusPost.PharmacyId))
                .OrderByDescending(t => t.RequestedAt)
                .Take(10)
                .ToListAsync();

            return new MOHGovernorateDetailsViewModel
            {
                Governorate = gov,
                Shortages = shortageList,
                RecentTransfers = transfers.Select(t => new TransferRequestViewModel
                {
                    Id = t.Id,
                    SurplusPostId = t.SurplusPostId,
                    DrugName = t.SurplusPost.Drug?.GenericName ?? "Unknown",
                    Quantity = t.Quantity,
                    FromPharmacy = t.SurplusPost.Pharmacy?.Name ?? "Unknown",
                    FromGovernorate = t.SurplusPost.Pharmacy?.Governorate ?? "—",
                    ToPharmacy = t.RequestingPharmacy?.Name ?? "Unknown",
                    ToGovernorate = t.RequestingPharmacy?.Governorate ?? "—",
                    Status = t.Status.ToString(),
                    RequestedAt = t.RequestedAt,
                    CompletedAt = t.CompletedAt,
                    Notes = t.Notes ?? ""
                }).ToList()
            };
        }

        public async Task<DistributorDashboardViewModel> GetDistributorDashboardAsync(int? distributorPharmacyId)
        {
            var now = DateTime.Now;
            var monthStart = new DateTime(now.Year, now.Month, 1);

            var pending = await _context.RestockRequests
                .AsNoTracking()
                .CountAsync(r => r.Status == RestockStatus.Pending);

            var deliveredThisMonth = await _context.RestockRequests
                .AsNoTracking()
                .CountAsync(r => r.Status == RestockStatus.Delivered &&
                                 r.ConfirmedAt >= monthStart);

            var allRequests = await _context.RestockRequests
                .AsNoTracking()
                .Include(r => r.Drug)
                .Include(r => r.Pharmacy)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            var pharmacyIds = allRequests.Select(r => r.PharmacyId).Distinct().ToList();
            var drugIds = allRequests.Select(r => r.DrugId).Distinct().ToList();
            var stockLookup = await _context.MedicationBatches
                .AsNoTracking()
                .Where(b => pharmacyIds.Contains(b.PharmacyId) && drugIds.Contains(b.DrugId))
                .ToListAsync();

            var mappedRequests = MapRestockRequests(allRequests, stockLookup);
            var recentRequests = mappedRequests.Take(15).ToList();
            var activeDeliveries = mappedRequests
                .Where(r => r.Status == RestockStatus.Confirmed.ToString() || r.Status == RestockStatus.Delivered.ToString())
                .ToList();

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

            var demandByGovernorate = regionalDemand
                .GroupBy(r => r.Governorate)
                .Select(g => new { Governorate = g.Key, Total = g.Sum(r => r.TotalQuantityNeeded) })
                .OrderByDescending(g => g.Total)
                .ToList();

            var topRequestedDrugs = allRequests
                .GroupBy(r => r.DrugId)
                .Select(g => new DrugDemandViewModel
                {
                    DrugName = g.First().Drug?.GenericName ?? "Unknown",
                    TotalQuantity = g.Sum(r => r.RequestedQuantity)
                })
                .OrderByDescending(d => d.TotalQuantity)
                .Take(10)
                .ToList();

            return new DistributorDashboardViewModel
            {
                DistributorName = "Jordan Pharma Distribution",
                PendingRestockRequests = pending,
                ConfirmedDeliveries = deliveredThisMonth,
                RecentRequests = recentRequests,
                AllRestockRequests = mappedRequests,
                ActiveDeliveries = activeDeliveries,
                RegionalDemand = regionalDemand,
                TopRequestedDrugs = topRequestedDrugs,
                DemandChartLabels = demandByGovernorate.Select(g => g.Governorate).ToList(),
                DemandChartValues = demandByGovernorate.Select(g => g.Total).ToList()
            };
        }

        public async Task<int> GetPendingRestockCountAsync()
        {
            return await _context.RestockRequests
                .AsNoTracking()
                .CountAsync(r => r.Status == RestockStatus.Pending);
        }

        public async Task<RestockRequestsPageViewModel> GetRestockRequestsPageAsync(string? status, string? governorate)
        {
            var pending = await GetPendingRestockCountAsync();

            var query = _context.RestockRequests
                .AsNoTracking()
                .Include(r => r.Drug)
                .Include(r => r.Pharmacy)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status) &&
                Enum.TryParse<RestockStatus>(status, ignoreCase: true, out var statusEnum))
            {
                query = query.Where(r => r.Status == statusEnum);
            }

            if (!string.IsNullOrWhiteSpace(governorate))
            {
                var gov = governorate.Trim();
                query = query.Where(r => r.Pharmacy!.Governorate == gov);
            }

            var requests = await query
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            var pharmacyIds = requests.Select(r => r.PharmacyId).Distinct().ToList();
            var drugIds = requests.Select(r => r.DrugId).Distinct().ToList();
            var stockLookup = pharmacyIds.Count == 0 || drugIds.Count == 0
                ? new List<MedicationBatch>()
                : await _context.MedicationBatches
                    .AsNoTracking()
                    .Where(b => pharmacyIds.Contains(b.PharmacyId) && drugIds.Contains(b.DrugId))
                    .ToListAsync();

            var governorates = await _context.Pharmacies
                .AsNoTracking()
                .Select(p => p.Governorate)
                .Distinct()
                .OrderBy(g => g)
                .ToListAsync();

            return new RestockRequestsPageViewModel
            {
                PendingCount = pending,
                Requests = MapRestockRequests(requests, stockLookup),
                StatusFilter = status,
                GovernorateFilter = governorate,
                Governorates = governorates
            };
        }

        public async Task<bool> ConfirmRestockRequestAsync(int id)
        {
            var request = await _context.RestockRequests.FirstOrDefaultAsync(r => r.Id == id);
            if (request == null || request.Status != RestockStatus.Pending)
                return false;

            request.Status = RestockStatus.Confirmed;
            request.ConfirmedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> MarkRestockDeliveredAsync(int id)
        {
            var request = await _context.RestockRequests
                .Include(r => r.Drug)
                .FirstOrDefaultAsync(r => r.Id == id);
            if (request == null || request.Status != RestockStatus.Confirmed)
                return false;

            request.Status = RestockStatus.Delivered;

            var batch = await _context.MedicationBatches
                .Where(b => b.PharmacyId == request.PharmacyId && b.DrugId == request.DrugId)
                .OrderByDescending(b => b.AddedAt)
                .FirstOrDefaultAsync();

            if (batch != null)
                batch.Quantity += request.RequestedQuantity;

            await _context.SaveChangesAsync();
            return true;
        }

        public IReadOnlyList<string> GetMohGovernorates() =>
            GovernorateCoords.Keys.OrderBy(g => g).ToList();

        public async Task<(byte[] Content, string Filename)> GenerateMOHReportCsvAsync(
            string reportType, DateTime? from, DateTime? to, string? governorate)
        {
            var now = DateTime.Now;
            var fromDate = from?.Date ?? now.AddMonths(-1).Date;
            var toDate = to?.Date ?? now.Date;
            if (toDate < fromDate)
                (fromDate, toDate) = (toDate, fromDate);

            var toExclusive = toDate.AddDays(1);
            var govFilter = NormalizeGovernorateFilter(governorate);
            var typeKey = NormalizeReportType(reportType);

            byte[] content;
            switch (typeKey)
            {
                case "expiry-waste":
                    content = await BuildExpiryWasteReportAsync(fromDate, toExclusive, govFilter);
                    break;
                case "transfer-activity":
                    content = await BuildTransferActivityReportAsync(fromDate, toExclusive, govFilter);
                    break;
                case "pharmacy-compliance":
                    content = await BuildPharmacyComplianceReportAsync(fromDate, toExclusive, govFilter);
                    break;
                default:
                    content = await BuildShortageSummaryReportAsync(fromDate, toExclusive, govFilter);
                    typeKey = "shortage-summary";
                    break;
            }

            var filename = $"medtrack-{typeKey}-{fromDate:yyyyMMdd}-{toDate:yyyyMMdd}.csv";
            return (content, filename);
        }

        private static string NormalizeReportType(string? reportType)
        {
            if (string.IsNullOrWhiteSpace(reportType))
                return "shortage-summary";

            var key = reportType.Trim().ToLowerInvariant();
            if (key is "shortage-summary" or "national shortage summary")
                return "shortage-summary";
            if (key is "expiry-waste" or "expiry waste by region")
                return "expiry-waste";
            if (key is "transfer-activity" or "transfer activity")
                return "transfer-activity";
            if (key is "pharmacy-compliance" or "pharmacy compliance")
                return "pharmacy-compliance";
            return key;
        }

        private static string? NormalizeGovernorateFilter(string? governorate)
        {
            if (string.IsNullOrWhiteSpace(governorate))
                return null;
            if (governorate.Equals("All Governorates", StringComparison.OrdinalIgnoreCase))
                return null;
            return governorate.Trim();
        }

        private static bool MatchesGovernorate(string? pharmacyGov, string? filter)
        {
            if (filter == null)
                return true;
            return string.Equals(pharmacyGov?.Trim(), filter, StringComparison.OrdinalIgnoreCase);
        }

        private async Task<HashSet<int>> GetFilteredPharmacyIdsAsync(string? governorate)
        {
            var pharmacies = await _context.Pharmacies.AsNoTracking().ToListAsync();
            return pharmacies
                .Where(p => MatchesGovernorate(p.Governorate, governorate))
                .Select(p => p.Id)
                .ToHashSet();
        }

        private async Task<byte[]> BuildShortageSummaryReportAsync(
            DateTime fromDate, DateTime toExclusive, string? governorate)
        {
            var pharmacyIds = await GetFilteredPharmacyIdsAsync(governorate);
            var batches = await _context.MedicationBatches
                .AsNoTracking()
                .Include(b => b.Drug)
                .Include(b => b.Pharmacy)
                .Where(b => pharmacyIds.Contains(b.PharmacyId))
                .Where(b => b.Quantity <= b.MinStockLevel)
                .ToListAsync();

            var rows = batches
                .GroupBy(b => b.DrugId)
                .Select(g =>
                {
                    var drug = g.First().Drug;
                    var firstSeen = g.Min(b => b.AddedAt);
                    return new[]
                    {
                        drug?.GenericName ?? "Unknown",
                        drug?.Category ?? "Other",
                        g.Select(b => b.PharmacyId).Distinct().Count().ToString(),
                        g.Select(b => b.Pharmacy?.Governorate ?? "").Where(x => x != "").Distinct().Count().ToString(),
                        Math.Max(0, (int)(DateTime.Now - firstSeen).TotalDays).ToString()
                    };
                })
                .OrderByDescending(r => int.Parse(r[2]))
                .ToList();

            return BuildCsv(
                new[] { "Drug Name", "Category", "Pharmacies Short", "Governorates Affected", "Days Since First Reported" },
                rows);
        }

        private async Task<byte[]> BuildExpiryWasteReportAsync(
            DateTime fromDate, DateTime toExclusive, string? governorate)
        {
            var pharmacyIds = await GetFilteredPharmacyIdsAsync(governorate);
            var pharmacies = await _context.Pharmacies.AsNoTracking()
                .Where(p => pharmacyIds.Contains(p.Id))
                .ToListAsync();

            var expired = await _context.MedicationBatches
                .AsNoTracking()
                .Where(b => pharmacyIds.Contains(b.PharmacyId))
                .Where(b => b.ExpiryDate >= fromDate && b.ExpiryDate < toExclusive)
                .ToListAsync();

            var rows = pharmacies
                .GroupBy(p => p.Governorate)
                .Select(gov =>
                {
                    var govIds = gov.Select(p => p.Id).ToHashSet();
                    var govBatches = expired.Where(b => govIds.Contains(b.PharmacyId)).ToList();
                    var wasteValue = govBatches.Sum(b => b.Quantity * (b.UnitPrice ?? 0));
                    var items = govBatches.Sum(b => b.Quantity);
                    return new[]
                    {
                        gov.Key,
                        items.ToString(),
                        wasteValue.ToString("F2"),
                        govBatches.Count.ToString()
                    };
                })
                .Where(r => int.Parse(r[1]) > 0 || decimal.Parse(r[2]) > 0)
                .OrderByDescending(r => decimal.Parse(r[2]))
                .ToList();

            return BuildCsv(
                new[] { "Governorate", "Items Expiring", "Waste Value (JOD)", "Batch Count" },
                rows);
        }

        private async Task<byte[]> BuildTransferActivityReportAsync(
            DateTime fromDate, DateTime toExclusive, string? governorate)
        {
            var transfers = await _context.TransferRequests
                .AsNoTracking()
                .Include(t => t.SurplusPost).ThenInclude(s => s.Drug)
                .Include(t => t.SurplusPost).ThenInclude(s => s.Pharmacy)
                .Include(t => t.RequestingPharmacy)
                .Where(t => t.Status == TransferStatus.Completed && t.CompletedAt != null)
                .Where(t => t.CompletedAt >= fromDate && t.CompletedAt < toExclusive)
                .OrderByDescending(t => t.CompletedAt)
                .ToListAsync();

            if (governorate != null)
                transfers = transfers
                    .Where(t =>
                        MatchesGovernorate(t.RequestingPharmacy?.Governorate, governorate) ||
                        MatchesGovernorate(t.SurplusPost?.Pharmacy?.Governorate, governorate))
                    .ToList();

            var rows = transfers.Select(t => new[]
            {
                t.CompletedAt!.Value.ToString("yyyy-MM-dd"),
                t.SurplusPost?.Drug?.GenericName ?? "Unknown",
                t.Quantity.ToString(),
                t.SurplusPost?.Pharmacy?.Name ?? "Unknown",
                t.SurplusPost?.Pharmacy?.Governorate ?? "—",
                t.RequestingPharmacy?.Name ?? "Unknown",
                t.RequestingPharmacy?.Governorate ?? "—",
                (t.Quantity * (t.SurplusPost?.Price ?? 0)).ToString("F2")
            }).ToList();

            return BuildCsv(
                new[] { "Completed Date", "Drug", "Quantity", "From Pharmacy", "From Governorate", "To Pharmacy", "To Governorate", "Value (JOD)" },
                rows);
        }

        private async Task<byte[]> BuildPharmacyComplianceReportAsync(
            DateTime fromDate, DateTime toExclusive, string? governorate)
        {
            var pharmacies = await _context.Pharmacies.AsNoTracking()
                .Where(p => p.CreatedAt < toExclusive)
                .ToListAsync();

            if (governorate != null)
                pharmacies = pharmacies.Where(p => MatchesGovernorate(p.Governorate, governorate)).ToList();

            var pharmacyIds = pharmacies.Select(p => p.Id).ToHashSet();
            var batches = await _context.MedicationBatches
                .AsNoTracking()
                .Where(b => pharmacyIds.Contains(b.PharmacyId))
                .ToListAsync();

            var rows = pharmacies.Select(p =>
            {
                var pBatches = batches.Where(b => b.PharmacyId == p.Id).ToList();
                var shortCount = pBatches.Count(b => b.Quantity <= b.MinStockLevel);
                var status = !p.IsApproved && p.IsActive ? "Pending"
                    : !p.IsApproved && !p.IsActive ? "Rejected"
                    : !p.IsActive ? "Suspended"
                    : "Active";
                var compliance = shortCount == 0 && p.IsApproved && p.IsActive ? "Compliant" : "At Risk";

                return new[]
                {
                    p.Name,
                    p.LicenseNo,
                    p.Governorate,
                    status,
                    pBatches.Count.ToString(),
                    shortCount.ToString(),
                    compliance,
                    p.CreatedAt.ToString("yyyy-MM-dd")
                };
            })
            .OrderBy(r => r[2])
            .ThenBy(r => r[0])
            .ToList();

            return BuildCsv(
                new[] { "Pharmacy", "License No", "Governorate", "Account Status", "Inventory Batches", "Active Shortages", "Compliance", "Registered" },
                rows);
        }

        private static byte[] BuildCsv(IReadOnlyList<string> headers, List<string[]> rows)
        {
            var lines = new List<string> { string.Join(",", headers.Select(CsvEscape)) };
            foreach (var row in rows)
                lines.Add(string.Join(",", row.Select(CsvEscape)));

            return Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(string.Join("\r\n", lines))).ToArray();
        }

        private static string CsvEscape(string? value)
        {
            value ??= "";
            if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            return value;
        }

        private static List<RestockRequestViewModel> MapRestockRequests(
            List<RestockRequest> requests,
            List<MedicationBatch> stockLookup)
        {
            return requests.Select(r =>
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
            }).ToList();
        }
    }
}
