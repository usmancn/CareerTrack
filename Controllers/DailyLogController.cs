using CareerTrack.Data;
using CareerTrack.Models.Entities;
using CareerTrack.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CareerTrack.Controllers
{
    [Authorize(Roles = "Student")]
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
                .Where(d => d.StudentId == userId)
                .OrderByDescending(d => d.DayNumber)
                .ToListAsync();

            ViewBag.PendingTodos = await _context.ToDos.CountAsync(t => t.StudentId == userId && !t.IsCompleted);
            return View(logs);
        }

        // GET: /DailyLog/Create
        public IActionResult Create()
        {
            var vm = new DailyLogCreateViewModel { LogDate = DateTime.Today };
            return View(vm);
        }

        // POST: /DailyLog/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DailyLogCreateViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            // Gelecek tarih girişini engelle
            if (vm.LogDate > DateTime.Today)
            {
                ModelState.AddModelError("LogDate", "Staj defteri tarihi bugünden ileri olamaz.");
                return View(vm);
            }

            var userId = await GetUserIdAsync();

            // Aynı gün numarasında kayıt var mı?
            var exists = await _context.DailyLogs
                .AnyAsync(d => d.StudentId == userId && d.DayNumber == vm.DayNumber);
            if (exists)
            {
                ModelState.AddModelError("DayNumber", $"{vm.DayNumber}. gün için zaten bir kayıt mevcut.");
                return View(vm);
            }

            var log = new DailyLog
            {
                StudentId = userId,
                DayNumber = vm.DayNumber,
                LogDate = vm.LogDate,
                Content = vm.Content
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

            var vm = new DailyLogCreateViewModel
            {
                Id = log.Id,
                DayNumber = log.DayNumber,
                LogDate = log.LogDate,
                Content = log.Content
            };
            return View(vm);
        }

        // POST: /DailyLog/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DailyLogCreateViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var userId = await GetUserIdAsync();
            var log = await _context.DailyLogs
                .FirstOrDefaultAsync(d => d.Id == id && d.StudentId == userId);

            if (log == null) return NotFound();

            // Onaylanmış kayıt düzenlenemez
            if (log.IsApprovedByAdmin)
            {
                TempData["Error"] = "Danışman tarafından onaylanmış bir kaydı düzenleyemezsiniz.";
                return RedirectToAction(nameof(Index));
            }

            log.DayNumber = vm.DayNumber;
            log.LogDate = vm.LogDate;
            log.Content = vm.Content;
            log.IsApprovedByAdmin = false; // Güncelleme sonrası onay sıfırlanır

            await _context.SaveChangesAsync();
            TempData["Success"] = "Günlük kaydı güncellendi!";
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

            _context.DailyLogs.Remove(log);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Günlük kaydı silindi.";
            return RedirectToAction(nameof(Index));
        }
    }
}
