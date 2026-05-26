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
                // Okul onay bekleyenler = EmployerAccepted
                PendingDailyLogs = await _context.DailyLogs.CountAsync(d => d.Status == DailyLogStatus.EmployerApproved),
                TotalDailyLogs = await _context.DailyLogs.CountAsync(),
                RecentDailyLogs = await _context.DailyLogs
                    .Include(d => d.Student)
                    .Where(d => d.Status == DailyLogStatus.EmployerApproved)
                    .OrderByDescending(d => d.LogDate)
                    .Take(8)
                    .ToListAsync(),
                RecentApplications = await _context.JobApplications
                    .Include(a => a.Company)
                    .Include(a => a.Student)
                    .Where(a => a.Status == ApplicationStatus.EmployerAccepted)
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

        // GET: /School/Students — Öğrenci Takibi ayrı sayfa
        public async Task<IActionResult> Students()
        {
            var studentUsers = await _userManager.GetUsersInRoleAsync(AppRoles.Student);

            var students = new List<SchoolStudentDetailViewModel>();
            foreach (var s in studentUsers.OrderBy(s => s.FullName))
            {
                var apps = await _context.JobApplications
                    .Include(a => a.Company)
                    .Where(a => a.StudentId == s.Id)
                    .ToListAsync();

                var logs = await _context.DailyLogs
                    .Where(d => d.StudentId == s.Id)
                    .ToListAsync();

                students.Add(new SchoolStudentDetailViewModel
                {
                    StudentId = s.Id,
                    StudentName = s.FullName,
                    Department = s.Department,
                    Email = s.Email ?? "",
                    TotalApplications = apps.Count,
                    OfferedCount = apps.Count(a => a.Status == ApplicationStatus.SchoolApproved || a.Status == ApplicationStatus.Completed),
                    RejectedCount = apps.Count(a => a.Status == ApplicationStatus.Rejected),
                    TotalDailyLogs = logs.Count,
                    ApprovedDailyLogs = logs.Count(d => d.IsSchoolApproved),
                    Applications = apps,
                    DailyLogs = logs
                });
            }

            return View(students);
        }

        // GET: /School/Applications — Sadece EmployerAccepted olanları göster (okul onayı bekleyenler)
        public async Task<IActionResult> Applications()
        {
            var apps = await _context.JobApplications
                .Include(a => a.Company)
                .Include(a => a.Student)
                .Where(a => a.Status == ApplicationStatus.EmployerAccepted ||
                            a.Status == ApplicationStatus.SchoolPending ||
                            a.Status == ApplicationStatus.SchoolRevision)
                .OrderByDescending(a => a.ApplicationDate)
                .ToListAsync();

            return View(apps);
        }

        // POST: /School/ApproveApplication/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveApplication(int id, string? schoolNote, bool approve)
        {
            var app = await _context.JobApplications.FindAsync(id);
            if (app == null) return NotFound();

            if (approve)
            {
                // Okul onayladı → staj başlar, öğrenci günlük yazabilir
                app.Status = ApplicationStatus.SchoolApproved;
            }
            else
            {
                // Okul reddetti veya revize istedi
                app.Status = ApplicationStatus.Rejected;
            }
            app.SchoolNote = schoolNote;
            await _context.SaveChangesAsync();

            TempData["Success"] = approve
                ? "Staj onaylandı! Öğrenci artık günlük yazabilir."
                : "Staj talebi reddedildi.";
            return RedirectToAction(nameof(Applications));
        }

        // GET: /School/DailyLogs — İşveren onaylı günlükleri listele
        public async Task<IActionResult> DailyLogs()
        {
            var logs = await _context.DailyLogs
                .Include(d => d.Student)
                .Include(d => d.JobApplication)
                    .ThenInclude(a => a!.Company)
                .Where(d => d.Status == DailyLogStatus.EmployerApproved)
                .OrderByDescending(d => d.LogDate)
                .ToListAsync();

            return View(logs);
        }

        // POST: /School/ApproveLog/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveLog(int id, string? schoolNote, bool approve)
        {
            var log = await _context.DailyLogs.FindAsync(id);
            if (log == null) return NotFound();

            if (approve)
            {
                log.IsSchoolApproved = true;
                log.Status = DailyLogStatus.SchoolApproved;
            }
            else
            {
                log.IsSchoolApproved = false;
                log.Status = DailyLogStatus.SchoolRejected;
            }
            log.SchoolNote = schoolNote;
            await _context.SaveChangesAsync();

            TempData["Success"] = approve ? "Günlük onaylandı." : "Günlük revize gerekli olarak işaretlendi.";
            return RedirectToAction(nameof(DailyLogs));
        }

        // GET: /School/StudentDetail/userId
        public async Task<IActionResult> StudentDetail(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var student = await _userManager.FindByIdAsync(id);
            if (student == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(student);
            if (!roles.Contains(AppRoles.Student)) return NotFound();

            var applications = await _context.JobApplications
                .Include(a => a.Company)
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
                OfferedCount = applications.Count(a => a.Status == ApplicationStatus.SchoolApproved ||
                                                       a.Status == ApplicationStatus.Completed),
                RejectedCount = applications.Count(a => a.Status == ApplicationStatus.Rejected),
                TotalDailyLogs = dailyLogs.Count,
                ApprovedDailyLogs = dailyLogs.Count(d => d.IsSchoolApproved),
                Applications = applications,
                DailyLogs = dailyLogs
            };

            return View(vm);
        }

        // POST: /School/CompleteInternship
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteInternship(int id)
        {
            var app = await _context.JobApplications.FindAsync(id);
            if (app == null) return NotFound();

            if (app.Status == ApplicationStatus.SchoolApproved)
            {
                app.Status = ApplicationStatus.Completed;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Staj başarıyla tamamlandı ve kapatıldı.";
            }
            else
            {
                TempData["Error"] = "Sadece devam eden (Okul Onaylı) stajlar tamamlanabilir.";
            }

            return RedirectToAction(nameof(StudentDetail), new { id = app.StudentId });
        }
    }
}
