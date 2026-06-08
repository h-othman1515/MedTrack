using MedTrack.Data;
using MedTrack.Services;
using MedTrack.Background;
using MedTrack.Middleware;
using MedTrack.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Data Source=medtrack.db"
    ));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 8;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("PharmacyStaff", policy => policy.RequireRole("Pharmacy Staff", "Pharmacy Manager", "System Admin"));
    options.AddPolicy("PharmacyManager", policy => policy.RequireRole("Pharmacy Manager", "System Admin"));
    options.AddPolicy("Distributor", policy => policy.RequireRole("Distributor", "System Admin"));
    options.AddPolicy("MOHAdmin", policy => policy.RequireRole("MOH Admin", "System Admin"));
    options.AddPolicy("SystemAdmin", policy => policy.RequireRole("System Admin"));
});

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IAlertService, AlertService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<ISurplusService, SurplusService>();
builder.Services.AddScoped<ITransferService, TransferService>();

builder.Services.AddHostedService<ExpiryAlertBackgroundService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

if (app.Environment.IsDevelopment())
{
    app.UseMiddleware<DevelopmentAuthMiddleware>();
}

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        context.Database.EnsureCreated();

        if (!context.Drugs.Any())
        {
            context.Drugs.AddRange(new List<MedTrack.Models.Drug>
            {
                new MedTrack.Models.Drug { GenericName = "Amoxicillin", BrandNames = "Amoxil, Trimox", Category = "Antibiotics", DefaultMinStockLevel = 20, DefaultUnit = "Capsules", RequiresPrescription = true, IsCritical = true },
                new MedTrack.Models.Drug { GenericName = "Metformin", BrandNames = "Glucophage, Fortamet", Category = "Diabetes", DefaultMinStockLevel = 50, DefaultUnit = "Tablets", RequiresPrescription = true, IsCritical = true },
                new MedTrack.Models.Drug { GenericName = "Paracetamol", BrandNames = "Panadol, Tylenol", Category = "Pain Relief", DefaultMinStockLevel = 100, DefaultUnit = "Tablets", RequiresPrescription = false, IsCritical = false },
                new MedTrack.Models.Drug { GenericName = "Atorvastatin", BrandNames = "Lipitor", Category = "Cardiovascular", DefaultMinStockLevel = 30, DefaultUnit = "Tablets", RequiresPrescription = true, IsCritical = true },
                new MedTrack.Models.Drug { GenericName = "Omeprazole", BrandNames = "Prilosec, Losec", Category = "Gastrointestinal", DefaultMinStockLevel = 25, DefaultUnit = "Capsules", RequiresPrescription = false, IsCritical = false },
                new MedTrack.Models.Drug { GenericName = "Insulin Glargine", BrandNames = "Lantus, Basaglar", Category = "Diabetes", DefaultMinStockLevel = 10, DefaultUnit = "Vials", RequiresPrescription = true, IsCritical = true },
                new MedTrack.Models.Drug { GenericName = "Salbutamol", BrandNames = "Ventolin, ProAir", Category = "Respiratory", DefaultMinStockLevel = 15, DefaultUnit = "Inhalers", RequiresPrescription = true, IsCritical = true },
                new MedTrack.Models.Drug { GenericName = "Vitamin D3", BrandNames = "D-Vita, Cal-D", Category = "Vitamins", DefaultMinStockLevel = 50, DefaultUnit = "Tablets", RequiresPrescription = false, IsCritical = false }
            });
            await context.SaveChangesAsync();
        }

        if (!context.Pharmacies.Any())
        {
            context.Pharmacies.AddRange(new List<MedTrack.Models.Pharmacy>
            {
                new MedTrack.Models.Pharmacy
                {
                    Name = "Al-Shifa Pharmacy",
                    LicenseNo = "MOH-2025-00120",
                    Governorate = "Amman",
                    Address = "123 Rainbow Street, Jabal Amman",
                    ContactEmail = "info@alshifa.jo",
                    Phone = "+962 6 123 4567",
                    Tier = PharmacyTier.Professional,
                    IsApproved = true,
                    IsActive = true
                },
                new MedTrack.Models.Pharmacy
                {
                    Name = "Zarqa Medical Pharmacy",
                    LicenseNo = "MOH-2025-00087",
                    Governorate = "Zarqa",
                    Address = "45 King Abdullah Street, Zarqa",
                    ContactEmail = "contact@zarqamed.jo",
                    Phone = "+962 5 987 6543",
                    Tier = PharmacyTier.Basic,
                    IsApproved = true,
                    IsActive = true
                }
            });
            await context.SaveChangesAsync();
        }

        if (!context.Pharmacies.Any(p => p.Name == "Zarqa Medical Pharmacy"))
        {
            context.Pharmacies.Add(new MedTrack.Models.Pharmacy
            {
                Name = "Zarqa Medical Pharmacy",
                LicenseNo = "MOH-2025-00087",
                Governorate = "Zarqa",
                Address = "45 King Abdullah Street, Zarqa",
                ContactEmail = "contact@zarqamed.jo",
                Phone = "+962 5 987 6543",
                Tier = PharmacyTier.Basic,
                IsApproved = true,
                IsActive = true
            });
            await context.SaveChangesAsync();
        }

        if (!context.MedicationBatches.Any())
        {
            var pharmacy = context.Pharmacies.OrderBy(p => p.Id).First();
            var drugs = context.Drugs.OrderBy(d => d.Id).Take(4).ToList();
            if (drugs.Count >= 4)
            {
                context.MedicationBatches.AddRange(
                    new MedTrack.Models.MedicationBatch
                    {
                        PharmacyId = pharmacy.Id,
                        DrugId = drugs[0].Id,
                        BatchNo = "BN20250601A",
                        Quantity = 150,
                        MinStockLevel = 20,
                        Unit = "Capsules",
                        ExpiryDate = DateTime.UtcNow.AddDays(25),
                        UnitPrice = 2.50m
                    },
                    new MedTrack.Models.MedicationBatch
                    {
                        PharmacyId = pharmacy.Id,
                        DrugId = drugs[1].Id,
                        BatchNo = "BN20250602B",
                        Quantity = 80,
                        MinStockLevel = 50,
                        Unit = "Tablets",
                        ExpiryDate = DateTime.UtcNow.AddDays(55),
                        UnitPrice = 1.80m
                    },
                    new MedTrack.Models.MedicationBatch
                    {
                        PharmacyId = pharmacy.Id,
                        DrugId = drugs[2].Id,
                        BatchNo = "BN20250603C",
                        Quantity = 200,
                        MinStockLevel = 100,
                        Unit = "Tablets",
                        ExpiryDate = DateTime.UtcNow.AddDays(120),
                        UnitPrice = 0.75m
                    }
                );
                await context.SaveChangesAsync();
            }
        }

        if (!context.SurplusPosts.Any())
        {
            var pharmacy = context.Pharmacies.OrderBy(p => p.Id).First();
            var drug = context.Drugs.OrderBy(d => d.Id).First();
            context.SurplusPosts.Add(new MedTrack.Models.SurplusPost
            {
                PharmacyId = pharmacy.Id,
                DrugId = drug.Id,
                Quantity = 40,
                ExpiryDate = DateTime.UtcNow.AddDays(90),
                Status = SurplusStatus.Available,
                Condition = "Sealed, stored at room temperature",
                Price = 1.50m
            });
            await context.SaveChangesAsync();
        }

        if (!context.TransferRequests.Any())
        {
            var surplus = context.SurplusPosts.OrderBy(s => s.Id).First();
            var requestingPharmacy = context.Pharmacies.OrderBy(p => p.Id).Skip(1).FirstOrDefault()
                ?? context.Pharmacies.OrderBy(p => p.Id).First();
            context.TransferRequests.Add(new MedTrack.Models.TransferRequest
            {
                SurplusPostId = surplus.Id,
                RequestingPharmacyId = requestingPharmacy.Id,
                Quantity = 20,
                Status = TransferStatus.Pending,
                Notes = "Needed for regular patient demand",
                RequestedAt = DateTime.UtcNow.AddDays(-2)
            });
            await context.SaveChangesAsync();
        }

        var logger = services.GetRequiredService<ILogger<Program>>();
        await DemoUserSeeder.SeedAsync(userManager, roleManager, context, logger);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

app.Run();