using CareerTrack.Models.Constants;
using CareerTrack.Data;
using CareerTrack.Models.Entities;
using CareerTrack.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CareerTrack.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;

        public AccountController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Dashboard");
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid) return View(model);

            var result = await _signInManager.PasswordSignInAsync(
                model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    var claims = await _userManager.GetClaimsAsync(user);
                    if (claims.Any(c => c.Type == AppClaims.RequiresPasswordChange && c.Value == "true"))
                    {
                        return RedirectToAction("Settings", "Profile");
                    }

                    if (claims.Any(c => c.Type == AppClaims.EmployerPendingApproval && c.Value == "true"))
                    {
                        return RedirectToAction(nameof(PendingApproval));
                    }
                }
                var roles = await _userManager.GetRolesAsync(user!);
                if (roles.Contains(AppRoles.Admin))
                    return RedirectToAction("Index", "Admin");
                if (roles.Contains(AppRoles.School))
                    return RedirectToAction("Index", "School");
                if (roles.Contains(AppRoles.Employer))
                    return RedirectToAction("Index", "Employer");
                return RedirectToLocal(returnUrl);
            }

            ModelState.AddModelError(string.Empty, "E-posta veya şifre hatalı.");
            return View(model);
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Dashboard");

            var model = new RegisterViewModel();
            PopulateRegisterCompanies(model);
            return View(model);
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            ValidateRegisterModel(model);
            if (!ModelState.IsValid)
            {
                PopulateRegisterCompanies(model);
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName.Trim(),
                Department = string.IsNullOrWhiteSpace(model.Department) ? null : model.Department.Trim(),
                EmailConfirmed = true
            };

            await using var transaction = await _context.Database.BeginTransactionAsync();
            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                var role = model.Role == AppRoles.Employer ? AppRoles.Employer : AppRoles.Student;
                var roleResult = await _userManager.AddToRoleAsync(user, role);
                if (!roleResult.Succeeded)
                {
                    await transaction.RollbackAsync();
                    foreach (var error in roleResult.Errors)
                        ModelState.AddModelError(string.Empty, error.Description);
                    PopulateRegisterCompanies(model);
                    return View(model);
                }

                if (role == AppRoles.Employer)
                {
                    if (model.EmployerCompanyMode == "New")
                    {
                        var company = new Company
                        {
                            Name = model.CompanyName!.Trim(),
                            Sector = model.CompanySector!.Trim(),
                            Location = model.CompanyLocation!.Trim(),
                            CreatedByUserId = user.Id,
                            IsApproved = false
                        };
                        _context.Companies.Add(company);
                        await _context.SaveChangesAsync();
                        user.CompanyId = company.Id;
                    }
                    else
                    {
                        user.CompanyId = model.CompanyId;
                    }

                    var updateResult = await _userManager.UpdateAsync(user);
                    if (!updateResult.Succeeded)
                    {
                        await transaction.RollbackAsync();
                        foreach (var error in updateResult.Errors)
                            ModelState.AddModelError(string.Empty, error.Description);
                        PopulateRegisterCompanies(model);
                        return View(model);
                    }

                    var claimResult = await _userManager.AddClaimAsync(
                        user,
                        new Claim(AppClaims.EmployerPendingApproval, "true"));
                    if (!claimResult.Succeeded)
                    {
                        await transaction.RollbackAsync();
                        foreach (var error in claimResult.Errors)
                            ModelState.AddModelError(string.Empty, error.Description);
                        PopulateRegisterCompanies(model);
                        return View(model);
                    }
                }

                await transaction.CommitAsync();
                await _signInManager.SignInAsync(user, isPersistent: false);

                if (role == AppRoles.Employer)
                    return RedirectToAction(nameof(PendingApproval));

                return RedirectToAction("Index", "Dashboard");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            await transaction.RollbackAsync();
            PopulateRegisterCompanies(model);
            return View(model);
        }

        [Authorize]
        public async Task<IActionResult> PendingApproval()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction(nameof(Login));

            var claims = await _userManager.GetClaimsAsync(user);
            if (!claims.Any(c => c.Type == AppClaims.EmployerPendingApproval && c.Value == "true"))
            {
                await _signInManager.RefreshSignInAsync(user);
                return RedirectToAction("Index", "Dashboard");
            }

            return View();
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        // GET: /Account/AccessDenied
        public IActionResult AccessDenied() => View();

        private void PopulateRegisterCompanies(RegisterViewModel model)
        {
            var companies = _context.Companies
                .Where(c => c.IsApproved)
                .OrderBy(c => c.Name)
                .Select(c => new { c.Id, c.Name })
                .ToList();

            model.Companies = new SelectList(companies, "Id", "Name", model.CompanyId);
        }

        private void ValidateRegisterModel(RegisterViewModel model)
        {
            if (model.Role != AppRoles.Student && model.Role != AppRoles.Employer)
            {
                ModelState.AddModelError(nameof(model.Role), "Geçersiz hesap türü seçildi.");
                return;
            }

            if (model.Role != AppRoles.Employer)
                return;

            if (model.EmployerCompanyMode != "Existing" && model.EmployerCompanyMode != "New")
            {
                ModelState.AddModelError(nameof(model.EmployerCompanyMode), "Geçersiz şirket seçimi.");
                return;
            }

            if (model.EmployerCompanyMode == "Existing")
            {
                if (!model.CompanyId.HasValue ||
                    !_context.Companies.Any(c => c.Id == model.CompanyId.Value && c.IsApproved))
                {
                    ModelState.AddModelError(nameof(model.CompanyId), "Onaylı bir şirket seçmelisiniz.");
                }
                return;
            }

            if (string.IsNullOrWhiteSpace(model.CompanyName))
                ModelState.AddModelError(nameof(model.CompanyName), "Şirket adı zorunludur.");
            if (string.IsNullOrWhiteSpace(model.CompanySector))
                ModelState.AddModelError(nameof(model.CompanySector), "Sektör zorunludur.");
            if (string.IsNullOrWhiteSpace(model.CompanyLocation))
                ModelState.AddModelError(nameof(model.CompanyLocation), "Konum zorunludur.");

            if (!string.IsNullOrWhiteSpace(model.CompanyName))
            {
                var companyName = model.CompanyName.Trim().ToLower();
                if (_context.Companies.Any(c => c.Name.ToLower() == companyName))
                    ModelState.AddModelError(nameof(model.CompanyName), "Bu isimde bir şirket zaten mevcut.");
            }
        }

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction("Index", "Dashboard");
        }
    }
}
