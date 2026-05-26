using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CareerTrack.Models.Entities;
using CareerTrack.Models.ViewModels;
using System.Security.Claims;

namespace CareerTrack.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public ProfileController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
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
            if (claims.Any(c => c.Type == "RequiresPasswordChange" && c.Value == "true"))
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
            bool requiresPasswordChange = claims.Any(c => c.Type == "RequiresPasswordChange" && c.Value == "true");

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

            // Update user info
            user.FullName = vm.FullName;
            user.Department = vm.Department;
            var updateResult = await _userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
            {
                foreach (var error in updateResult.Errors) ModelState.AddModelError(string.Empty, error.Description);
                if (requiresPasswordChange) ViewBag.RequiresPasswordChange = true;
                return View(vm);
            }

            // Update password if provided
            if (!string.IsNullOrEmpty(vm.NewPassword))
            {
                if (string.IsNullOrEmpty(vm.OldPassword))
                {
                    ModelState.AddModelError(string.Empty, "Mevcut şifrenizi girmelisiniz.");
                    if (requiresPasswordChange) ViewBag.RequiresPasswordChange = true;
                    return View(vm);
                }

                var passResult = await _userManager.ChangePasswordAsync(user, vm.OldPassword, vm.NewPassword);
                if (!passResult.Succeeded)
                {
                    foreach (var error in passResult.Errors) ModelState.AddModelError(string.Empty, error.Description);
                    if (requiresPasswordChange) ViewBag.RequiresPasswordChange = true;
                    return View(vm);
                }

                // If password changed successfully, remove the claim
                if (requiresPasswordChange)
                {
                    var claim = claims.First(c => c.Type == "RequiresPasswordChange");
                    await _userManager.RemoveClaimAsync(user, claim);
                }

                // Re-sign in to refresh cookie
                await _signInManager.RefreshSignInAsync(user);
                TempData["Success"] = "Profiliniz ve şifreniz başarıyla güncellendi.";
                return RedirectToAction("Index", "Home");
            }

            TempData["Success"] = "Profiliniz başarıyla güncellendi.";
            return RedirectToAction("Settings");
        }
    }
}
