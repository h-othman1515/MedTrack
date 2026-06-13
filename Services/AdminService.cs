using MedTrack.Data;
using MedTrack.Models;
using MedTrack.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MedTrack.Services
{
    public interface IAdminService
    {
        Task<AdminDashboardViewModel> GetDashboardAsync();
        Task<UserManagementViewModel> GetUsersAsync(UserManager<MedTrack.Models.ApplicationUser> userManager);
        Task<PharmacyManagementViewModel> GetPharmaciesAsync();
        Task<bool> ApprovePharmacyAsync(int id, string? adminId, string? adminName);
        Task<bool> RejectPharmacyAsync(int id, string? adminId, string? adminName);
        Task<bool> SuspendPharmacyAsync(int id, string? adminId, string? adminName);
        Task<bool> ReactivatePharmacyAsync(int id, string? adminId, string? adminName);
        Task<bool> DeletePharmacyAsync(int id, string? adminId, string? adminName);
        Task<DrugCatalogViewModel> GetDrugsAsync();
        Task<AuditLogViewModel> GetAuditLogsAsync();
        Task<BroadcastViewModel> GetBroadcastHistoryAsync();
    }

    public class AdminService : IAdminService
    {
        private readonly ApplicationDbContext _context;

        public AdminService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<AdminDashboardViewModel> GetDashboardAsync()
        {
            var now = DateTime.UtcNow;
            var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            var pendingPharmacies = await _context.Pharmacies
                .AsNoTracking()
                .Where(p => !p.IsApproved && p.IsActive)
                .OrderByDescending(p => p.CreatedAt)
                .Take(10)
                .ToListAsync();

            var managerLookup = await _context.Users
                .AsNoTracking()
                .Where(u => u.PharmacyId != null)
                .GroupBy(u => u.PharmacyId!.Value)
                .Select(g => new { PharmacyId = g.Key, Name = g.OrderBy(u => u.CreatedAt).First().FullName })
                .ToDictionaryAsync(x => x.PharmacyId, x => x.Name);

            var recentActivity = await _context.AuditLogs
                .AsNoTracking()
                .OrderByDescending(l => l.Timestamp)
                .Take(10)
                .Select(l => new ActivityLogViewModel
                {
                    Type = l.EntityType,
                    Action = l.Action,
                    Details = l.Details,
                    PerformedBy = l.UserName ?? "System",
                    Timestamp = l.Timestamp
                })
                .ToListAsync();

            var batches = await _context.MedicationBatches.AsNoTracking().ToListAsync();
            var totalInventoryValue = batches.Sum(b => b.Quantity * (b.UnitPrice ?? 0));

            var completedTransfers = await _context.TransferRequests
                .AsNoTracking()
                .Include(t => t.SurplusPost)
                .Where(t => t.Status == TransferStatus.Completed && t.CompletedAt >= monthStart)
                .ToListAsync();

            var wastePrevented = completedTransfers.Sum(t => t.Quantity * (t.SurplusPost?.Price ?? 0));

            return new AdminDashboardViewModel
            {
                TotalUsers = await _context.Users.CountAsync(),
                ActivePharmacies = await _context.Pharmacies.CountAsync(p => p.IsApproved && p.IsActive),
                PendingApprovals = await _context.Pharmacies.CountAsync(p => !p.IsApproved && p.IsActive),
                TotalDrugs = await _context.Drugs.CountAsync(),
                ActiveAlerts = await _context.MedicationBatches.CountAsync(b => b.Quantity <= b.MinStockLevel),
                TotalTransfers = await _context.TransferRequests.CountAsync(),
                PendingPharmacies = pendingPharmacies.Select(p => new PendingPharmacyViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    ManagerName = managerLookup.GetValueOrDefault(p.Id, "—"),
                    LicenseNo = p.LicenseNo,
                    Governorate = p.Governorate,
                    RegisteredAt = p.CreatedAt
                }).ToList(),
                RecentActivity = recentActivity,
                TotalInventoryValue = totalInventoryValue,
                WastePreventedMonth = wastePrevented,
                ActiveSubscriptions = await _context.Pharmacies.CountAsync(p => p.IsApproved && p.IsActive)
            };
        }

        public async Task<UserManagementViewModel> GetUsersAsync(UserManager<MedTrack.Models.ApplicationUser> userManager)
        {
            var users = await _context.Users
                .AsNoTracking()
                .Include(u => u.Pharmacy)
                .OrderBy(u => u.FullName)
                .ToListAsync();

            var result = new List<UserListViewModel>();
            foreach (var user in users)
            {
                var roles = await userManager.GetRolesAsync(user);
                var role = roles.FirstOrDefault() ?? "—";
                result.Add(new UserListViewModel
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email ?? "",
                    Role = role,
                    PharmacyName = user.Pharmacy?.Name ?? "—",
                    Governorate = user.Pharmacy?.Governorate ?? "—",
                    Status = !user.IsActive ? "Suspended" : "Active",
                    LastLogin = null,
                    CreatedAt = user.CreatedAt
                });
            }

            return new UserManagementViewModel { Users = result };
        }

        public async Task<PharmacyManagementViewModel> GetPharmaciesAsync()
        {
            var pharmacies = await _context.Pharmacies
                .AsNoTracking()
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            var inventoryCounts = await _context.MedicationBatches
                .AsNoTracking()
                .GroupBy(b => b.PharmacyId)
                .Select(g => new { PharmacyId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.PharmacyId, x => x.Count);

            var managers = await _context.Users
                .AsNoTracking()
                .Where(u => u.PharmacyId != null)
                .GroupBy(u => u.PharmacyId!.Value)
                .Select(g => new { PharmacyId = g.Key, Name = g.OrderBy(u => u.CreatedAt).First().FullName })
                .ToDictionaryAsync(x => x.PharmacyId, x => x.Name);

            var list = pharmacies.Select(p => new PharmacyListViewModel
            {
                Id = p.Id,
                Name = p.Name,
                Email = p.ContactEmail ?? "—",
                LicenseNo = p.LicenseNo,
                Governorate = p.Governorate,
                ManagerName = managers.GetValueOrDefault(p.Id, "—"),
                Tier = p.Tier.ToString(),
                InventoryCount = inventoryCounts.GetValueOrDefault(p.Id, 0),
                Status = GetPharmacyStatus(p),
                CreatedAt = p.CreatedAt
            }).ToList();

            return new PharmacyManagementViewModel
            {
                Pharmacies = list,
                TotalCount = list.Count,
                ActiveCount = list.Count(p => p.Status == "Active"),
                PendingCount = list.Count(p => p.Status == "Pending"),
                SuspendedCount = list.Count(p => p.Status == "Suspended")
            };
        }

        public async Task<bool> ApprovePharmacyAsync(int id, string? adminId, string? adminName)
        {
            var pharmacy = await _context.Pharmacies.FirstOrDefaultAsync(p => p.Id == id);
            if (pharmacy == null || pharmacy.IsApproved)
                return false;

            pharmacy.IsApproved = true;
            pharmacy.IsActive = true;

            var staff = await _context.Users.Where(u => u.PharmacyId == id).ToListAsync();
            foreach (var user in staff)
                user.IsActive = true;

            await AddAuditLogAsync(adminId, adminName, "Approve", "Pharmacy",
                $"Approved pharmacy {pharmacy.Name} ({pharmacy.LicenseNo})");
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RejectPharmacyAsync(int id, string? adminId, string? adminName)
        {
            var pharmacy = await _context.Pharmacies.FirstOrDefaultAsync(p => p.Id == id);
            if (pharmacy == null || pharmacy.IsApproved || !pharmacy.IsActive)
                return false;

            pharmacy.IsActive = false;

            var staff = await _context.Users.Where(u => u.PharmacyId == id).ToListAsync();
            foreach (var user in staff)
                user.IsActive = false;

            await AddAuditLogAsync(adminId, adminName, "Reject", "Pharmacy",
                $"Rejected pharmacy registration {pharmacy.Name} ({pharmacy.LicenseNo})", "Medium");
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SuspendPharmacyAsync(int id, string? adminId, string? adminName)
        {
            var pharmacy = await _context.Pharmacies.FirstOrDefaultAsync(p => p.Id == id);
            if (pharmacy == null || !pharmacy.IsApproved || !pharmacy.IsActive)
                return false;

            pharmacy.IsActive = false;

            var staff = await _context.Users.Where(u => u.PharmacyId == id).ToListAsync();
            foreach (var user in staff)
                user.IsActive = false;

            await AddAuditLogAsync(adminId, adminName, "Suspend", "Pharmacy",
                $"Suspended pharmacy {pharmacy.Name} ({pharmacy.LicenseNo})", "Medium");
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ReactivatePharmacyAsync(int id, string? adminId, string? adminName)
        {
            var pharmacy = await _context.Pharmacies.FirstOrDefaultAsync(p => p.Id == id);
            if (pharmacy == null || !pharmacy.IsApproved || pharmacy.IsActive)
                return false;

            pharmacy.IsActive = true;

            var staff = await _context.Users.Where(u => u.PharmacyId == id).ToListAsync();
            foreach (var user in staff)
                user.IsActive = true;

            await AddAuditLogAsync(adminId, adminName, "Reactivate", "Pharmacy",
                $"Reactivated pharmacy {pharmacy.Name} ({pharmacy.LicenseNo})");
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeletePharmacyAsync(int id, string? adminId, string? adminName)
        {
            var pharmacy = await _context.Pharmacies.FirstOrDefaultAsync(p => p.Id == id);
            if (pharmacy == null)
                return false;

            var name = pharmacy.Name;
            var license = pharmacy.LicenseNo;

            var staff = await _context.Users.Where(u => u.PharmacyId == id).ToListAsync();
            foreach (var user in staff)
                user.PharmacyId = null;

            _context.Pharmacies.Remove(pharmacy);

            await AddAuditLogAsync(adminId, adminName, "Delete", "Pharmacy",
                $"Deleted pharmacy {name} ({license})", "High");
            await _context.SaveChangesAsync();
            return true;
        }

        private static string GetPharmacyStatus(Pharmacy p) =>
            !p.IsApproved && p.IsActive ? "Pending"
            : !p.IsApproved && !p.IsActive ? "Rejected"
            : !p.IsActive ? "Suspended"
            : "Active";

        private async Task AddAuditLogAsync(
            string? userId,
            string? userName,
            string action,
            string entityType,
            string details,
            string severity = "Low")
        {
            _context.AuditLogs.Add(new AuditLog
            {
                UserId = userId,
                UserName = userName,
                Action = action,
                EntityType = entityType,
                Details = details,
                Severity = severity,
                Timestamp = DateTime.UtcNow
            });
        }

        public async Task<DrugCatalogViewModel> GetDrugsAsync()
        {
            var drugs = await _context.Drugs
                .AsNoTracking()
                .OrderBy(d => d.GenericName)
                .ToListAsync();

            var categories = drugs
                .Where(d => !string.IsNullOrWhiteSpace(d.Category))
                .Select(d => d.Category!)
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            return new DrugCatalogViewModel
            {
                TotalDrugs = drugs.Count,
                Categories = categories,
                Drugs = drugs.Select(d => new DrugListViewModel
                {
                    Id = d.Id,
                    GenericName = d.GenericName,
                    ScientificName = d.GenericName,
                    BrandNames = d.BrandNames ?? "—",
                    Category = d.Category ?? "Other",
                    DefaultMinStock = d.DefaultMinStockLevel,
                    DefaultUnit = d.DefaultUnit ?? "Tablets",
                    RequiresPrescription = d.RequiresPrescription,
                    IsCritical = d.IsCritical,
                    IsActive = d.IsActive
                }).ToList()
            };
        }

        public async Task<AuditLogViewModel> GetAuditLogsAsync()
        {
            var logs = await _context.AuditLogs
                .AsNoTracking()
                .OrderByDescending(l => l.Timestamp)
                .Take(100)
                .Select(l => new AuditLogEntryViewModel
                {
                    Timestamp = l.Timestamp,
                    UserName = l.UserName ?? "System",
                    UserRole = "—",
                    Action = l.Action,
                    EntityType = l.EntityType,
                    Details = l.Details,
                    IpAddress = l.IpAddress ?? "—",
                    Severity = l.Severity
                })
                .ToListAsync();

            return new AuditLogViewModel { Logs = logs };
        }

        public async Task<BroadcastViewModel> GetBroadcastHistoryAsync()
        {
            var broadcasts = await _context.Notifications
                .AsNoTracking()
                .Where(n => n.Type == NotificationType.System)
                .OrderByDescending(n => n.SentAt)
                .Take(20)
                .ToListAsync();

            var pharmacyCount = await _context.Pharmacies.CountAsync(p => p.IsApproved && p.IsActive);

            return new BroadcastViewModel
            {
                RecentBroadcasts = broadcasts.Select(b => new BroadcastHistoryViewModel
                {
                    Subject = b.Message.Length > 50 ? b.Message[..50] + "…" : b.Message,
                    Type = "Alert",
                    Message = b.Message,
                    SentAt = b.SentAt,
                    RecipientsCount = pharmacyCount
                }).ToList()
            };
        }
    }
}
