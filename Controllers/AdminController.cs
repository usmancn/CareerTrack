using CareerTrack.Data;
using CareerTrack.Models.Constants;
using CareerTrack.Models.Entities;
using CareerTrack.Models.Enums;
using CareerTrack.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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
                TotalUsers = await _context.Users.CountAsync(),



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
                k => k.Status switch
                {
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
            var companies = await _context.Companies
                .OrderBy(c => c.IsApproved)
                .ThenBy(c => c.Name)
                .Select(c => new UserCompanyOptionViewModel { Id = c.Id, Name = c.Name, IsApproved = c.IsApproved })
                .ToListAsync();
            var roleRows = await _context.UserRoles
                .Join(_context.Roles,
                    userRole => userRole.RoleId,
                    role => role.Id,
                    (userRole, role) => new { userRole.UserId, RoleName = role.Name })
                .ToListAsync();
            var rolesByUserId = roleRows
                .GroupBy(r => r.UserId)
                .ToDictionary(g => g.Key, g => g.Select(r => r.RoleName ?? string.Empty).ToHashSet());
            var pendingEmployerUserIds = await _context.UserClaims
                .Where(c => c.ClaimType == AppClaims.EmployerPendingApproval && c.ClaimValue == "true")
                .Select(c => c.UserId)
                .ToHashSetAsync();

            var vm = new UserRoleManagementViewModel
            {
                Companies = companies
            };
            foreach (var user in users)
            {
                rolesByUserId.TryGetValue(user.Id, out var roles);
                roles ??= new HashSet<string>();
                var selectedRole = AppRoles.All.FirstOrDefault(roles.Contains) ?? AppRoles.Student;

                vm.Users.Add(new UserRoleItemViewModel
                {
                    UserId = user.Id,
                    FullName = user.FullName,
                    Email = user.Email ?? string.Empty,
                    Department = user.Department,
                    CurrentRole = AppRoles.DisplayName(selectedRole),
                    SelectedRole = selectedRole,
                    CompanyId = user.CompanyId,
                    IsEmployerPendingApproval = pendingEmployerUserIds.Contains(user.Id),
                    IsCurrentUser = user.Id == currentUserId
                });
            }

            return View(vm);
        }

        // POST: /Admin/UpdateUserRole
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateUserRole(string userId, string role, int? companyId)
        {
            if (!AppRoles.All.Contains(role))
            {
                TempData["Error"] = "Geçersiz rol seçimi.";
                return RedirectToAction(nameof(Users));
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            if (role == AppRoles.Employer &&
                (!companyId.HasValue || !await _context.Companies.AnyAsync(c => c.Id == companyId.Value && c.IsApproved)))
            {
                TempData["Error"] = "İşveren rolü için onaylı bir şirket seçmelisiniz.";
                return RedirectToAction(nameof(Users));
            }

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

            if (currentRoles.Contains(AppRoles.Employer) &&
                (role != AppRoles.Employer || user.CompanyId != companyId))
            {
                var hasEmployerData = await _context.JobPostings.AnyAsync(p => p.EmployerId == user.Id) ||
                                      await _context.StudentTasks.AnyAsync(t => t.AssignedByEmployerId == user.Id);
                if (hasEmployerData)
                {
                    TempData["Error"] = "İlanı veya atanmış görevi bulunan bir işverenin rolü ya da şirketi değiştirilemez.";
                    return RedirectToAction(nameof(Users));
                }
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();

            if (!currentRoles.Contains(role))
            {
                var addResult = await _userManager.AddToRoleAsync(user, role);
                if (!addResult.Succeeded)
                {
                    await transaction.RollbackAsync();
                    TempData["Error"] = "Yeni rol atanırken hata oluştu.";
                    return RedirectToAction(nameof(Users));
                }
            }

            var rolesToRemove = currentRoles.Where(r => r != role).ToList();
            if (rolesToRemove.Any())
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                if (!removeResult.Succeeded)
                {
                    await transaction.RollbackAsync();
                    TempData["Error"] = "Mevcut roller kaldırılırken hata oluştu.";
                    return RedirectToAction(nameof(Users));
                }
            }

            user.CompanyId = role == AppRoles.Employer ? companyId : null;
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                await transaction.RollbackAsync();
                TempData["Error"] = "Kullanıcının şirket bağlantısı güncellenirken hata oluştu.";
                return RedirectToAction(nameof(Users));
            }

            if (!await RemoveEmployerPendingApprovalAsync(user))
            {
                await transaction.RollbackAsync();
                TempData["Error"] = "İşveren onay durumu güncellenirken hata oluştu.";
                return RedirectToAction(nameof(Users));
            }

            await transaction.CommitAsync();
            TempData["Success"] = $"{user.FullName} kullanıcısının rolü {AppRoles.DisplayName(role)} olarak güncellendi.";
            return RedirectToAction(nameof(Users));
        }

        // POST: /Admin/ApproveEmployer
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveEmployer(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            if (!await _userManager.IsInRoleAsync(user, AppRoles.Employer))
            {
                TempData["Error"] = "Yalnızca işveren kullanıcıları onaylanabilir.";
                return RedirectToAction(nameof(Users));
            }

            if (!user.CompanyId.HasValue)
            {
                TempData["Error"] = "İşvereni onaylamak için önce onaylı bir şirket atamalısınız.";
                return RedirectToAction(nameof(Users));
            }

            var company = await _context.Companies.FindAsync(user.CompanyId.Value);
            if (company == null)
            {
                TempData["Error"] = "İşverene bağlı şirket bulunamadı.";
                return RedirectToAction(nameof(Users));
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();

            if (!company.IsApproved)
                company.IsApproved = true;

            if (!await RemoveEmployerPendingApprovalAsync(user))
            {
                await transaction.RollbackAsync();
                TempData["Error"] = "İşveren onay durumu güncellenirken hata oluştu.";
                return RedirectToAction(nameof(Users));
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            TempData["Success"] = $"{user.FullName} işveren hesabı ve bağlı şirket onaylandı.";
            return RedirectToAction(nameof(Users));
        }

        // POST: /Admin/DeleteUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            if (user.Id == _userManager.GetUserId(User))
            {
                TempData["Error"] = "Kendi hesabınızı bu ekrandan silemezsiniz.";
                return RedirectToAction(nameof(Users));
            }

            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains(AppRoles.Admin))
            {
                var adminUsers = await _userManager.GetUsersInRoleAsync(AppRoles.Admin);
                if (adminUsers.Count <= 1)
                {
                    TempData["Error"] = "Sistemde en az bir Admin kalmalıdır.";
                    return RedirectToAction(nameof(Users));
                }
            }

            if (roles.Contains(AppRoles.Employer))
            {
                var hasEmployerData = await _context.JobPostings.AnyAsync(p => p.EmployerId == user.Id) ||
                                      await _context.StudentTasks.AnyAsync(t => t.AssignedByEmployerId == user.Id);
                if (hasEmployerData)
                {
                    TempData["Error"] = "İlanı veya atanmış görevi bulunan işveren silinemez.";
                    return RedirectToAction(nameof(Users));
                }
            }

            var removablePendingCompanyIds = await _context.Companies
                .Where(c => c.CreatedByUserId == user.Id &&
                            !c.IsApproved &&
                            !c.JobApplications.Any() &&
                            !c.JobPostings.Any())
                .Select(c => c.Id)
                .ToListAsync();

            await using var transaction = await _context.Database.BeginTransactionAsync();
            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                await transaction.RollbackAsync();
                TempData["Error"] = "Kullanıcı silinirken hata oluştu.";
                return RedirectToAction(nameof(Users));
            }

            if (removablePendingCompanyIds.Any())
            {
                var companies = await _context.Companies
                    .Where(c => removablePendingCompanyIds.Contains(c.Id))
                    .ToListAsync();
                _context.Companies.RemoveRange(companies);
                await _context.SaveChangesAsync();
            }

            await transaction.CommitAsync();
            TempData["Success"] = $"{user.FullName} kullanıcısı silindi.";
            return RedirectToAction(nameof(Users));
        }

        // GET: /Admin/CreateUser
        public async Task<IActionResult> CreateUser()
        {
            var vm = new AdminCreateUserViewModel();
            await PopulateCompanySelectListAsync(vm);
            return View(vm);
        }

        // POST: /Admin/CreateUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(AdminCreateUserViewModel vm)
        {
            if (!AppRoles.All.Contains(vm.Role))
                ModelState.AddModelError(nameof(vm.Role), "Geçersiz rol seçimi.");

            if (vm.Role == AppRoles.Employer &&
                (!vm.CompanyId.HasValue || !await _context.Companies.AnyAsync(c => c.Id == vm.CompanyId.Value && c.IsApproved)))
            {
                ModelState.AddModelError(nameof(vm.CompanyId), "İşveren rolü için onaylı bir şirket seçmelisiniz.");
            }

            if (!ModelState.IsValid)
            {
                await PopulateCompanySelectListAsync(vm);
                return View(vm);
            }

            var userExists = await _userManager.FindByEmailAsync(vm.Email);
            if (userExists != null)
            {
                ModelState.AddModelError("Email", "Bu e-posta adresine sahip bir kullanıcı zaten var.");
                await PopulateCompanySelectListAsync(vm);
                return View(vm);
            }

            var user = new ApplicationUser
            {
                UserName = vm.Email,
                Email = vm.Email,
                FullName = vm.FullName,
                Department = vm.Department,
                CompanyId = vm.Role == AppRoles.Employer ? vm.CompanyId : null
            };

            await using var transaction = await _context.Database.BeginTransactionAsync();
            var result = await _userManager.CreateAsync(user, vm.Password);
            if (result.Succeeded)
            {
                var roleResult = await _userManager.AddToRoleAsync(user, vm.Role);
                if (!roleResult.Succeeded)
                {
                    await transaction.RollbackAsync();
                    foreach (var error in roleResult.Errors)
                        ModelState.AddModelError(string.Empty, error.Description);
                    await PopulateCompanySelectListAsync(vm);
                    return View(vm);
                }

                // Add RequiresPasswordChange claim so they are forced to change their password
                var claimResult = await _userManager.AddClaimAsync(user, new Claim(AppClaims.RequiresPasswordChange, "true"));
                if (!claimResult.Succeeded)
                {
                    await transaction.RollbackAsync();
                    foreach (var error in claimResult.Errors)
                        ModelState.AddModelError(string.Empty, error.Description);
                    await PopulateCompanySelectListAsync(vm);
                    return View(vm);
                }

                await transaction.CommitAsync();
                TempData["Success"] = $"{vm.FullName} adlı kullanıcı başarıyla oluşturuldu ve {AppRoles.DisplayName(vm.Role)} rolü atandı.";
                return RedirectToAction(nameof(Users));
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                await transaction.RollbackAsync();
                await PopulateCompanySelectListAsync(vm);
                return View(vm);
            }
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

        // POST: /Admin/ApproveLog
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveLog(int id, string? adminNote, bool approve)
        {
            var log = await _context.DailyLogs.FindAsync(id);
            if (log == null) return NotFound();

            if (log.Status != DailyLogStatus.EmployerApproved)
            {
                TempData["Error"] = "Yalnızca işveren tarafından onaylanmış günlükler Admin tarafından değerlendirilebilir.";
                return RedirectToAction(nameof(DailyLogs));
            }

            if (!approve && string.IsNullOrWhiteSpace(adminNote))
            {
                TempData["Error"] = "Revize isteği için öğrenciye açıklayıcı bir not yazmalısınız.";
                return RedirectToAction(nameof(DailyLogs));
            }

            if (adminNote?.Trim().Length > 500)
            {
                TempData["Error"] = "Danışman notu en fazla 500 karakter olabilir.";
                return RedirectToAction(nameof(DailyLogs));
            }

            log.IsSchoolApproved = approve;
            log.Status = approve ? DailyLogStatus.SchoolApproved : DailyLogStatus.SchoolRejected;
            log.SchoolNote = string.IsNullOrWhiteSpace(adminNote) ? null : adminNote.Trim();

            await _context.SaveChangesAsync();
            TempData["Success"] = approve
                ? "Günlük Admin tarafından onaylandı."
                : "Günlük revize edilmesi için öğrenciye geri gönderildi.";
            return RedirectToAction(nameof(DailyLogs));
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
            if (!Enum.IsDefined(typeof(ApplicationStatus), status))
            {
                TempData["Error"] = "Geçersiz başvuru durumu seçildi.";
                return RedirectToAction(nameof(Applications));
            }

            var app = await _context.JobApplications.FindAsync(id);
            if (app == null) return NotFound();

            if (status == ApplicationStatus.SchoolRevision && app.InternshipPostingId.HasValue)
            {
                TempData["Error"] = "İlan başvuruları öğrenci tarafından düzenlenemediği için revizyon durumuna alınamaz.";
                return RedirectToAction(nameof(Applications));
            }

            app.Status = status;
            await _context.SaveChangesAsync();

            var statusText = status switch
            {
                ApplicationStatus.Pending => "Yeni Başvuru",
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
                .Include(c => c.JobPostings)
                .AsSplitQuery()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (company == null) return NotFound();

            if (company.JobApplications.Any() || company.JobPostings.Any())
            {
                TempData["Error"] = "Başvurusu veya ilanı bulunan şirket silinemez.";
                return RedirectToAction(nameof(Companies));
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();
            var linkedUsers = await _context.Users
                .Where(u => u.CompanyId == id)
                .ToListAsync();
            foreach (var linkedUser in linkedUsers)
            {
                linkedUser.CompanyId = null;
                if (await _userManager.IsInRoleAsync(linkedUser, AppRoles.Employer) &&
                    !await AddEmployerPendingApprovalIfMissingAsync(linkedUser))
                {
                    await transaction.RollbackAsync();
                    TempData["Error"] = "Bağlı işveren onay durumuna alınırken hata oluştu.";
                    return RedirectToAction(nameof(Companies));
                }
            }

            _context.Companies.Remove(company);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
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

            await using var transaction = await _context.Database.BeginTransactionAsync();
            if (approve)
            {
                company.IsApproved = true;
                var linkedUsers = await _context.Users
                    .Where(u => u.CompanyId == id)
                    .ToListAsync();
                foreach (var linkedUser in linkedUsers)
                {
                    if (await _userManager.IsInRoleAsync(linkedUser, AppRoles.Employer) &&
                        !await RemoveEmployerPendingApprovalAsync(linkedUser))
                    {
                        await transaction.RollbackAsync();
                        TempData["Error"] = "Bağlı işveren onay durumu güncellenirken hata oluştu.";
                        return RedirectToAction(nameof(Companies));
                    }
                }

                TempData["Success"] = $"\"{company.Name}\" şirketi ve bağlı bekleyen işverenler onaylandı.";
            }
            else
            {
                // Kullanılan bir şirket önerisi silinemez
                var hasApplications = await _context.JobApplications.AnyAsync(a => a.CompanyId == id);
                var hasPostings = await _context.JobPostings.AnyAsync(p => p.CompanyId == id);
                if (hasApplications || hasPostings)
                {
                    TempData["Error"] = "Bu şirkete bağlı başvuru veya ilan olduğu için silinemez.";
                    return RedirectToAction(nameof(Companies));
                }

                var linkedUsers = await _context.Users
                    .Where(u => u.CompanyId == id)
                    .ToListAsync();
                foreach (var linkedUser in linkedUsers)
                    linkedUser.CompanyId = null;

                _context.Companies.Remove(company);
                TempData["Success"] = $"\"{company.Name}\" şirket önerisi reddedildi ve silindi.";
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return RedirectToAction(nameof(Companies));
        }

        private async Task<CompanyManagementViewModel> BuildCompanyModelAsync(CompanyFormViewModel? form = null)
        {
            var companies = await _context.Companies
                .Include(c => c.JobApplications)
                .Include(c => c.JobPostings)
                .Include(c => c.CreatedBy)
                .AsSplitQuery()
                .OrderBy(c => c.IsApproved)
                .ThenBy(c => c.Name)
                .ToListAsync();
            var linkedEmployerCompanyIds = await _context.Users
                .Where(u => u.CompanyId.HasValue)
                .Select(u => u.CompanyId!.Value)
                .ToHashSetAsync();
            var pendingEmployerCompanyIds = await _context.UserClaims
                .Where(c => c.ClaimType == AppClaims.EmployerPendingApproval && c.ClaimValue == "true")
                .Join(_context.Users,
                    claim => claim.UserId,
                    user => user.Id,
                    (claim, user) => user.CompanyId)
                .Where(companyId => companyId.HasValue)
                .Select(companyId => companyId!.Value)
                .ToHashSetAsync();

            return new CompanyManagementViewModel
            {
                Form = form ?? new CompanyFormViewModel(),
                Companies = companies,
                LinkedEmployerCompanyIds = linkedEmployerCompanyIds,
                PendingEmployerCompanyIds = pendingEmployerCompanyIds
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

        private async Task PopulateCompanySelectListAsync(AdminCreateUserViewModel vm)
        {
            var companies = await _context.Companies
                .Where(c => c.IsApproved)
                .OrderBy(c => c.Name)
                .ToListAsync();

            vm.Companies = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                companies, "Id", "Name", vm.CompanyId);
        }

        private async Task<bool> RemoveEmployerPendingApprovalAsync(ApplicationUser user)
        {
            var claims = await _userManager.GetClaimsAsync(user);
            var pendingClaims = claims.Where(c => c.Type == AppClaims.EmployerPendingApproval).ToList();
            foreach (var pendingClaim in pendingClaims)
            {
                var result = await _userManager.RemoveClaimAsync(user, pendingClaim);
                if (!result.Succeeded)
                    return false;
            }

            return true;
        }

        private async Task<bool> AddEmployerPendingApprovalIfMissingAsync(ApplicationUser user)
        {
            var claims = await _userManager.GetClaimsAsync(user);
            if (claims.Any(c => c.Type == AppClaims.EmployerPendingApproval && c.Value == "true"))
                return true;

            var result = await _userManager.AddClaimAsync(user, new Claim(AppClaims.EmployerPendingApproval, "true"));
            return result.Succeeded;
        }
    }
}
