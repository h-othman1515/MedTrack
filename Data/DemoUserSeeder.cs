using MedTrack.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MedTrack.Data
{
    public static class DemoUserSeeder
    {
        public static async Task SeedAsync(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context,
            ILogger logger)
        {
            var roles = new[] { "Pharmacy Staff", "Pharmacy Manager", "Distributor", "MOH Admin", "System Admin" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            await EnsureUserAsync(userManager, logger,
                email: "admin@medtrack.jo",
                password: "Admin123!",
                fullName: "System Administrator",
                role: "System Admin");

            await EnsureUserAsync(userManager, logger,
                email: "moh@medtrack.jo",
                password: "Moh12345!",
                fullName: "MOH Administrator",
                role: "MOH Admin");

            var pharmacy = await EnsureDefaultPharmacyAsync(context);
            await EnsurePendingPharmacyAsync(context);
            await EnsureDemoRestockDataAsync(context, pharmacy, logger);
            await EnsureUserAsync(userManager, logger,
                email: "pharmacy@medtrack.jo",
                password: "Pharm123!",
                fullName: "Al-Shifa Manager",
                role: "Pharmacy Manager",
                pharmacyId: pharmacy.Id);

            await EnsureUserAsync(userManager, logger,
                email: "distributor@medtrack.jo",
                password: "Dist123!",
                fullName: "Jordan Pharma Distributor",
                role: "Distributor");
        }

        private static async Task<Pharmacy> EnsureDefaultPharmacyAsync(ApplicationDbContext context)
        {
            const string licenseNo = "MOH-2025-00120";

            var pharmacy = await context.Pharmacies
                .FirstOrDefaultAsync(p => p.LicenseNo == licenseNo);

            if (pharmacy != null)
                return pharmacy;

            pharmacy = await context.Pharmacies.OrderBy(p => p.Id).FirstOrDefaultAsync();
            if (pharmacy != null)
                return pharmacy;

            pharmacy = new Pharmacy
            {
                Name = "Al-Shifa Pharmacy",
                LicenseNo = licenseNo,
                Governorate = "Amman",
                Address = "123 Rainbow Street, Jabal Amman",
                ContactEmail = "info@alshifa.jo",
                Phone = "+962 6 123 4567",
                Tier = PharmacyTier.Professional,
                IsApproved = true,
                IsActive = true
            };

            context.Pharmacies.Add(pharmacy);
            await context.SaveChangesAsync();
            return pharmacy;
        }

        private static async Task EnsurePendingPharmacyAsync(ApplicationDbContext context)
        {
            const string licenseNo = "MOH-PENDING-DEMO";

            if (await context.Pharmacies.AnyAsync(p => p.LicenseNo == licenseNo))
                return;

            context.Pharmacies.Add(new Pharmacy
            {
                Name = "Bayt Al-Tibb Pharmacy",
                LicenseNo = licenseNo,
                Governorate = "Irbid",
                Address = "University Street, Irbid",
                ContactEmail = "pending@medtrack.jo",
                Phone = "+962 2 123 4567",
                Tier = PharmacyTier.Basic,
                IsApproved = false,
                IsActive = true
            });
            await context.SaveChangesAsync();
        }

        private static async Task EnsureDemoRestockDataAsync(
            ApplicationDbContext context,
            Pharmacy pharmacy,
            ILogger logger)
        {
            if (await context.RestockRequests.AnyAsync())
                return;

            var drugDefs = new (string Name, string Category)[]
            {
                ("Amoxicillin 500mg", "Antibiotics"),
                ("Metformin 850mg", "Diabetes"),
                ("Omeprazole 20mg", "Gastrointestinal"),
                ("Insulin Glargine", "Diabetes")
            };

            var drugs = new List<Drug>();
            foreach (var (name, category) in drugDefs)
            {
                var drug = await context.Drugs.FirstOrDefaultAsync(d => d.GenericName == name);
                if (drug == null)
                {
                    drug = new Drug
                    {
                        GenericName = name,
                        Category = category,
                        DefaultMinStockLevel = 20,
                        DefaultUnit = "Tablets",
                        IsActive = true
                    };
                    context.Drugs.Add(drug);
                    await context.SaveChangesAsync();
                }
                drugs.Add(drug);
            }

            var expiryBase = DateTime.UtcNow.AddMonths(8);
            for (var i = 0; i < drugs.Count; i++)
            {
                var drug = drugs[i];
                var exists = await context.MedicationBatches.AnyAsync(b =>
                    b.PharmacyId == pharmacy.Id && b.DrugId == drug.Id);
                if (!exists)
                {
                    context.MedicationBatches.Add(new MedicationBatch
                    {
                        PharmacyId = pharmacy.Id,
                        DrugId = drug.Id,
                        Quantity = 5 + i * 3,
                        MinStockLevel = 20,
                        BatchNo = $"DEMO-{drug.Id:D4}",
                        ExpiryDate = expiryBase.AddDays(i * 30),
                        Unit = "Tablets",
                        UnitPrice = 2.50m,
                        AddedAt = DateTime.UtcNow.AddDays(-14)
                    });
                }
            }
            await context.SaveChangesAsync();

            if (!await context.MedicationBatches.AnyAsync(b =>
                    b.PharmacyId == pharmacy.Id && b.ExpiryDate < DateTime.UtcNow))
            {
                var expiredDefs = new (string DrugName, int Qty, decimal Price, int DaysAgo)[]
                {
                    ("Amoxicillin 500mg", 12, 2.50m, 10),
                    ("Metformin 850mg", 8, 1.80m, 22),
                    ("Omeprazole 20mg", 15, 3.20m, 5)
                };

                foreach (var (drugName, qty, price, daysAgo) in expiredDefs)
                {
                    var drug = drugs.FirstOrDefault(d => d.GenericName == drugName)
                        ?? await context.Drugs.FirstOrDefaultAsync(d => d.GenericName == drugName);
                    if (drug == null) continue;

                    context.MedicationBatches.Add(new MedicationBatch
                    {
                        PharmacyId = pharmacy.Id,
                        DrugId = drug.Id,
                        Quantity = qty,
                        MinStockLevel = 20,
                        BatchNo = $"EXP-{drug.Id:D4}",
                        ExpiryDate = DateTime.UtcNow.AddDays(-daysAgo),
                        Unit = "Tablets",
                        UnitPrice = price,
                        AddedAt = DateTime.UtcNow.AddMonths(-3)
                    });
                }
                await context.SaveChangesAsync();
            }

            var batches = await context.MedicationBatches
                .Where(b => b.PharmacyId == pharmacy.Id && b.Quantity <= b.MinStockLevel)
                .OrderBy(b => b.DrugId)
                .ToListAsync();

            var statuses = new[]
            {
                RestockStatus.Pending,
                RestockStatus.Pending,
                RestockStatus.Confirmed,
                RestockStatus.Delivered
            };

            for (var i = 0; i < batches.Count && i < statuses.Length; i++)
            {
                var batch = batches[i];
                var status = statuses[i];
                context.RestockRequests.Add(new RestockRequest
                {
                    PharmacyId = pharmacy.Id,
                    DrugId = batch.DrugId,
                    RequestedQuantity = Math.Max(batch.MinStockLevel * 2 - batch.Quantity, batch.MinStockLevel),
                    Status = status,
                    CreatedAt = DateTime.UtcNow.AddDays(-i * 2),
                    ConfirmedAt = status != RestockStatus.Pending
                        ? DateTime.UtcNow.AddDays(-i)
                        : null
                });
            }

            await context.SaveChangesAsync();
            logger.LogInformation("Seeded demo inventory and restock requests for distributor portal.");
        }

        private static async Task EnsureUserAsync(
            UserManager<ApplicationUser> userManager,
            ILogger logger,
            string email,
            string password,
            string fullName,
            string role,
            int? pharmacyId = null)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FullName = fullName,
                    EmailConfirmed = true,
                    IsActive = true,
                    PharmacyId = pharmacyId
                };

                var create = await userManager.CreateAsync(user, password);
                if (!create.Succeeded)
                {
                    logger.LogError("Could not create {Email}: {Errors}",
                        email, string.Join("; ", create.Errors.Select(e => e.Description)));
                    return;
                }
            }
            else
            {
                user.IsActive = true;
                user.FullName = fullName;
                user.EmailConfirmed = true;
                if (pharmacyId.HasValue)
                    user.PharmacyId = pharmacyId;

                var update = await userManager.UpdateAsync(user);
                if (!update.Succeeded)
                {
                    logger.LogError("Could not update {Email}: {Errors}",
                        email, string.Join("; ", update.Errors.Select(e => e.Description)));
                    return;
                }

                // Keep demo passwords in sync for local development accounts.
                var hasPassword = await userManager.HasPasswordAsync(user);
                if (hasPassword)
                {
                    var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
                    var reset = await userManager.ResetPasswordAsync(user, resetToken, password);
                    if (!reset.Succeeded)
                    {
                        logger.LogWarning("Could not reset password for {Email}: {Errors}",
                            email, string.Join("; ", reset.Errors.Select(e => e.Description)));
                    }
                }
                else
                {
                    await userManager.AddPasswordAsync(user, password);
                }
            }

            user = await userManager.FindByEmailAsync(email);
            if (user == null) return;

            if (!await userManager.IsInRoleAsync(user, role))
            {
                var roleResult = await userManager.AddToRoleAsync(user, role);
                if (!roleResult.Succeeded)
                {
                    logger.LogError("Could not assign role {Role} to {Email}: {Errors}",
                        role, email, string.Join("; ", roleResult.Errors.Select(e => e.Description)));
                }
            }
        }
    }
}
