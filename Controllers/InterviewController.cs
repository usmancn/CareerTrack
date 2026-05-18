using CareerTrack.Data;
using CareerTrack.Models.Constants;
using CareerTrack.Models.Entities;
using CareerTrack.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CareerTrack.Controllers
{
    [Authorize(Roles = AppRoles.Student)]
    public class InterviewController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public InterviewController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private async Task<string> GetUserIdAsync() =>
            (await _userManager.GetUserAsync(User))!.Id;

        // GET: /Interview/Index/applicationId
        public async Task<IActionResult> Index(int applicationId)
        {
            var userId = await GetUserIdAsync();
            var app = await _context.JobApplications
                .Include(a => a.Company)
                .FirstOrDefaultAsync(a => a.Id == applicationId && a.StudentId == userId);

            if (app == null) return NotFound();

            var interviews = await _context.Interviews
                .Where(i => i.JobApplicationId == applicationId)
                .OrderBy(i => i.InterviewDate)
                .ToListAsync();

            ViewBag.Application = app;
            ViewBag.PendingTodos = await _context.ToDos.CountAsync(t => t.StudentId == userId && !t.IsCompleted);

            return View(interviews);
        }

        // GET: /Interview/Create?applicationId=5
        public async Task<IActionResult> Create(int applicationId)
        {
            var userId = await GetUserIdAsync();
            var app = await _context.JobApplications
                .Include(a => a.Company)
                .FirstOrDefaultAsync(a => a.Id == applicationId && a.StudentId == userId);

            if (app == null) return NotFound();

            var vm = new InterviewCreateViewModel
            {
                JobApplicationId = applicationId,
                InterviewDate = DateTime.Today.AddDays(7),
                CompanyName = app.Company?.Name,
                Position = app.Position
            };
            return View(vm);
        }

        // POST: /Interview/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InterviewCreateViewModel vm)
        {
            var userId = await GetUserIdAsync();
            var app = await _context.JobApplications
                .Include(a => a.Company)
                .FirstOrDefaultAsync(a => a.Id == vm.JobApplicationId && a.StudentId == userId);

            if (app == null) return NotFound();

            vm.CompanyName = app.Company?.Name;
            vm.Position = app.Position;

            // Geçmiş tarih kontrolü
            if (vm.InterviewDate < DateTime.Today)
            {
                ModelState.AddModelError("InterviewDate", "Mülakat tarihi geçmiş bir tarih olamaz.");
            }

            if (!ModelState.IsValid)
                return View(vm);

            var interview = new Interview
            {
                JobApplicationId = vm.JobApplicationId,
                InterviewDate = vm.InterviewDate,
                Stage = vm.Stage,
                Notes = vm.Notes,
                ResultStatus = vm.ResultStatus
            };

            _context.Interviews.Add(interview);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Mülakat aşaması eklendi!";
            return RedirectToAction(nameof(Index), new { applicationId = vm.JobApplicationId });
        }

        // GET: /Interview/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var userId = await GetUserIdAsync();
            var interview = await _context.Interviews
                .Include(i => i.JobApplication)
                    .ThenInclude(a => a!.Company)
                .FirstOrDefaultAsync(i => i.Id == id && i.JobApplication!.StudentId == userId);

            if (interview == null) return NotFound();

            var vm = new InterviewCreateViewModel
            {
                Id = interview.Id,
                JobApplicationId = interview.JobApplicationId,
                InterviewDate = interview.InterviewDate,
                Stage = interview.Stage,
                Notes = interview.Notes,
                ResultStatus = interview.ResultStatus,
                CompanyName = interview.JobApplication?.Company?.Name,
                Position = interview.JobApplication?.Position
            };
            return View(vm);
        }

        // POST: /Interview/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, InterviewCreateViewModel vm)
        {
            var userId = await GetUserIdAsync();
            var interview = await _context.Interviews
                .Include(i => i.JobApplication)
                    .ThenInclude(a => a!.Company)
                .FirstOrDefaultAsync(i => i.Id == id && i.JobApplication!.StudentId == userId);

            if (interview == null) return NotFound();

            vm.JobApplicationId = interview.JobApplicationId;
            vm.CompanyName = interview.JobApplication?.Company?.Name;
            vm.Position = interview.JobApplication?.Position;

            if (vm.InterviewDate < DateTime.Today)
            {
                ModelState.AddModelError(nameof(vm.InterviewDate), "Mülakat tarihi geçmiş bir tarih olamaz.");
            }

            if (!ModelState.IsValid) return View(vm);

            interview.InterviewDate = vm.InterviewDate;
            interview.Stage = vm.Stage;
            interview.Notes = vm.Notes;
            interview.ResultStatus = vm.ResultStatus;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Mülakat güncellendi!";
            return RedirectToAction(nameof(Index), new { applicationId = interview.JobApplicationId });
        }

        // POST: /Interview/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = await GetUserIdAsync();
            var interview = await _context.Interviews
                .Include(i => i.JobApplication)
                .FirstOrDefaultAsync(i => i.Id == id && i.JobApplication!.StudentId == userId);

            if (interview == null) return NotFound();

            var appId = interview.JobApplicationId;
            _context.Interviews.Remove(interview);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Mülakat silindi.";
            return RedirectToAction(nameof(Index), new { applicationId = appId });
        }
    }
}
