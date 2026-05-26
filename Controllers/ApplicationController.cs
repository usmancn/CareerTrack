using CareerTrack.Data;
using CareerTrack.Models.Constants;
using CareerTrack.Models.Entities;
using CareerTrack.Models.Enums;
using CareerTrack.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CareerTrack.Controllers
{
    [Authorize(Roles = AppRoles.Student)]
    public class ApplicationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ApplicationController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private async Task<string> GetUserIdAsync() =>
            (await _userManager.GetUserAsync(User))!.Id;

        // GET: /Application — Başvurularım + Açık İlanlar
        public async Task<IActionResult> Index()
        {
            var userId = await GetUserIdAsync();

            var applications = await _context.JobApplications
                .Include(a => a.Company)
                .Include(a => a.ToDos)
                .Where(a => a.StudentId == userId)
                .OrderByDescending(a => a.ApplicationDate)
                .ToListAsync();

            // Henüz başvurulmamış aktif ilanlar
            var appliedPostingIds = applications
                .Where(a => a.InternshipPostingId != null)
                .Select(a => a.InternshipPostingId!.Value)
                .ToList();

            var openPostings = await _context.JobPostings
                .Include(j => j.Company)
                .Include(j => j.Employer)
                .Where(j => j.IsActive && !appliedPostingIds.Contains(j.Id))
                .OrderByDescending(j => j.CreatedAt)
                .ToListAsync();

            var pendingTodosCount = await _context.ToDos
                .CountAsync(t => t.StudentId == userId && !t.IsCompleted);

            var model = new StudentApplicationIndexViewModel
            {
                MyApplications = applications,
                OpenPostings = openPostings,
                PendingTodosCount = pendingTodosCount
            };

            return View(model);
        }

        // POST: /Application/ApplyToPosting — Belirli bir ilana başvur
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApplyToPosting(int postingId)
        {
            var userId = await GetUserIdAsync();

            var posting = await _context.JobPostings
                .Include(jp => jp.Company)
                .FirstOrDefaultAsync(jp => jp.Id == postingId && jp.IsActive);

            if (posting == null)
            {
                TempData["Error"] = "İlan bulunamadı veya artık aktif değil.";
                return RedirectToAction(nameof(Index));
            }

            // Zaten başvurulmuş mu?
            var alreadyApplied = await _context.JobApplications
                .AnyAsync(a => a.StudentId == userId && a.InternshipPostingId == postingId);

            if (alreadyApplied)
            {
                TempData["Error"] = "Bu ilana zaten başvurdunuz.";
                return RedirectToAction(nameof(Index));
            }

            var application = new JobApplication
            {
                StudentId = userId,
                CompanyId = posting.CompanyId,
                InternshipPostingId = postingId,
                Position = posting.Title,
                ApplicationDate = DateTime.Today,
                InternshipType = posting.InternshipType,
                InternshipStartDate = posting.StartDate,
                InternshipEndDate = posting.EndDate,
                TotalInternshipDays = (int)(posting.EndDate - posting.StartDate).TotalDays,
                Status = ApplicationStatus.Pending  // İşveren inceleyecek
            };

            _context.JobApplications.Add(application);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"\"{posting.Title}\" ilanına başvurunuz alındı! İşveren inceleyecektir.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Application/Create — Manuel başvuru (ilan olmayan şirkete)
        public async Task<IActionResult> Create()
        {
            var userId = await GetUserIdAsync();
            var vm = new ApplicationCreateViewModel
            {
                ApplicationDate = DateTime.Today,
                Companies = await BuildCompanySelectListAsync(userId)
            };
            return View(vm);
        }

        // POST: /Application/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ApplicationCreateViewModel vm)
        {
            var userId = await GetUserIdAsync();

            if (!await _context.Companies.AnyAsync(c => c.Id == vm.CompanyId))
                ModelState.AddModelError(nameof(vm.CompanyId), "Geçerli bir şirket seçiniz.");

            if (vm.ApplicationDate > DateTime.Today)
                ModelState.AddModelError("ApplicationDate", "Başvuru tarihi bugünden ileri bir tarih olamaz.");

            if (!ModelState.IsValid)
            {
                vm.Companies = await BuildCompanySelectListAsync(userId, vm.CompanyId);
                return View(vm);
            }

            var application = new JobApplication
            {
                StudentId = userId,
                CompanyId = vm.CompanyId,
                Position = vm.Position,
                ApplicationDate = vm.ApplicationDate,
                InternshipType = vm.InternshipType,
                InternshipStartDate = vm.InternshipStartDate,
                InternshipEndDate = vm.InternshipEndDate,
                TotalInternshipDays = vm.TotalInternshipDays,
                Status = ApplicationStatus.Pending  // İşveren inceleyecek
            };

            _context.JobApplications.Add(application);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Başvuru başarıyla eklendi! İşveren inceleyecektir.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Application/SuggestCompany
        public IActionResult SuggestCompany()
        {
            return View(new CompanySuggestViewModel());
        }

        // POST: /Application/SuggestCompany
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SuggestCompany(CompanySuggestViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var userId = await GetUserIdAsync();

            var exists = await _context.Companies
                .AnyAsync(c => c.Name.ToLower() == vm.Name.Trim().ToLower());
            if (exists)
            {
                ModelState.AddModelError(nameof(vm.Name), "Bu isimde bir şirket zaten mevcut.");
                return View(vm);
            }

            var company = new Company
            {
                Name = vm.Name.Trim(),
                Sector = vm.Sector.Trim(),
                Location = vm.Location.Trim(),
                IsApproved = false,
                CreatedByUserId = userId
            };

            _context.Companies.Add(company);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"\"{company.Name}\" şirketi önerildi. Admin onayından sonra seçilebilecek.";
            return RedirectToAction(nameof(Create));
        }

        // GET: /Application/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var userId = await GetUserIdAsync();
            var app = await _context.JobApplications
                .FirstOrDefaultAsync(a => a.Id == id && a.StudentId == userId);

            if (app == null) return NotFound();

            // Sadece Pending veya SchoolRevision durumundaki başvurular düzenlenebilir
            if (app.Status != ApplicationStatus.Pending && app.Status != ApplicationStatus.SchoolRevision)
            {
                TempData["Error"] = "Sadece bekleyen veya revize istenen başvurular düzenlenebilir.";
                return RedirectToAction(nameof(Index));
            }

            var vm = new ApplicationCreateViewModel
            {
                Id = app.Id,
                CompanyId = app.CompanyId,
                Position = app.Position,
                ApplicationDate = app.ApplicationDate,
                InternshipType = app.InternshipType,
                Status = app.Status,
                InternshipStartDate = app.InternshipStartDate,
                InternshipEndDate = app.InternshipEndDate,
                TotalInternshipDays = app.TotalInternshipDays,
                Companies = await BuildCompanySelectListAsync(userId, app.CompanyId)
            };
            return View(vm);
        }

        // POST: /Application/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ApplicationCreateViewModel vm)
        {
            var userId = await GetUserIdAsync();
            var app = await _context.JobApplications
                .FirstOrDefaultAsync(a => a.Id == id && a.StudentId == userId);

            if (app == null) return NotFound();

            if (app.Status != ApplicationStatus.Pending && app.Status != ApplicationStatus.SchoolRevision)
            {
                TempData["Error"] = "Sadece bekleyen veya revize istenen başvurular düzenlenebilir.";
                return RedirectToAction(nameof(Index));
            }

            if (!await _context.Companies.AnyAsync(c => c.Id == vm.CompanyId))
                ModelState.AddModelError(nameof(vm.CompanyId), "Geçerli bir şirket seçiniz.");

            if (vm.ApplicationDate > DateTime.Today)
                ModelState.AddModelError(nameof(vm.ApplicationDate), "Başvuru tarihi bugünden ileri bir tarih olamaz.");

            if (!ModelState.IsValid)
            {
                vm.Companies = await BuildCompanySelectListAsync(userId, vm.CompanyId);
                return View(vm);
            }

            app.CompanyId = vm.CompanyId;
            app.Position = vm.Position;
            app.ApplicationDate = vm.ApplicationDate;
            app.InternshipType = vm.InternshipType;
            app.InternshipStartDate = vm.InternshipStartDate;
            app.InternshipEndDate = vm.InternshipEndDate;
            app.TotalInternshipDays = vm.TotalInternshipDays;
            // Revize sonrası yeniden işverene gönder
            if (app.Status == ApplicationStatus.SchoolRevision)
                app.Status = ApplicationStatus.Pending;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Başvuru güncellendi!";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Application/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var userId = await GetUserIdAsync();
            var app = await _context.JobApplications
                .Include(a => a.Company)
                .FirstOrDefaultAsync(a => a.Id == id && a.StudentId == userId);

            if (app == null) return NotFound();
            return View(app);
        }

        // POST: /Application/MarkStageComplete — "Öğrenci bunu tamamladım" der
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkStageComplete(int id)
        {
            var userId = await GetUserIdAsync();
            var app = await _context.JobApplications
                .FirstOrDefaultAsync(a => a.Id == id && a.StudentId == userId);

            if (app == null) return NotFound();

            // Sadece bu JobApplication'a ait olan son atanmış görevi (ToDo) bul ve tamamla
            var currentTodo = await _context.ToDos
                .Where(t => t.JobApplicationId == app.Id && t.StudentId == userId && !t.IsCompleted)
                .OrderByDescending(t => t.Id)
                .FirstOrDefaultAsync();

            if (currentTodo != null)
            {
                currentTodo.IsCompleted = true;
                await _context.SaveChangesAsync();
            }

            TempData["Success"] = "Tebrikler! Göreviniz tamamlandı olarak işaretlendi. İşveren değerlendirmesi bekleniyor.";
            return RedirectToAction(nameof(Index), new { tab = "myapps" });
        }

        // POST: /Application/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = await GetUserIdAsync();
            var app = await _context.JobApplications
                .FirstOrDefaultAsync(a => a.Id == id && a.StudentId == userId);

            if (app == null) return NotFound();

            if (app.Status != ApplicationStatus.Pending)
            {
                TempData["Error"] = "Sadece işveren tarafından henüz değerlendirilmemiş başvurular silinebilir.";
                return RedirectToAction(nameof(Index));
            }

            _context.JobApplications.Remove(app);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Başvuru silindi.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<SelectList> BuildCompanySelectListAsync(string userId, int? selectedCompanyId = null)
        {
            var companies = await _context.Companies
                .Where(c => c.IsApproved || c.CreatedByUserId == userId)
                .OrderBy(c => c.Name)
                .ToListAsync();

            var items = companies.Select(c => new
            {
                c.Id,
                DisplayName = c.IsApproved ? c.Name : $"{c.Name} (Onay Bekliyor)"
            });

            return new SelectList(items, "Id", "DisplayName", selectedCompanyId);
        }
    }
}
