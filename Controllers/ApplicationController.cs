using CareerTrack.Data;
using CareerTrack.Models.Entities;
using CareerTrack.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CareerTrack.Controllers
{
    [Authorize(Roles = "Student")]
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

        // GET: /Application
        public async Task<IActionResult> Index()
        {
            var userId = await GetUserIdAsync();
            var applications = await _context.JobApplications
                .Include(a => a.Company)
                .Include(a => a.Interviews)
                .Where(a => a.StudentId == userId)
                .OrderByDescending(a => a.ApplicationDate)
                .ToListAsync();

            ViewBag.PendingTodos = await _context.ToDos
                .CountAsync(t => t.StudentId == userId && !t.IsCompleted);

            return View(applications);
        }

        // GET: /Application/Create
        public async Task<IActionResult> Create()
        {
            var vm = new ApplicationCreateViewModel
            {
                ApplicationDate = DateTime.Today,
                Companies = new SelectList(await _context.Companies.OrderBy(c => c.Name).ToListAsync(), "Id", "Name")
            };
            return View(vm);
        }

        // POST: /Application/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ApplicationCreateViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.Companies = new SelectList(await _context.Companies.ToListAsync(), "Id", "Name");
                return View(vm);
            }

            // Controller-level validation: geçmiş tarih engellemesi
            if (vm.ApplicationDate > DateTime.Today)
            {
                ModelState.AddModelError("ApplicationDate", "Başvuru tarihi bugünden ileri bir tarih olamaz.");
                vm.Companies = new SelectList(await _context.Companies.ToListAsync(), "Id", "Name");
                return View(vm);
            }

            var userId = await GetUserIdAsync();
            var application = new JobApplication
            {
                StudentId = userId,
                CompanyId = vm.CompanyId,
                Position = vm.Position,
                ApplicationDate = vm.ApplicationDate,
                InternshipType = vm.InternshipType,
                Status = vm.Status
            };

            _context.JobApplications.Add(application);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Başvuru başarıyla eklendi!";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Application/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var userId = await GetUserIdAsync();
            var app = await _context.JobApplications
                .FirstOrDefaultAsync(a => a.Id == id && a.StudentId == userId);

            if (app == null) return NotFound();

            var vm = new ApplicationCreateViewModel
            {
                Id = app.Id,
                CompanyId = app.CompanyId,
                Position = app.Position,
                ApplicationDate = app.ApplicationDate,
                InternshipType = app.InternshipType,
                Status = app.Status,
                Companies = new SelectList(await _context.Companies.ToListAsync(), "Id", "Name", app.CompanyId)
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

            if (!ModelState.IsValid)
            {
                vm.Companies = new SelectList(await _context.Companies.ToListAsync(), "Id", "Name");
                return View(vm);
            }

            app.CompanyId = vm.CompanyId;
            app.Position = vm.Position;
            app.ApplicationDate = vm.ApplicationDate;
            app.InternshipType = vm.InternshipType;
            app.Status = vm.Status;

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
                .Include(a => a.Interviews)
                .Include(a => a.Offer)
                .FirstOrDefaultAsync(a => a.Id == id && a.StudentId == userId);

            if (app == null) return NotFound();
            return View(app);
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

            _context.JobApplications.Remove(app);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Başvuru silindi.";
            return RedirectToAction(nameof(Index));
        }
    }
}
