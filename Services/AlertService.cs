using MedTrack.Data;
using MedTrack.Models;
using MedTrack.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace MedTrack.Services
{
    public interface IAlertService
    {
        Task<List<AlertViewModel>> GetPharmacyAlertsAsync(int pharmacyId);
        Task<List<AlertViewModel>> GetUnreadAlertsAsync(int pharmacyId);
        Task AcknowledgeAlertAsync(int alertId, int pharmacyId);
        Task MarkAllReadAsync(int pharmacyId);
        Task ClearAcknowledgedAsync(int pharmacyId);
        Task<int> ScanAndCreateExpiryAlertsAsync();
        Task<int> DetectShortagesAsync();
    }

    public class AlertService : IAlertService
    {
        private readonly ApplicationDbContext _context;

        public AlertService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<AlertViewModel>> GetPharmacyAlertsAsync(int pharmacyId)
        {
            var alerts = new List<AlertViewModel>();
            var dismissedKeys = await GetDismissedKeysAsync(pharmacyId);

            var expiryAlerts = await _context.ExpiryAlerts
                .AsNoTracking()
                .Where(a => a.MedicationBatch.PharmacyId == pharmacyId)
                .Include(a => a.MedicationBatch).ThenInclude(b => b.Drug)
                .OrderByDescending(a => a.SentAt)
                .ToListAsync();

            alerts.AddRange(expiryAlerts.Select(a => new AlertViewModel
            {
                Id = a.Id,
                BatchId = a.BatchId,
                DrugId = a.MedicationBatch.DrugId,
                Type = "Expiry",
                Level = a.AlertLevel.ToString(),
                DrugName = a.MedicationBatch.Drug?.GenericName ?? "Unknown",
                BatchNo = a.MedicationBatch.BatchNo ?? "N/A",
                Message = $"{a.MedicationBatch.Drug?.GenericName} expires in {(int)a.AlertLevel} days.",
                SentAt = a.SentAt ?? DateTime.Now,
                Channel = a.Channel?.ToString() ?? "InApp",
                IsAcknowledged = a.IsAcknowledged,
                SuggestedAction = a.AlertLevel == AlertLevel._7 ? "Dispose" : "Surplus"
            }));

            var lowStock = await _context.MedicationBatches
                .AsNoTracking()
                .Where(b => b.PharmacyId == pharmacyId && b.Quantity <= b.MinStockLevel)
                .Include(b => b.Drug)
                .ToListAsync();

            alerts.AddRange(lowStock
                .Where(b => !dismissedKeys.Contains(ShortageDismissKey(b.Id)))
                .Select(b => new AlertViewModel
                {
                    Id = b.Id + 10000,
                    BatchId = b.Id,
                    DrugId = b.DrugId,
                    Type = "Shortage",
                    Level = b.Quantity == 0 ? "Critical" : "High",
                    DrugName = b.Drug?.GenericName ?? "Unknown",
                    BatchNo = b.BatchNo ?? "N/A",
                    Message = $"Stock below minimum ({b.Quantity}/{b.MinStockLevel}).",
                    SentAt = DateTime.Now.AddDays(-1),
                    Channel = "InApp",
                    IsAcknowledged = false
                }));

            var restockRequests = await _context.RestockRequests
                .AsNoTracking()
                .Where(r => r.PharmacyId == pharmacyId && r.Status == RestockStatus.Pending)
                .Include(r => r.Drug)
                .Include(r => r.Pharmacy)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            alerts.AddRange(restockRequests
                .Where(r => !dismissedKeys.Contains(RestockDismissKey(r.Id)))
                .Select(r => new AlertViewModel
                {
                    Id = r.Id + 20000,
                    DrugId = r.DrugId,
                    Type = "Restock",
                    Level = "Pending",
                    DrugName = r.Drug?.GenericName ?? "Unknown",
                    BatchNo = "N/A",
                    Message = $"Auto-restock request sent to distributor ({r.RequestedQuantity} units requested).",
                    SentAt = r.CreatedAt,
                    Channel = "InApp",
                    IsAcknowledged = false
                }));

            var transferUpdates = await _context.TransferRequests
                .AsNoTracking()
                .Where(t => (t.RequestingPharmacyId == pharmacyId || t.SurplusPost.PharmacyId == pharmacyId) &&
                            (t.Status == TransferStatus.Pending || t.Status == TransferStatus.Confirmed))
                .Include(t => t.SurplusPost).ThenInclude(s => s.Drug)
                .Include(t => t.SurplusPost).ThenInclude(s => s.Pharmacy)
                .Include(t => t.RequestingPharmacy)
                .OrderByDescending(t => t.RequestedAt)
                .ToListAsync();

            alerts.AddRange(transferUpdates
                .Where(t => !dismissedKeys.Contains(TransferDismissKey(t.Id)))
                .Select(t => new AlertViewModel
                {
                    Id = t.Id + 30000,
                    Type = "Transfer",
                    Level = t.Status.ToString(),
                    DrugName = t.SurplusPost.Drug?.GenericName ?? "Unknown",
                    BatchNo = "N/A",
                    Message = t.RequestingPharmacyId == pharmacyId
                        ? $"Transfer request to {t.SurplusPost.Pharmacy?.Name} — {t.Quantity} units ({t.Status})."
                        : $"Incoming transfer from {t.RequestingPharmacy?.Name} — {t.Quantity} units ({t.Status}).",
                    SentAt = t.RequestedAt,
                    Channel = "InApp",
                    IsAcknowledged = false
                }));

            return alerts.OrderByDescending(a => a.SentAt).ToList();
        }

        public async Task<List<AlertViewModel>> GetUnreadAlertsAsync(int pharmacyId)
        {
            var all = await GetPharmacyAlertsAsync(pharmacyId);
            return all.Where(a => !a.IsAcknowledged).ToList();
        }

        public async Task AcknowledgeAlertAsync(int alertId, int pharmacyId)
        {
            if (alertId >= 30000)
            {
                await UpsertDismissedNotificationAsync(pharmacyId, TransferDismissKey(alertId - 30000), NotificationType.Transfer);
                await _context.SaveChangesAsync();
                return;
            }

            if (alertId >= 20000)
            {
                await UpsertDismissedNotificationAsync(pharmacyId, RestockDismissKey(alertId - 20000), NotificationType.Restock);
                await _context.SaveChangesAsync();
                return;
            }

            if (alertId >= 10000)
            {
                await UpsertDismissedNotificationAsync(pharmacyId, ShortageDismissKey(alertId - 10000), NotificationType.Shortage);
                await _context.SaveChangesAsync();
                return;
            }

            var alert = await _context.ExpiryAlerts
                .Include(a => a.MedicationBatch)
                .FirstOrDefaultAsync(a => a.Id == alertId && a.MedicationBatch.PharmacyId == pharmacyId);

            if (alert != null)
            {
                alert.IsAcknowledged = true;
                alert.AcknowledgedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task MarkAllReadAsync(int pharmacyId)
        {
            var expiryAlerts = await _context.ExpiryAlerts
                .Where(a => a.MedicationBatch.PharmacyId == pharmacyId && !a.IsAcknowledged)
                .ToListAsync();

            foreach (var alert in expiryAlerts)
            {
                alert.IsAcknowledged = true;
                alert.AcknowledgedAt = DateTime.UtcNow;
            }

            var lowStock = await _context.MedicationBatches
                .Where(b => b.PharmacyId == pharmacyId && b.Quantity <= b.MinStockLevel)
                .ToListAsync();

            foreach (var batch in lowStock)
                await UpsertDismissedNotificationAsync(pharmacyId, ShortageDismissKey(batch.Id), NotificationType.Shortage);

            var pendingRestocks = await _context.RestockRequests
                .Where(r => r.PharmacyId == pharmacyId && r.Status == RestockStatus.Pending)
                .ToListAsync();

            foreach (var request in pendingRestocks)
                await UpsertDismissedNotificationAsync(pharmacyId, RestockDismissKey(request.Id), NotificationType.Restock);

            var activeTransfers = await _context.TransferRequests
                .Where(t => (t.RequestingPharmacyId == pharmacyId || t.SurplusPost.PharmacyId == pharmacyId) &&
                            (t.Status == TransferStatus.Pending || t.Status == TransferStatus.Confirmed))
                .ToListAsync();

            foreach (var transfer in activeTransfers)
                await UpsertDismissedNotificationAsync(pharmacyId, TransferDismissKey(transfer.Id), NotificationType.Transfer);

            await _context.SaveChangesAsync();
        }

        public async Task ClearAcknowledgedAsync(int pharmacyId)
        {
            var acknowledgedExpiry = await _context.ExpiryAlerts
                .Where(a => a.MedicationBatch.PharmacyId == pharmacyId && a.IsAcknowledged)
                .ToListAsync();

            _context.ExpiryAlerts.RemoveRange(acknowledgedExpiry);

            var dismissed = await _context.Notifications
                .Where(n => n.PharmacyId == pharmacyId && n.IsRead &&
                            (n.Type == NotificationType.Shortage ||
                             n.Type == NotificationType.Restock ||
                             n.Type == NotificationType.Transfer))
                .ToListAsync();

            _context.Notifications.RemoveRange(dismissed);
            await _context.SaveChangesAsync();
        }

        public async Task<int> ScanAndCreateExpiryAlertsAsync()
        {
            int created = 0;
            var thresholds = new[] { AlertLevel._60, AlertLevel._30, AlertLevel._7 };

            foreach (var level in thresholds)
            {
                var days = (int)level;
                var cutoff = DateTime.Now.AddDays(days);
                var batches = await _context.MedicationBatches
                    .Where(b => b.ExpiryDate <= cutoff && b.ExpiryDate > DateTime.Now)
                    .Include(b => b.ExpiryAlerts)
                    .ToListAsync();

                foreach (var batch in batches)
                {
                    var alreadyAlerted = batch.ExpiryAlerts.Any(a => a.AlertLevel == level && !a.IsAcknowledged);

                    if (!alreadyAlerted)
                    {
                        _context.ExpiryAlerts.Add(new ExpiryAlert
                        {
                            BatchId = batch.Id,
                            AlertLevel = level,
                            SentAt = DateTime.UtcNow,
                            Channel = NotificationChannel.InApp,
                            IsAcknowledged = false
                        });
                        created++;
                    }
                }
            }

            if (created > 0)
                await _context.SaveChangesAsync();

            return created;
        }

        public async Task<int> DetectShortagesAsync()
        {
            var shortages = await _context.MedicationBatches
                .Where(b => b.Quantity <= b.MinStockLevel)
                .GroupBy(b => new { b.Pharmacy.Governorate, b.DrugId })
                .Where(g => g.Count() >= 3)
                .Select(g => new { g.Key.Governorate, g.Key.DrugId, Count = g.Count() })
                .ToListAsync();

            foreach (var s in shortages)
            {
                var drug = await _context.Drugs.FindAsync(s.DrugId);
                var message = $"Regional shortage: {drug?.GenericName} in {s.Governorate} ({s.Count} pharmacies affected)";

                var exists = await _context.Notifications
                    .AnyAsync(n => n.Message == message && n.SentAt > DateTime.Now.Date);

                if (!exists)
                {
                    var mohPharmacies = await _context.Pharmacies
                        .Where(p => p.Staff.Any(u => u.PharmacyId == p.Id))
                        .Take(1)
                        .ToListAsync();

                    foreach (var p in mohPharmacies)
                    {
                        _context.Notifications.Add(new Notification
                        {
                            PharmacyId = p.Id,
                            Type = NotificationType.Shortage,
                            Message = message,
                            SentAt = DateTime.UtcNow
                        });
                    }
                }
            }

            await _context.SaveChangesAsync();
            return shortages.Count;
        }

        private async Task<HashSet<string>> GetDismissedKeysAsync(int pharmacyId)
        {
            var keys = await _context.Notifications
                .AsNoTracking()
                .Where(n => n.PharmacyId == pharmacyId && n.IsRead &&
                            n.Message.StartsWith("dismiss:"))
                .Select(n => n.Message)
                .ToListAsync();

            return keys.ToHashSet();
        }

        private static string ShortageDismissKey(int batchId) => $"dismiss:shortage:batch:{batchId}";
        private static string RestockDismissKey(int requestId) => $"dismiss:restock:{requestId}";
        private static string TransferDismissKey(int transferId) => $"dismiss:transfer:{transferId}";

        private async Task UpsertDismissedNotificationAsync(int pharmacyId, string key, NotificationType type)
        {
            var existing = await _context.Notifications
                .FirstOrDefaultAsync(n => n.PharmacyId == pharmacyId && n.Message == key);

            if (existing != null)
            {
                existing.IsRead = true;
                return;
            }

            _context.Notifications.Add(new Notification
            {
                PharmacyId = pharmacyId,
                Type = type,
                Message = key,
                Channel = NotificationChannel.InApp,
                SentAt = DateTime.UtcNow,
                IsRead = true
            });
        }
    }
}
