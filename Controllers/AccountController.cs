using MedTrack.Data;
using MedTrack.Models;
using MedTrack.Models.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Collections.Generic;
using System.Security.Claims;
using ApplicationUser = MedTrack.Models.ApplicationUser;

namespace MedTrack.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public AccountController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _context = context;
        }

        public IActionResult Login() => View(new LoginViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var result = await _signInManager.PasswordSignInAsync(
                model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !user.IsActive)
            {
                await _signInManager.SignOutAsync();
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
                return View(model);
            }

            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Count == 0)
            {
                await _signInManager.SignOutAsync();
                ModelState.AddModelError(string.Empty, "Your account has no assigned role. Please contact an administrator.");
                return View(model);
            }

            var extraClaims = new List<Claim>();
            if (user.PharmacyId.HasValue)
                extraClaims.Add(new Claim("PharmacyId", user.PharmacyId.Value.ToString()));

            await _signInManager.SignOutAsync();
            await _signInManager.SignInWithClaimsAsync(user, model.RememberMe, extraClaims);

            return RedirectForRole(roles.FirstOrDefault());
        }

        private IActionResult RedirectForRole(string? role) => role switch
        {
            "System Admin" => RedirectToAction("Index", "Admin"),
            "MOH Admin" => RedirectToAction("Dashboard", "MOH"),
            "Distributor" => RedirectToAction("Index", "Distributor"),
            _ => RedirectToAction("Index", "Dashboard")
        };

        public IActionResult Register() => View(new RegisterViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            if (await _userManager.FindByEmailAsync(model.Email) != null)
            {
                ModelState.AddModelError(nameof(model.Email), "An account with this email already exists.");
                return View(model);
            }

            var role = model.Role switch
            {
                "Pharmacy Staff" => "Pharmacy Staff",
                "Distributor" => "Distributor",
                _ => "Pharmacy Manager"
            };

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                EmailConfirmed = true,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return View(model);
            }

            await _userManager.AddToRoleAsync(user, role);

            if (role is "Pharmacy Staff" or "Pharmacy Manager")
            {
                var pharmacy = new Pharmacy
                {
                    Name = model.PharmacyName ?? $"{model.FullName}'s Pharmacy",
                    LicenseNo = model.LicenseNo ?? $"MOH-PENDING-{DateTime.UtcNow:yyyyMMdd}",
                    Governorate = model.Governorate ?? "Amman",
                    Address = model.Address ?? "",
                    Phone = model.Phone ?? "",
                    ContactEmail = model.Email,
                    IsApproved = false,
                    IsActive = true
                };
                _context.Pharmacies.Add(pharmacy);
                await _context.SaveChangesAsync();
                user.PharmacyId = pharmacy.Id;
                await _userManager.UpdateAsync(user);
            }

            var claims = new List<Claim>();
            if (user.PharmacyId.HasValue)
                claims.Add(new Claim("PharmacyId", user.PharmacyId.Value.ToString()));

            await _signInManager.SignInWithClaimsAsync(user, isPersistent: false, claims);
            return RedirectForRole(role);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction(nameof(Login));

            var roles = await _userManager.GetRolesAsync(user);
            var model = new ProfileViewModel
            {
                FullName = user.FullName,
                Email = user.Email ?? "",
                Role = roles.FirstOrDefault() ?? "User"
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction(nameof(Login));

            user.FullName = model.FullName;
            await _userManager.UpdateAsync(user);
            TempData["Success"] = "Profile updated successfully.";
            return RedirectToAction(nameof(Profile));
        }
        public IActionResult Settings() => View();
        public IActionResult ForgotPassword() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ForgotPassword(string email)
        {
            TempData["Success"] = "If an account exists for that email, a reset link has been sent.";
            return RedirectToAction(nameof(Login));
        }

        public IActionResult ChangePassword() => View(new ChangePasswordViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction(nameof(Login));

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return View(model);
            }

            TempData["Success"] = "Password updated successfully.";
            return RedirectToAction(nameof(Profile));
        }

        public IActionResult AccessDenied() => View();
    }
}