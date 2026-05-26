using CareerTrack.Data;
using CareerTrack.Models.Constants;
using CareerTrack.Models.Entities;
using CareerTrack.Models.Enums;
using CareerTrack.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CareerTrack.Controllers
{
    [Authorize(Roles = AppRoles.Admin)]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Admin
        public async Task<IActionResult> Index()
        {
            var studentUsers = await _userManager.GetUsersInRoleAsync(AppRoles.Student);
            var employerUsers = await _userManager.GetUsersInRoleAsync(AppRoles.Employer);
            var schoolUsers = await _userManager.GetUsersInRoleAsync(AppRoles.School);

            var vm = new AdminDashboardViewModel
            {
                TotalStudents = studentUsers.Count,
                TotalEmployers = employerUsers.Count,
                TotalSchools = schoolUsers.Count,
                TotalUsers = studentUsers.Count + employerUsers.Count + schoolUsers.Count,



                TotalCompanies = await _context.Companies.CountAsync(),
                PendingCompanies = await _context.Companies.CountAsync(c => !c.IsApproved),

                TotalJobPostings = await _context.JobPostings.CountAsync(),
                ActiveJobPostings = await _context.JobPostings.CountAsync(jp => jp.IsActive),

                RecentDailyLogs = await _context.DailyLogs
                    .Include(d => d.Student)
                    .OrderByDescending(d => d.LogDate)
                    .Take(10)
                    .ToListAsync(),

                RecentApplications = await _context.JobApplications
                    .Include(a => a.Company)
                    .Include(a => a.Student)
                    .OrderByDescending(a => a.ApplicationDate)
                    .Take(10)
                    .ToListAsync(),

                PendingCompanyList = await _context.Companies
                    .Where(c => !c.IsApproved)
                    .OrderBy(c => c.Name)
                    .ToListAsync()
            };

            var rawStats = await _context.JobApplications
                .GroupBy(a => a.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            vm.ApplicationStatusStats = rawStats.ToDictionary(
                k => k.Status switch {
                    ApplicationStatus.SchoolPending => "Okul Onayı Bekliyor",
                    ApplicationStatus.SchoolRevision => "Revize İstendi",
                    ApplicationStatus.SchoolApproved => "Okul Onaylı",
                    ApplicationStatus.PreScreening => "Ön Eleme",
                    ApplicationStatus.AptitudeTest => "Yetenek Testi",
                    ApplicationStatus.LanguageTest => "Dil Sınavı",
                    ApplicationStatus.Interview => "Mülakat",
                    ApplicationStatus.EmployerAccepted => "Kabul Edildi",
                    ApplicationStatus.Rejected => "Reddedildi",
                    ApplicationStatus.Completed => "Tamamlandı",
                    _ => k.Status.ToString()
                },
                v => v.Count
            );

            return View(vm);
        }

        // GET: /Admin/Users
        public async Task<IActionResult> Users()
        {
            var currentUserId = _userManager.GetUserId(User);
            var users = await _context.Users
                .OrderBy(u => u.FullName)
                .ThenBy(u => u.Email)
                .ToListAsync();

            var vm = new UserRoleManagementViewModel();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var selectedRole = AppRoles.All.FirstOrDefault(roles.Contains) ?? AppRoles.Student;

                vm.Users.Add(new UserRoleItemViewModel
                {
                    UserId = user.Id,
                    FullName = user.FullName,
                    Email = user.Email ?? string.Empty,
                    Department = user.Department,
                    CurrentRole = AppRoles.DisplayName(selectedRole),
                    SelectedRole = selectedRole,
                    IsCurrentUser = user.Id == currentUserId
                });
            }

            return View(vm);
        }

        // POST: /Admin/UpdateUserRole
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateUserRole(string userId, string role)
        {
            if (!AppRoles.All.Contains(role))
            {
                TempData["Error"] = "Geçersiz rol seçimi.";
                return RedirectToAction(nameof(Users));
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var currentRoles = await _userManager.GetRolesAsync(user);
            if (user.Id == _userManager.GetUserId(User) && role != AppRoles.Admin)
            {
                TempData["Error"] = "Kendi Admin rolünüzü bu ekrandan kaldıramazsınız.";
                return RedirectToAction(nameof(Users));
            }

            if (currentRoles.Contains(AppRoles.Admin) && role != AppRoles.Admin)
            {
                var adminUsers = await _userManager.GetUsersInRoleAsync(AppRoles.Admin);
                if (adminUsers.Count <= 1)
                {
                    TempData["Error"] = "Sistemde en az bir Admin kalmalıdır.";
                    return RedirectToAction(nameof(Users));
                }
            }

            if (currentRoles.Any())
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                if (!removeResult.Succeeded)
                {
                    TempData["Error"] = "Mevcut roller kaldırılırken hata oluştu.";
                    return RedirectToAction(nameof(Users));
                }
            }

            var addResult = await _userManager.AddToRoleAsync(user, role);
            if (!addResult.Succeeded)
            {
                TempData["Error"] = "Yeni rol atanırken hata oluştu.";
                return RedirectToAction(nameof(Users));
            }

            TempData["Success"] = $"{user.FullName} kullanıcısının rolü {AppRoles.DisplayName(role)} olarak güncellendi.";
            return RedirectToAction(nameof(Users));
        }

        // GET: /Admin/CreateUser
        public IActionResult CreateUser()
        {
            return View(new AdminCreateUserViewModel());
        }

        // POST: /Admin/CreateUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(AdminCreateUserViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var userExists = await _userManager.FindByEmailAsync(vm.Email);
            if (userExists != null)
            {
                ModelState.AddModelError("Email", "Bu e-posta adresine sahip bir kullanıcı zaten var.");
                return View(vm);
            }

            var user = new ApplicationUser
            {
                UserName = vm.Email,
                Email = vm.Email,
                FullName = vm.FullName,
                Department = vm.Department
            };

            var result = await _userManager.CreateAsync(user, vm.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(vm);
            }

            // Assign role
            if (AppRoles.All.Contains(vm.Role))
            {
                await _userManager.AddToRoleAsync(user, vm.Role);
            }

            TempData["Success"] = $"{vm.FullName} adlı kullanıcı başarıyla oluşturuldu ve {AppRoles.DisplayName(vm.Role)} rolü atandı.";
            return RedirectToAction(nameof(Users));
        }

        // GET: /Admin/DailyLogs
        public async Task<IActionResult> DailyLogs()
        {
            var logs = await _context.DailyLogs
                .Include(d => d.Student)
                .OrderBy(d => d.Status)
                .ThenByDescending(d => d.LogDate)
                .ToListAsync();
            return View(logs);
        }

        // GET: /Admin/Applications
        public async Task<IActionResult> Applications()
        {
            var apps = await _context.JobApplications
                .Include(a => a.Company)
                .Include(a => a.Student)
                .OrderByDescending(a => a.ApplicationDate)
                .ToListAsync();
            return View(apps);
        }

        // POST: /Admin/UpdateApplicationStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateApplicationStatus(int id, ApplicationStatus status)
        {
            var app = await _context.JobApplications.FindAsync(id);
            if (app == null) return NotFound();

            app.Status = status;
            await _context.SaveChangesAsync();

            var statusText = status switch
            {
                ApplicationStatus.SchoolPending => "Okul Onayı Bekliyor",
                ApplicationStatus.SchoolRevision => "Okul Revize İstedi",
                ApplicationStatus.SchoolApproved => "Okul Onaylı",
                ApplicationStatus.PreScreening => "Ön Eleme",
                ApplicationStatus.AptitudeTest => "Genel Yetenek Testi",
                ApplicationStatus.LanguageTest => "İngilizce Sınavı",
                ApplicationStatus.Interview => "Mülakat",
                ApplicationStatus.EmployerAccepted => "Kabul Edildi",
                ApplicationStatus.Rejected => "Reddedildi",
                ApplicationStatus.Completed => "Tamamlandı",
                _ => status.ToString()
            };

            TempData["Success"] = $"Başvuru durumu '{statusText}' olarak güncellendi.";
            return RedirectToAction(nameof(Applications));
        }

        // GET: /Admin/Companies
        public async Task<IActionResult> Companies()
        {
            return View(await BuildCompanyModelAsync());
        }

        // POST: /Admin/AddCompany
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCompany([Bind(Prefix = "Form")] CompanyFormViewModel form)
        {
            ValidateCompanyForm(form, "Form.");
            if (!ModelState.IsValid)
                return View(nameof(Companies), await BuildCompanyModelAsync(form));

            var exists = await _context.Companies
                .AnyAsync(c => c.Name.ToLower() == form.Name.Trim().ToLower());
            if (exists)
            {
                ModelState.AddModelError("Form.Name", "Bu isimde bir şirket zaten mevcut.");
                return View(nameof(Companies), await BuildCompanyModelAsync(form));
            }

            _context.Companies.Add(new Company
            {
                Name = form.Name.Trim(),
                Sector = form.Sector.Trim(),
                Location = form.Location.Trim(),
                IsApproved = true
            });
            await _context.SaveChangesAsync();
            TempData["Success"] = "Şirket eklendi!";
            return RedirectToAction(nameof(Companies));
        }

        // POST: /Admin/EditCompany
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCompany(CompanyFormViewModel form)
        {
            ValidateCompanyForm(form);
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Şirket bilgileri geçerli değil. Lütfen alanları kontrol edin.";
                return RedirectToAction(nameof(Companies));
            }

            var company = await _context.Companies.FindAsync(form.Id);
            if (company == null) return NotFound();

            var duplicate = await _context.Companies
                .AnyAsync(c => c.Id != form.Id && c.Name.ToLower() == form.Name.Trim().ToLower());
            if (duplicate)
            {
                TempData["Error"] = "Bu isimde başka bir şirket zaten mevcut.";
                return RedirectToAction(nameof(Companies));
            }

            company.Name = form.Name.Trim();
            company.Sector = form.Sector.Trim();
            company.Location = form.Location.Trim();

            await _context.SaveChangesAsync();
            TempData["Success"] = "Şirket güncellendi!";
            return RedirectToAction(nameof(Companies));
        }

        // POST: /Admin/DeleteCompany
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCompany(int id)
        {
            var company = await _context.Companies
                .Include(c => c.JobApplications)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (company == null) return NotFound();

            if (company.JobApplications.Any())
            {
                TempData["Error"] = "Başvurusu bulunan şirket silinemez.";
                return RedirectToAction(nameof(Companies));
            }

            _context.Companies.Remove(company);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Şirket silindi.";
            return RedirectToAction(nameof(Companies));
        }

        // POST: /Admin/ApproveCompany/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveCompany(int id, bool approve)
        {
            var company = await _context.Companies.FindAsync(id);
            if (company == null) return NotFound();

            if (approve)
            {
                company.IsApproved = true;
                TempData["Success"] = $"\"{company.Name}\" şirketi onaylandı.";
            }
            else
            {
                // Başvurusu yoksa sil, varsa sadece reddet
                var hasApplications = await _context.JobApplications.AnyAsync(a => a.CompanyId == id);
                if (hasApplications)
                {
                    TempData["Error"] = "Bu şirkete bağlı başvurular olduğu için silinemez.";
                    return RedirectToAction(nameof(Companies));
                }
                _context.Companies.Remove(company);
                TempData["Success"] = $"\"{company.Name}\" şirket önerisi reddedildi ve silindi.";
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Companies));
        }

        private async Task<CompanyManagementViewModel> BuildCompanyModelAsync(CompanyFormViewModel? form = null)
        {
            var companies = await _context.Companies
                .Include(c => c.JobApplications)
                .Include(c => c.CreatedBy)
                .OrderBy(c => c.IsApproved)
                .ThenBy(c => c.Name)
                .ToListAsync();

            return new CompanyManagementViewModel
            {
                Form = form ?? new CompanyFormViewModel(),
                Companies = companies
            };
        }

        private void ValidateCompanyForm(CompanyFormViewModel form, string keyPrefix = "")
        {
            if (string.IsNullOrWhiteSpace(form.Name))
                ModelState.AddModelError($"{keyPrefix}{nameof(form.Name)}", "Şirket adı boş geçilemez!");
            if (string.IsNullOrWhiteSpace(form.Sector))
                ModelState.AddModelError($"{keyPrefix}{nameof(form.Sector)}", "Sektör boş geçilemez!");
            if (string.IsNullOrWhiteSpace(form.Location))
                ModelState.AddModelError($"{keyPrefix}{nameof(form.Location)}", "Konum boş geçilemez!");
        }
    }
}
