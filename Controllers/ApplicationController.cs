using CareerTrack.Data;
using CareerTrack.Models.Constants;
using CareerTrack.Models.Entities;
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

        /// <summary>
        /// Onaylı şirketler + bu öğrencinin önerdiği (henüz onaylanmamış) şirketler.
        /// </summary>
        private async Task<SelectList> BuildCompanySelectListAsync(string userId, int? selectedCompanyId = null)
        {
            var companies = await _context.Companies
                .Where(c => c.IsApproved || c.CreatedByUserId == userId)
                .OrderBy(c => c.Name)
                .ToListAsync();

            // Onaylanmamış şirketlerin adının yanına "(Onay Bekliyor)" ekle
            var items = companies.Select(c => new
            {
                c.Id,
                DisplayName = c.IsApproved ? c.Name : $"{c.Name} (Onay Bekliyor)"
            });

            return new SelectList(items, "Id", "DisplayName", selectedCompanyId);
        }

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
            {
                ModelState.AddModelError(nameof(vm.CompanyId), "Geçerli bir şirket seçiniz.");
            }

            if (vm.ApplicationDate > DateTime.Today)
            {
                ModelState.AddModelError("ApplicationDate", "Başvuru tarihi bugünden ileri bir tarih olamaz.");
            }

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
                Status = vm.Status
            };

            _context.JobApplications.Add(application);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Başvuru başarıyla eklendi!";
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

            // Aynı isimde şirket var mı kontrol et
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

            TempData["Success"] = $"\"{company.Name}\" şirketi önerildi. Admin onayından sonra herkes tarafından görünecek, şimdilik siz seçebilirsiniz.";
            return RedirectToAction(nameof(Create));
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

            if (!await _context.Companies.AnyAsync(c => c.Id == vm.CompanyId))
            {
                ModelState.AddModelError(nameof(vm.CompanyId), "Geçerli bir şirket seçiniz.");
            }

            if (vm.ApplicationDate > DateTime.Today)
            {
                ModelState.AddModelError(nameof(vm.ApplicationDate), "Başvuru tarihi bugünden ileri bir tarih olamaz.");
            }

            if (!ModelState.IsValid)
            {
                vm.Companies = await BuildCompanySelectListAsync(userId, vm.CompanyId);
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
