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
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            // Rollere göre kendi panellerine yönlendir
            if (User.IsInRole(AppRoles.Admin))
                return RedirectToAction("Index", "Admin");
            if (User.IsInRole(AppRoles.School))
                return RedirectToAction("Index", "School");
            if (User.IsInRole(AppRoles.Employer))
                return RedirectToAction("Index", "Employer");

            var studentId = user.Id;

            // LINQ sorguları — tek ViewModel'e üç tablo
            var activeApplications = await _context.JobApplications
                .Include(a => a.Company)
                .Include(a => a.Interviews)
                .Where(a => a.StudentId == studentId)
                .OrderByDescending(a => a.ApplicationDate)
                .ToListAsync();

            var upcomingInterviews = await _context.Interviews
                .Include(i => i.JobApplication)
                    .ThenInclude(a => a!.Company)
                .Where(i => i.JobApplication!.StudentId == studentId
                         && i.InterviewDate >= DateTime.Today)
                .OrderBy(i => i.InterviewDate)
                .Take(5)
                .ToListAsync();

            var incompleteToDos = await _context.ToDos
                .Where(t => t.StudentId == studentId && !t.IsCompleted)
                .OrderBy(t => t.DueDate)
                .ToListAsync();

            // ViewBag — _Layout bildirim badge'leri için
            ViewBag.PendingTodos = incompleteToDos.Count;
            ViewBag.UpcomingInterviews = upcomingInterviews.Count;

            var vm = new StudentDashboardViewModel
            {
                StudentName = user.FullName,
                Department = user.Department,
                ActiveApplications = activeApplications,
                UpcomingInterviews = upcomingInterviews,
                IncompleteToDos = incompleteToDos,
                TotalApplications = activeApplications.Count,
                OfferedCount = activeApplications.Count(a => a.Status == ApplicationStatus.Offered),
                RejectedCount = activeApplications.Count(a => a.Status == ApplicationStatus.Rejected),
                InReviewCount = activeApplications.Count(a => a.Status == ApplicationStatus.InReview)
            };

            return View(vm);
        }
    }
}
