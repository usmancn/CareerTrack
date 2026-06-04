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
    [Authorize(Roles = AppRoles.Student)]
    public class DailyLogController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DailyLogController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private async Task<string> GetUserIdAsync() =>
            (await _userManager.GetUserAsync(User))!.Id;

        private static bool CanStudentModify(DailyLogStatus status) =>
            status == DailyLogStatus.Draft ||
            status == DailyLogStatus.EmployerRejected ||
            status == DailyLogStatus.SchoolRejected;

        private Task<JobApplication?> GetOwnedApprovedApplicationAsync(int applicationId, string userId) =>
            _context.JobApplications.FirstOrDefaultAsync(a =>
                a.Id == applicationId &&
                a.StudentId == userId &&
                a.Status == ApplicationStatus.SchoolApproved);

        private void ValidateLogAgainstApplication(DailyLogCreateViewModel vm, JobApplication application)
        {
            if (application.InternshipStartDate.HasValue &&
                vm.LogDate.Date < application.InternshipStartDate.Value.Date)
            {
                ModelState.AddModelError(nameof(vm.LogDate), "Günlük tarihi staj başlangıç tarihinden önce olamaz.");
            }

            if (application.InternshipEndDate.HasValue &&
                vm.LogDate.Date > application.InternshipEndDate.Value.Date)
            {
                ModelState.AddModelError(nameof(vm.LogDate), "Günlük tarihi staj bitiş tarihinden sonra olamaz.");
            }

            if (application.TotalInternshipDays.HasValue &&
                vm.DayNumber > application.TotalInternshipDays.Value)
            {
                ModelState.AddModelError(nameof(vm.DayNumber), "Gün numarası tanımlı toplam staj gününü aşamaz.");
            }
        }

        private async Task PopulateApplicationsSelectListAsync(DailyLogCreateViewModel vm, string userId, int? selectedId = null)
        {
            var approvedApplications = await _context.JobApplications
                .Include(a => a.Company)
                .Where(a => a.StudentId == userId && a.Status == ApplicationStatus.SchoolApproved)
                .ToListAsync();

            vm.Applications = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                approvedApplications.Select(a => new { a.Id, DisplayName = $"{a.Company?.Name} - {a.Position}" }),
                "Id", "DisplayName", selectedId);
        }

        // GET: /DailyLog
        public async Task<IActionResult> Index()
        {
            var userId = await GetUserIdAsync();
            var logs = await _context.DailyLogs
                .Include(d => d.JobApplication)
                    .ThenInclude(a => a!.Company)
                .Where(d => d.StudentId == userId)
                .OrderByDescending(d => d.DayNumber)
                .ToListAsync();

            ViewBag.PendingTodos = await _context.ToDos
                .CountAsync(t => t.StudentId == userId && !t.IsCompleted);

            // Okul onaylı aktif stajlar (günlük yazılabilecek başvurular)
            ViewBag.ApprovedApplications = await _context.JobApplications
                .Include(a => a.Company)
                .Where(a => a.StudentId == userId && a.Status == ApplicationStatus.SchoolApproved)
                .ToListAsync();

            return View(logs);
        }

        // GET: /DailyLog/Create
        public async Task<IActionResult> Create()
        {
            var userId = await GetUserIdAsync();

            // Sadece okul onaylı stajlara günlük yazılabilir
            if (!await _context.JobApplications.AnyAsync(a =>
                a.StudentId == userId && a.Status == ApplicationStatus.SchoolApproved))
            {
                TempData["Error"] = "Günlük yazabilmek için okul onaylı aktif bir stajınız olmalıdır.";
                return RedirectToAction(nameof(Index));
            }

            var vm = new DailyLogCreateViewModel
            {
                LogDate = DateTime.Today
            };
            await PopulateApplicationsSelectListAsync(vm, userId);
            return View(vm);
        }

        // POST: /DailyLog/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DailyLogCreateViewModel vm)
        {
            var userId = await GetUserIdAsync();

            if (vm.LogDate > DateTime.Today)
            {
                ModelState.AddModelError("LogDate", "Staj defteri tarihi bugünden ileri olamaz.");
            }

            // Sadece SchoolApproved başvuruya günlük yazılabilir
            var application = await GetOwnedApprovedApplicationAsync(vm.JobApplicationId, userId);
            if (application == null)
            {
                ModelState.AddModelError(nameof(vm.JobApplicationId), "Geçerli ve okul onaylı bir staj seçiniz.");
            }
            else
            {
                ValidateLogAgainstApplication(vm, application);
            }

            // Aynı gün numarasında kayıt var mı?
            if (ModelState.IsValid)
            {
                var exists = await _context.DailyLogs
                    .AnyAsync(d => d.StudentId == userId && d.JobApplicationId == vm.JobApplicationId && d.DayNumber == vm.DayNumber);
                if (exists)
                {
                    ModelState.AddModelError("DayNumber", $"{vm.DayNumber}. gün için zaten bir kayıt mevcut.");
                }
            }

            if (!ModelState.IsValid)
            {
                await PopulateApplicationsSelectListAsync(vm, userId, vm.JobApplicationId);
                return View(vm);
            }

            var log = new DailyLog
            {
                StudentId = userId,
                JobApplicationId = vm.JobApplicationId,
                DayNumber = vm.DayNumber,
                LogDate = vm.LogDate,
                Content = vm.Content,
                Status = DailyLogStatus.Draft
            };

            _context.DailyLogs.Add(log);
            await _context.SaveChangesAsync();
            TempData["Success"] = $"{vm.DayNumber}. gün kaydı başarıyla eklendi!";
            return RedirectToAction(nameof(Index));
        }

        // GET: /DailyLog/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var userId = await GetUserIdAsync();
            var log = await _context.DailyLogs
                .FirstOrDefaultAsync(d => d.Id == id && d.StudentId == userId);

            if (log == null) return NotFound();

            if (!CanStudentModify(log.Status))
            {
                TempData["Error"] = "Onay sürecindeki veya onaylanmış bir kaydı düzenleyemezsiniz.";
                return RedirectToAction(nameof(Index));
            }

            var vm = new DailyLogCreateViewModel
            {
                Id = log.Id,
                JobApplicationId = log.JobApplicationId,
                DayNumber = log.DayNumber,
                LogDate = log.LogDate,
                Content = log.Content
            };
            await PopulateApplicationsSelectListAsync(vm, userId, log.JobApplicationId);
            return View(vm);
        }

        // POST: /DailyLog/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DailyLogCreateViewModel vm)
        {
            var userId = await GetUserIdAsync();
            var log = await _context.DailyLogs
                .FirstOrDefaultAsync(d => d.Id == id && d.StudentId == userId);

            if (log == null) return NotFound();

            if (!CanStudentModify(log.Status))
            {
                TempData["Error"] = "Onay sürecindeki veya onaylanmış bir kaydı düzenleyemezsiniz.";
                return RedirectToAction(nameof(Index));
            }

            if (vm.LogDate > DateTime.Today)
            {
                ModelState.AddModelError(nameof(vm.LogDate), "Staj defteri tarihi bugünden ileri olamaz.");
            }

            var application = await GetOwnedApprovedApplicationAsync(vm.JobApplicationId, userId);
            if (application == null)
            {
                ModelState.AddModelError(nameof(vm.JobApplicationId), "Geçerli ve okul onaylı bir staj seçiniz.");
            }
            else
            {
                ValidateLogAgainstApplication(vm, application);
            }

            var duplicateDay = await _context.DailyLogs
                .AnyAsync(d => d.StudentId == userId && d.JobApplicationId == vm.JobApplicationId &&
                               d.DayNumber == vm.DayNumber && d.Id != id);
            if (duplicateDay)
            {
                ModelState.AddModelError(nameof(vm.DayNumber), $"{vm.DayNumber}. gün için zaten bir kayıt mevcut.");
            }

            if (!ModelState.IsValid)
            {
                vm.Id = id;
                await PopulateApplicationsSelectListAsync(vm, userId, vm.JobApplicationId);
                return View(vm);
            }

            log.JobApplicationId = vm.JobApplicationId;
            log.DayNumber = vm.DayNumber;
            log.LogDate = vm.LogDate;
            log.Content = vm.Content;
            log.Status = DailyLogStatus.Draft;
            log.IsEmployerApproved = false;
            log.IsSchoolApproved = false;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Günlük kaydı güncellendi!";
            return RedirectToAction(nameof(Index));
        }

        // POST: /DailyLog/SendToEmployer/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendToEmployer(int id)
        {
            var userId = await GetUserIdAsync();
            var log = await _context.DailyLogs
                .FirstOrDefaultAsync(d => d.Id == id && d.StudentId == userId);

            if (log == null) return NotFound();

            if (!CanStudentModify(log.Status))
            {
                TempData["Error"] = "Sadece taslak veya revize istenen kayıtlar gönderilebilir.";
                return RedirectToAction(nameof(Index));
            }

            if (await GetOwnedApprovedApplicationAsync(log.JobApplicationId, userId) == null)
            {
                TempData["Error"] = "Bu günlük artık aktif ve okul onaylı bir staja bağlı değil.";
                return RedirectToAction(nameof(Index));
            }

            log.Status = DailyLogStatus.SentToEmployer;
            log.IsEmployerApproved = false;
            log.IsSchoolApproved = false;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Günlük işverene onay için gönderildi.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /DailyLog/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = await GetUserIdAsync();
            var log = await _context.DailyLogs
                .FirstOrDefaultAsync(d => d.Id == id && d.StudentId == userId);

            if (log == null) return NotFound();

            if (!CanStudentModify(log.Status))
            {
                TempData["Error"] = "Onay sürecindeki veya onaylanmış bir kaydı silemezsiniz.";
                return RedirectToAction(nameof(Index));
            }

            _context.DailyLogs.Remove(log);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Günlük kaydı silindi.";
            return RedirectToAction(nameof(Index));
        }
    }
}
