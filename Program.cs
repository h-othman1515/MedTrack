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
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Server=(localdb)\\MSSQLLocalDB;Database=Medtrak_DB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
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
    options.ExpireTimeSpan = TimeSpan.FromDays(14);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
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
    options.Cookie.SameSite = SameSiteMode.Lax;
});

builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IAlertService, AlertService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<ISurplusService, SurplusService>();
builder.Services.AddScoped<ITransferService, TransferService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IReportsService, ReportsService>();

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
app.UseMiddleware<AppSessionMiddleware>();
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

        context.Database.Migrate();

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