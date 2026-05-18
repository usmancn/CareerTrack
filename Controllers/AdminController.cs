using CareerTrack.Data;
using CareerTrack.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CareerTrack.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Admin
        public async Task<IActionResult> Index()
        {
            var vm = new AdminDashboardViewModel
            {
                TotalStudents = await _context.Users.CountAsync(),
                TotalApplications = await _context.JobApplications.CountAsync(),
                TotalOffered = await _context.JobApplications
                    .CountAsync(a => a.Status == Models.Enums.ApplicationStatus.Offered),
                PendingDailyLogs = await _context.DailyLogs.CountAsync(d => !d.IsApprovedByAdmin),
                TotalDailyLogs = await _context.DailyLogs.CountAsync(),

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
                    .ToListAsync()
            };

            return View(vm);
        }

        // GET: /Admin/DailyLogs
        public async Task<IActionResult> DailyLogs()
        {
            var logs = await _context.DailyLogs
                .Include(d => d.Student)
                .OrderBy(d => d.IsApprovedByAdmin)
                .ThenByDescending(d => d.LogDate)
                .ToListAsync();
            return View(logs);
        }

        // POST: /Admin/ApproveLog/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveLog(int id, string? adminNote, bool approve)
        {
            var log = await _context.DailyLogs.FindAsync(id);
            if (log == null) return NotFound();

            log.IsApprovedByAdmin = approve;
            log.AdminNote = adminNote;
            await _context.SaveChangesAsync();

            TempData["Success"] = approve ? "Günlük onaylandı." : "Günlük 'Revize Gerekli' olarak işaretlendi.";
            return RedirectToAction(nameof(DailyLogs));
        }

        // GET: /Admin/Applications
        public async Task<IActionResult> Applications()
        {
            var apps = await _context.JobApplications
                .Include(a => a.Company)
                .Include(a => a.Student)
                .Include(a => a.Interviews)
                .OrderByDescending(a => a.ApplicationDate)
                .ToListAsync();
            return View(apps);
        }

        // GET: /Admin/Companies
        public async Task<IActionResult> Companies()
        {
            var companies = await _context.Companies
                .Include(c => c.JobApplications)
                .OrderBy(c => c.Name)
                .ToListAsync();
            return View(companies);
        }

        // POST: /Admin/AddCompany
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCompany(string name, string sector, string location)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["Error"] = "Şirket adı boş olamaz.";
                return RedirectToAction(nameof(Companies));
            }

            _context.Companies.Add(new Models.Entities.Company
            {
                Name = name,
                Sector = sector ?? "Belirtilmedi",
                Location = location ?? "Belirtilmedi"
            });
            await _context.SaveChangesAsync();
            TempData["Success"] = "Şirket eklendi!";
            return RedirectToAction(nameof(Companies));
        }
    }
}
