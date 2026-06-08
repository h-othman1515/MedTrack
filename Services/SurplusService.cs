using MedTrack.Data;
using MedTrack.Models;
using MedTrack.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace MedTrack.Services
{
    public interface ISurplusService
    {
        Task<List<SurplusListingViewModel>> SearchAsync(SurplusSearchViewModel search);
        Task<SurplusListingViewModel?> GetByIdAsync(int id);
        Task<int> CreateAsync(int pharmacyId, SurplusCreateViewModel model);
        Task UpdateStatusAsync(int id, SurplusStatus status);
    }

    public class SurplusService : ISurplusService
    {
        private readonly ApplicationDbContext _context;

        public SurplusService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<SurplusListingViewModel>> SearchAsync(SurplusSearchViewModel search)
        {
            var query = _context.SurplusPosts
                .AsNoTracking()
                .Where(s => s.Status == SurplusStatus.Available)
                .Include(s => s.Drug)
                .Include(s => s.Pharmacy)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search.SearchTerm))
            {
                query = query.Where(s =>
                    (s.Drug != null && s.Drug.GenericName.Contains(search.SearchTerm)) ||
                    (s.Pharmacy != null && s.Pharmacy.Name.Contains(search.SearchTerm)));
            }

            if (!string.IsNullOrEmpty(search.Governorate))
                query = query.Where(s => s.Pharmacy != null && s.Pharmacy.Governorate == search.Governorate);

            if (!string.IsNullOrEmpty(search.Category))
                query = query.Where(s => s.Drug != null && s.Drug.Category == search.Category);

            if (search.MinDaysUntilExpiry.HasValue)
            {
                var cutoff = DateTime.Now.AddDays(search.MinDaysUntilExpiry.Value);
                query = query.Where(s => s.ExpiryDate == null || s.ExpiryDate >= cutoff);
            }

            var results = await query.OrderBy(s => s.ExpiryDate).ToListAsync();

            return results.Select(s => new SurplusListingViewModel
            {
                Id = s.Id,
                DrugName = s.Drug?.GenericName ?? "Unknown",
                GenericName = s.Drug?.GenericName ?? "Unknown",
                Category = s.Drug?.Category ?? "Unknown",
                Quantity = s.Quantity,
                Unit = s.Drug?.DefaultUnit ?? "units",
                ExpiryDate = s.ExpiryDate ?? DateTime.Now.AddYears(1),
                DaysUntilExpiry = s.ExpiryDate.HasValue ? (s.ExpiryDate.Value - DateTime.Now).Days : 365,
                PharmacyName = s.Pharmacy?.Name ?? "Unknown",
                Governorate = s.Pharmacy?.Governorate ?? "Unknown",
                Status = s.Status.ToString(),
                PostedAt = s.PostedAt,
                Condition = s.Condition,
                Price = s.Price
            }).ToList();
        }

        public async Task<SurplusListingViewModel?> GetByIdAsync(int id)
        {
            var s = await _context.SurplusPosts
                .AsNoTracking()
                .Include(s => s.Drug)
                .Include(s => s.Pharmacy)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (s == null) return null;

            return new SurplusListingViewModel
            {
                Id = s.Id,
                DrugName = s.Drug?.GenericName ?? "Unknown",
                Quantity = s.Quantity,
                PharmacyName = s.Pharmacy?.Name ?? "Unknown",
                Governorate = s.Pharmacy?.Governorate ?? "Unknown",
                Price = s.Price
            };
        }

        public async Task<int> CreateAsync(int pharmacyId, SurplusCreateViewModel model)
        {
            var post = new SurplusPost
            {
                PharmacyId = pharmacyId,
                DrugId = model.DrugId,
                Quantity = model.Quantity,
                ExpiryDate = model.ExpiryDate,
                Condition = model.Condition,
                Price = model.Price,
                Status = SurplusStatus.Available
            };

            _context.SurplusPosts.Add(post);
            await _context.SaveChangesAsync();
            return post.Id;
        }

        public async Task UpdateStatusAsync(int id, SurplusStatus status)
        {
            var post = await _context.SurplusPosts.FindAsync(id);
            if (post != null)
            {
                post.Status = status;
                await _context.SaveChangesAsync();
            }
        }
    }

    public interface ITransferService
    {
        Task<List<TransferRequestViewModel>> GetPharmacyTransfersAsync(int pharmacyId);
        Task<TransferRequestViewModel?> GetTransferAsync(int id, int pharmacyId);
        Task<int> CreateAsync(int requestingPharmacyId, TransferCreateViewModel model);
        Task UpdateStatusAsync(int id, TransferStatus status);
    }

    public class TransferService : ITransferService
    {
        private readonly ApplicationDbContext _context;

        public TransferService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<TransferRequestViewModel>> GetPharmacyTransfersAsync(int pharmacyId)
        {
            var requests = await _context.TransferRequests
                .AsNoTracking()
                .Where(t => t.SurplusPost.PharmacyId == pharmacyId || t.RequestingPharmacyId == pharmacyId)
                .Include(t => t.SurplusPost).ThenInclude(s => s.Drug)
                .Include(t => t.SurplusPost).ThenInclude(s => s.Pharmacy)
                .Include(t => t.RequestingPharmacy)
                .OrderByDescending(t => t.RequestedAt)
                .ToListAsync();

            return requests.Select(t => new TransferRequestViewModel
            {
                Id = t.Id,
                SurplusPostId = t.SurplusPostId,
                DrugName = t.SurplusPost.Drug?.GenericName ?? "Unknown",
                Quantity = t.Quantity,
                FromPharmacy = t.SurplusPost.Pharmacy?.Name ?? "Unknown",
                FromGovernorate = t.SurplusPost.Pharmacy?.Governorate ?? "Unknown",
                ToPharmacy = t.RequestingPharmacy?.Name ?? "Unknown",
                ToGovernorate = t.RequestingPharmacy?.Governorate ?? "Unknown",
                Status = t.Status.ToString(),
                RequestedAt = t.RequestedAt,
                CompletedAt = t.CompletedAt,
                Notes = t.Notes
            }).ToList();
        }

        public async Task<TransferRequestViewModel?> GetTransferAsync(int id, int pharmacyId)
        {
            var t = await _context.TransferRequests
                .AsNoTracking()
                .Where(x => x.Id == id && (x.SurplusPost.PharmacyId == pharmacyId || x.RequestingPharmacyId == pharmacyId))
                .Include(x => x.SurplusPost).ThenInclude(s => s.Drug)
                .Include(x => x.SurplusPost).ThenInclude(s => s.Pharmacy)
                .Include(x => x.RequestingPharmacy)
                .FirstOrDefaultAsync();

            if (t == null) return null;

            return new TransferRequestViewModel
            {
                Id = t.Id,
                SurplusPostId = t.SurplusPostId,
                DrugName = t.SurplusPost.Drug?.GenericName ?? "Unknown",
                Quantity = t.Quantity,
                FromPharmacy = t.SurplusPost.Pharmacy?.Name ?? "Unknown",
                FromGovernorate = t.SurplusPost.Pharmacy?.Governorate ?? "Unknown",
                ToPharmacy = t.RequestingPharmacy?.Name ?? "Unknown",
                ToGovernorate = t.RequestingPharmacy?.Governorate ?? "Unknown",
                Status = t.Status.ToString(),
                RequestedAt = t.RequestedAt,
                CompletedAt = t.CompletedAt,
                Notes = t.Notes
            };
        }

        public async Task<int> CreateAsync(int requestingPharmacyId, TransferCreateViewModel model)
        {
            var request = new TransferRequest
            {
                SurplusPostId = model.SurplusPostId,
                RequestingPharmacyId = requestingPharmacyId,
                Quantity = model.Quantity,
                Notes = model.Notes,
                Status = TransferStatus.Pending
            };

            _context.TransferRequests.Add(request);
            await _context.SaveChangesAsync();
            return request.Id;
        }

        public async Task UpdateStatusAsync(int id, TransferStatus status)
        {
            var request = await _context.TransferRequests.FindAsync(id);
            if (request != null)
            {
                request.Status = status;
                if (status == TransferStatus.Completed)
                    request.CompletedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }
    }
}