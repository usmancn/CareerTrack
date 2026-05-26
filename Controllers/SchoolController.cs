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
    [Authorize(Roles = AppRoles.School)]
    public class SchoolController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public SchoolController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /School
        public async Task<IActionResult> Index()
        {
            var studentUsers = await _userManager.GetUsersInRoleAsync(AppRoles.Student);
            var vm = new SchoolDashboardViewModel
            {
                TotalStudents = studentUsers.Count,
                TotalApplications = await _context.JobApplications.CountAsync(),
                PendingDailyLogs = await _context.DailyLogs.CountAsync(d => !d.IsApprovedByAdmin),
                TotalDailyLogs = await _context.DailyLogs.CountAsync(),
                RecentDailyLogs = await _context.DailyLogs
                    .Include(d => d.Student)
                    .OrderBy(d => d.IsApprovedByAdmin)
                    .ThenByDescending(d => d.LogDate)
                    .Take(8)
                    .ToListAsync(),
                RecentApplications = await _context.JobApplications
                    .Include(a => a.Company)
                    .Include(a => a.Student)
                    .OrderByDescending(a => a.ApplicationDate)
                    .Take(8)
                    .ToListAsync(),
                Students = studentUsers.Select(s => new StudentListItemViewModel
                {
                    UserId = s.Id,
                    FullName = s.FullName,
                    Department = s.Department,
                    Email = s.Email ?? string.Empty
                }).OrderBy(s => s.FullName).ToList()
            };

            return View(vm);
        }

        // GET: /School/StudentDetail/userId
        public async Task<IActionResult> StudentDetail(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var student = await _userManager.FindByIdAsync(id);
            if (student == null) return NotFound();

            // Rol kontrolü — sadece öğrencileri göster
            var roles = await _userManager.GetRolesAsync(student);
            if (!roles.Contains(AppRoles.Student)) return NotFound();

            var applications = await _context.JobApplications
                .Include(a => a.Company)
                .Include(a => a.Interviews)
                .Where(a => a.StudentId == id)
                .OrderByDescending(a => a.ApplicationDate)
                .ToListAsync();

            var dailyLogs = await _context.DailyLogs
                .Where(d => d.StudentId == id)
                .OrderByDescending(d => d.DayNumber)
                .ToListAsync();

            var vm = new SchoolStudentDetailViewModel
            {
                StudentId = id,
                StudentName = student.FullName,
                Department = student.Department,
                Email = student.Email ?? string.Empty,
                TotalApplications = applications.Count,
                OfferedCount = applications.Count(a => a.Status == ApplicationStatus.Offered),
                RejectedCount = applications.Count(a => a.Status == ApplicationStatus.Rejected),
                TotalDailyLogs = dailyLogs.Count,
                ApprovedDailyLogs = dailyLogs.Count(d => d.IsApprovedByAdmin),
                Applications = applications,
                DailyLogs = dailyLogs
            };

            return View(vm);
        }

        // GET: /School/DailyLogs
        public async Task<IActionResult> DailyLogs()
        {
            var logs = await _context.DailyLogs
                .Include(d => d.Student)
                .OrderBy(d => d.IsApprovedByAdmin)
                .ThenByDescending(d => d.LogDate)
                .ToListAsync();

            return View(logs);
        }

        // POST: /School/ApproveLog/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveLog(int id, string? adminNote, bool approve)
        {
            var log = await _context.DailyLogs.FindAsync(id);
            if (log == null) return NotFound();

            log.IsApprovedByAdmin = approve;
            log.AdminNote = adminNote;
            await _context.SaveChangesAsync();

            TempData["Success"] = approve ? "Günlük onaylandı." : "Günlük revize gerekli olarak işaretlendi.";
            return RedirectToAction(nameof(DailyLogs));
        }

        // GET: /School/Applications
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
    }
}
