using MedTrack.Models;
using Microsoft.AspNetCore.Identity;
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

            var pharmacy = context.Pharmacies.OrderBy(p => p.Id).FirstOrDefault();
            if (pharmacy != null)
            {
                await EnsureUserAsync(userManager, logger,
                    email: "pharmacy@medtrack.jo",
                    password: "Pharm123!",
                    fullName: "Al-Shifa Manager",
                    role: "Pharmacy Manager",
                    pharmacyId: pharmacy.Id);
            }
            else
            {
                logger.LogWarning("No pharmacy found — pharmacy@medtrack.jo was not seeded.");
            }

            await EnsureUserAsync(userManager, logger,
                email: "distributor@medtrack.jo",
                password: "Dist123!",
                fullName: "Jordan Pharma Distributor",
                role: "Distributor");
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
