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
            var approvedApplications = await _context.JobApplications
                .Include(a => a.Company)
                .Where(a => a.StudentId == userId && a.Status == ApplicationStatus.SchoolApproved)
                .ToListAsync();

            if (!approvedApplications.Any())
            {
                TempData["Error"] = "Günlük yazabilmek için okul onaylı aktif bir stajınız olmalıdır.";
                return RedirectToAction(nameof(Index));
            }

            var vm = new DailyLogCreateViewModel
            {
                LogDate = DateTime.Today,
                Applications = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                    approvedApplications.Select(a => new { a.Id, DisplayName = $"{a.Company?.Name} - {a.Position}" }), 
                    "Id", "DisplayName")
            };
            return View(vm);
        }

        // POST: /DailyLog/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DailyLogCreateViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            if (vm.LogDate > DateTime.Today)
            {
                ModelState.AddModelError("LogDate", "Staj defteri tarihi bugünden ileri olamaz.");
                return View(vm);
            }

            var userId = await GetUserIdAsync();

            // Sadece SchoolApproved başvuruya günlük yazılabilir
            if (!await _context.JobApplications.AnyAsync(a =>
                a.Id == vm.JobApplicationId &&
                a.StudentId == userId &&
                a.Status == ApplicationStatus.SchoolApproved))
            {
                ModelState.AddModelError(nameof(vm.JobApplicationId), "Geçerli ve okul onaylı bir staj seçiniz.");
            }

            if (!ModelState.IsValid)
            {
                var apps = await _context.JobApplications
                    .Include(a => a.Company)
                    .Where(a => a.StudentId == userId && a.Status == ApplicationStatus.SchoolApproved)
                    .ToListAsync();
                vm.Applications = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                    apps.Select(a => new { a.Id, DisplayName = $"{a.Company?.Name} - {a.Position}" }), 
                    "Id", "DisplayName", vm.JobApplicationId);
                return View(vm);
            }

            // Aynı gün numarasında kayıt var mı?
            var exists = await _context.DailyLogs
                .AnyAsync(d => d.StudentId == userId && d.JobApplicationId == vm.JobApplicationId && d.DayNumber == vm.DayNumber);
            if (exists)
            {
                ModelState.AddModelError("DayNumber", $"{vm.DayNumber}. gün için zaten bir kayıt mevcut.");
                var apps = await _context.JobApplications
                    .Include(a => a.Company)
                    .Where(a => a.StudentId == userId && a.Status == ApplicationStatus.SchoolApproved)
                    .ToListAsync();
                vm.Applications = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                    apps.Select(a => new { a.Id, DisplayName = $"{a.Company?.Name} - {a.Position}" }), 
                    "Id", "DisplayName", vm.JobApplicationId);
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

            if (log.Status == DailyLogStatus.EmployerApproved || log.Status == DailyLogStatus.SchoolApproved)
            {
                TempData["Error"] = "Onaylanmış bir kaydı düzenleyemezsiniz.";
                return RedirectToAction(nameof(Index));
            }

            var applications = await _context.JobApplications
                .Include(a => a.Company)
                .Where(a => a.StudentId == userId && a.Status == ApplicationStatus.SchoolApproved)
                .ToListAsync();

            var vm = new DailyLogCreateViewModel
            {
                Id = log.Id,
                JobApplicationId = log.JobApplicationId,
                DayNumber = log.DayNumber,
                LogDate = log.LogDate,
                Content = log.Content,
                Applications = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                    applications.Select(a => new { a.Id, DisplayName = $"{a.Company?.Name} - {a.Position}" }), 
                    "Id", "DisplayName", log.JobApplicationId)
            };
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

            if (log.Status == DailyLogStatus.EmployerApproved || log.Status == DailyLogStatus.SchoolApproved)
            {
                TempData["Error"] = "Onaylanmış bir kaydı düzenleyemezsiniz.";
                return RedirectToAction(nameof(Index));
            }

            if (vm.LogDate > DateTime.Today)
                ModelState.AddModelError(nameof(vm.LogDate), "Staj defteri tarihi bugünden ileri olamaz.");

            var duplicateDay = await _context.DailyLogs
                .AnyAsync(d => d.StudentId == userId && d.JobApplicationId == vm.JobApplicationId &&
                               d.DayNumber == vm.DayNumber && d.Id != id);
            if (duplicateDay)
                ModelState.AddModelError(nameof(vm.DayNumber), $"{vm.DayNumber}. gün için zaten bir kayıt mevcut.");

            if (!ModelState.IsValid)
            {
                var apps = await _context.JobApplications
                    .Include(a => a.Company)
                    .Where(a => a.StudentId == userId && a.Status == ApplicationStatus.SchoolApproved)
                    .ToListAsync();
                vm.Applications = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                    apps.Select(a => new { a.Id, DisplayName = $"{a.Company?.Name} - {a.Position}" }), 
                    "Id", "DisplayName", vm.JobApplicationId);
                return View(vm);
            }

            log.JobApplicationId = vm.JobApplicationId;
            log.DayNumber = vm.DayNumber;
            log.LogDate = vm.LogDate;
            log.Content = vm.Content;
            log.Status = DailyLogStatus.Draft;

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

            if (log.Status != DailyLogStatus.Draft && log.Status != DailyLogStatus.EmployerRejected)
            {
                TempData["Error"] = "Sadece taslak veya reddedilmiş kayıtlar gönderilebilir.";
                return RedirectToAction(nameof(Index));
            }

            log.Status = DailyLogStatus.SentToEmployer;
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

            if (log.Status == DailyLogStatus.EmployerApproved || log.Status == DailyLogStatus.SchoolApproved)
            {
                TempData["Error"] = "Onaylanmış bir kaydı silemezsiniz.";
                return RedirectToAction(nameof(Index));
            }

            _context.DailyLogs.Remove(log);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Günlük kaydı silindi.";
            return RedirectToAction(nameof(Index));
        }
    }
}
