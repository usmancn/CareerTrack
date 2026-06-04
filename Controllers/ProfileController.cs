using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CareerTrack.Data;
using CareerTrack.Models.Entities;
using CareerTrack.Models.ViewModels;

namespace CareerTrack.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;

        public ProfileController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        // GET: /Profile/Settings
        public async Task<IActionResult> Settings()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var vm = new ProfileSettingsViewModel
            {
                FullName = user.FullName,
                Department = user.Department,
                Email = user.Email ?? string.Empty
            };

            var claims = await _userManager.GetClaimsAsync(user);
            if (claims.Any(c => c.Type == CareerTrack.Models.Constants.AppClaims.RequiresPasswordChange && c.Value == "true"))
            {
                ViewBag.RequiresPasswordChange = true;
                TempData["Warning"] = "Devam etmeden önce lütfen geçici şifrenizi değiştirin.";
            }

            return View(vm);
        }

        // POST: /Profile/Settings
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Settings(ProfileSettingsViewModel vm)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var claims = await _userManager.GetClaimsAsync(user);
            bool requiresPasswordChange = claims.Any(c => c.Type == CareerTrack.Models.Constants.AppClaims.RequiresPasswordChange && c.Value == "true");

            if (!ModelState.IsValid)
            {
                if (requiresPasswordChange) ViewBag.RequiresPasswordChange = true;
                return View(vm);
            }

            // Eğer şifre zorunlu değişecekse ve şifre girmemişse hata ver
            if (requiresPasswordChange && string.IsNullOrEmpty(vm.NewPassword))
            {
                ModelState.AddModelError(string.Empty, "Lütfen geçici şifrenizi yenisiyle değiştirin.");
                ViewBag.RequiresPasswordChange = true;
                return View(vm);
            }

            if (!string.IsNullOrEmpty(vm.OldPassword) && string.IsNullOrEmpty(vm.NewPassword))
            {
                ModelState.AddModelError(nameof(vm.NewPassword), "Yeni şifrenizi girmelisiniz.");
                if (requiresPasswordChange) ViewBag.RequiresPasswordChange = true;
                return View(vm);
            }

            if (!string.IsNullOrEmpty(vm.NewPassword))
            {
                if (string.IsNullOrEmpty(vm.OldPassword))
                {
                    ModelState.AddModelError(nameof(vm.OldPassword), "Mevcut şifrenizi girmelisiniz.");
                    if (requiresPasswordChange) ViewBag.RequiresPasswordChange = true;
                    return View(vm);
                }

                if (!await _userManager.CheckPasswordAsync(user, vm.OldPassword))
                {
                    ModelState.AddModelError(nameof(vm.OldPassword), "Mevcut şifreniz hatalı.");
                    if (requiresPasswordChange) ViewBag.RequiresPasswordChange = true;
                    return View(vm);
                }
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();

            // Update user info
            user.FullName = vm.FullName.Trim();
            user.Department = string.IsNullOrWhiteSpace(vm.Department) ? null : vm.Department.Trim();
            var updateResult = await _userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
            {
                foreach (var error in updateResult.Errors) ModelState.AddModelError(string.Empty, error.Description);
                if (requiresPasswordChange) ViewBag.RequiresPasswordChange = true;
                await transaction.RollbackAsync();
                return View(vm);
            }

            // Update password if provided
            if (!string.IsNullOrEmpty(vm.NewPassword))
            {
                var passResult = await _userManager.ChangePasswordAsync(user, vm.OldPassword!, vm.NewPassword);
                if (!passResult.Succeeded)
                {
                    foreach (var error in passResult.Errors) ModelState.AddModelError(string.Empty, error.Description);
                    if (requiresPasswordChange) ViewBag.RequiresPasswordChange = true;
                    await transaction.RollbackAsync();
                    return View(vm);
                }

                // If password changed successfully, remove the claim
                if (requiresPasswordChange)
                {
                    var claim = claims.First(c => c.Type == CareerTrack.Models.Constants.AppClaims.RequiresPasswordChange);
                    var claimResult = await _userManager.RemoveClaimAsync(user, claim);
                    if (!claimResult.Succeeded)
                    {
                        foreach (var error in claimResult.Errors) ModelState.AddModelError(string.Empty, error.Description);
                        ViewBag.RequiresPasswordChange = true;
                        await transaction.RollbackAsync();
                        return View(vm);
                    }
                }

                await transaction.CommitAsync();

                // Re-sign in to refresh cookie
                await _signInManager.RefreshSignInAsync(user);
                TempData["Success"] = "Profiliniz ve şifreniz başarıyla güncellendi.";
                return RedirectToAction("Index", "Dashboard");
            }

            await transaction.CommitAsync();
            TempData["Success"] = "Profiliniz başarıyla güncellendi.";
            return RedirectToAction("Settings");
        }
    }
}
