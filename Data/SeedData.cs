using Microsoft.AspNetCore.Identity;
using MedTrackJordan.Models;

namespace MedTrackJordan.Data
{
    public static class SeedData
    {
        // Roles
        public const string Role_Admin = "Admin";
        public const string Role_PharmacyMgr = "PharmacyManager";
        public const string Role_Staff = "PharmacyStaff";
        public const string Role_Distributor = "Distributor";
        public const string Role_MOH = "MOHAdmin";

        public static async Task InitializeAsync(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            // ── Create roles ─────────────────────────────────────────────
            string[] roles =
            [
                Role_Admin,
                Role_PharmacyMgr,
                Role_Staff,
                Role_Distributor,
                Role_MOH
            ];

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // ── Create default admin ──────────────────────────────────────
            const string adminEmail = "admin@medtrack.jo";
            const string adminPassword = "Admin@12345";

            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "System Administrator",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(admin, adminPassword);
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(admin, Role_Admin);
            }
        }
    }
}