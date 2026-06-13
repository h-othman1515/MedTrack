using MedTrack.Data;
using MedTrack.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace MedTrack.Services
{
    public interface IReportsService
    {
        Task<WasteReportViewModel> GetWasteReportAsync(int pharmacyId, DateTime? from, DateTime? to);
    }

    public class ReportsService : IReportsService
    {
        private readonly ApplicationDbContext _context;

        public ReportsService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<WasteReportViewModel> GetWasteReportAsync(int pharmacyId, DateTime? from, DateTime? to)
        {
            var now = DateTime.Now;
            var periodStart = from?.Date ?? now.AddMonths(-1).Date;
            var periodEnd = to?.Date ?? now.Date;
            if (periodEnd < periodStart)
                (periodStart, periodEnd) = (periodEnd, periodStart);

            var periodEndExclusive = periodEnd.AddDays(1);

            var batches = await _context.MedicationBatches
                .AsNoTracking()
                .Include(b => b.Drug)
                .Where(b => b.PharmacyId == pharmacyId)
                .Where(b => b.ExpiryDate >= periodStart && b.ExpiryDate < periodEndExclusive)
                .OrderByDescending(b => b.ExpiryDate)
                .ToListAsync();

            var items = batches.Select(b =>
            {
                var unitPrice = b.UnitPrice ?? 0;
                return new WasteItemViewModel
                {
                    DrugName = b.Drug?.GenericName ?? "Unknown",
                    BatchNo = b.BatchNo ?? "—",
                    Quantity = b.Quantity,
                    UnitPrice = unitPrice,
                    TotalValue = b.Quantity * unitPrice,
                    ExpiryDate = b.ExpiryDate,
                    DisposalMethod = GetDisposalMethod(b.ExpiryDate, now)
                };
            }).ToList();

            return new WasteReportViewModel
            {
                PeriodStart = periodStart,
                PeriodEnd = periodEnd,
                TotalWasteValue = items.Sum(i => i.TotalValue),
                TotalItemsWasted = items.Sum(i => i.Quantity),
                Items = items
            };
        }

        private static string GetDisposalMethod(DateTime expiryDate, DateTime now) =>
            expiryDate > now ? "Scheduled disposal" : "Expired — regulated disposal";
    }
}
