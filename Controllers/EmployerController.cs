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
    [Authorize(Roles = AppRoles.Employer)]
    public class EmployerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public EmployerController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private async Task<ApplicationUser> GetCurrentUserAsync() =>
            (await _userManager.GetUserAsync(User))!;

        // GET: /Employer
        public async Task<IActionResult> Index()
        {
            var user = await GetCurrentUserAsync();
            if (user.CompanyId == null)
            {
                TempData["Error"] = "Hesabınıza bir şirket bağlanmamış. Lütfen admin ile iletişime geçin.";
                return View(new EmployerDashboardViewModel());
            }

            var companyId = user.CompanyId.Value;
            var company = await _context.Companies.FindAsync(companyId);

            var postings = await _context.JobPostings
                .Where(jp => jp.EmployerId == user.Id)
                .ToListAsync();

            var applications = await _context.JobApplications
                .Include(a => a.Student)
                .Include(a => a.Company)
                .Where(a => a.CompanyId == companyId)
                .OrderByDescending(a => a.ApplicationDate)
                .ToListAsync();

            var vm = new EmployerDashboardViewModel
            {
                CompanyName = company?.Name ?? "Bilinmiyor",
                TotalPostings = postings.Count,
                ActivePostings = postings.Count(p => p.IsActive),
                TotalApplications = applications.Count,
                PendingApplications = applications.Count(a => a.Status == ApplicationStatus.Pending),
                RecentPostings = await _context.JobPostings
                    .Where(jp => jp.EmployerId == user.Id)
                    .OrderByDescending(jp => jp.CreatedAt)
                    .Take(5)
                    .ToListAsync(),
                RecentApplications = applications.Take(10).ToList()
            };

            return View(vm);
        }

        // GET: /Employer/Postings
        public async Task<IActionResult> Postings()
        {
            var user = await GetCurrentUserAsync();
            var postings = await _context.JobPostings
                .Include(jp => jp.Company)
                .Where(jp => jp.EmployerId == user.Id)
                .OrderByDescending(jp => jp.CreatedAt)
                .ToListAsync();

            return View(postings);
        }

        // GET: /Employer/CreatePosting
        public IActionResult CreatePosting()
        {
            return View(new JobPostingCreateViewModel());
        }

        // POST: /Employer/CreatePosting
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePosting(JobPostingCreateViewModel vm)
        {
            if (vm.EndDate <= vm.StartDate)
            {
                ModelState.AddModelError(nameof(vm.EndDate), "Bitiş tarihi, başlangıç tarihinden sonra olmalıdır.");
            }
            if (vm.StartDate < DateTime.Today)
            {
                ModelState.AddModelError(nameof(vm.StartDate), "Başlangıç tarihi geçmiş bir tarih olamaz.");
            }

            if (!ModelState.IsValid) return View(vm);

            var user = await GetCurrentUserAsync();
            if (user.CompanyId == null)
            {
                TempData["Error"] = "Şirket bağlantınız yok. İlan oluşturamazsınız.";
                return RedirectToAction(nameof(Index));
            }

            var posting = new JobPosting
            {
                EmployerId = user.Id,
                CompanyId = user.CompanyId.Value,
                Title = vm.Title,
                Description = vm.Description,
                InternshipType = vm.InternshipType,
                Quota = vm.Quota,
                StartDate = vm.StartDate,
                EndDate = vm.EndDate,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            _context.JobPostings.Add(posting);
            await _context.SaveChangesAsync();
            TempData["Success"] = "İlan başarıyla yayınlandı!";
            return RedirectToAction(nameof(Postings));
        }

        // GET: /Employer/EditPosting/5
        public async Task<IActionResult> EditPosting(int id)
        {
            var user = await GetCurrentUserAsync();
            var posting = await _context.JobPostings
                .FirstOrDefaultAsync(jp => jp.Id == id && jp.EmployerId == user.Id);

            if (posting == null) return NotFound();

            var vm = new JobPostingCreateViewModel
            {
                Id = posting.Id,
                Title = posting.Title,
                Description = posting.Description,
                InternshipType = posting.InternshipType,
                Quota = posting.Quota,
                StartDate = posting.StartDate,
                EndDate = posting.EndDate
            };
            return View(vm);
        }

        // POST: /Employer/EditPosting/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPosting(int id, JobPostingCreateViewModel vm)
        {
            var user = await GetCurrentUserAsync();
            var posting = await _context.JobPostings
                .FirstOrDefaultAsync(jp => jp.Id == id && jp.EmployerId == user.Id);

            if (posting == null) return NotFound();

            if (vm.EndDate <= vm.StartDate)
            {
                ModelState.AddModelError(nameof(vm.EndDate), "Bitiş tarihi, başlangıç tarihinden sonra olmalıdır.");
            }

            if (!ModelState.IsValid) return View(vm);

            posting.Title = vm.Title;
            posting.Description = vm.Description;
            posting.InternshipType = vm.InternshipType;
            posting.Quota = vm.Quota;
            posting.StartDate = vm.StartDate;
            posting.EndDate = vm.EndDate;

            await _context.SaveChangesAsync();
            TempData["Success"] = "İlan güncellendi!";
            return RedirectToAction(nameof(Postings));
        }

        // POST: /Employer/TogglePosting/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TogglePosting(int id)
        {
            var user = await GetCurrentUserAsync();
            var posting = await _context.JobPostings
                .FirstOrDefaultAsync(jp => jp.Id == id && jp.EmployerId == user.Id);

            if (posting == null) return NotFound();

            posting.IsActive = !posting.IsActive;
            await _context.SaveChangesAsync();
            TempData["Success"] = posting.IsActive ? "İlan tekrar aktif edildi." : "İlan pasif yapıldı.";
            return RedirectToAction(nameof(Postings));
        }

        // POST: /Employer/DeletePosting/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePosting(int id)
        {
            var user = await GetCurrentUserAsync();
            var posting = await _context.JobPostings
                .FirstOrDefaultAsync(jp => jp.Id == id && jp.EmployerId == user.Id);

            if (posting == null) return NotFound();

            _context.JobPostings.Remove(posting);
            await _context.SaveChangesAsync();
            TempData["Success"] = "İlan silindi.";
            return RedirectToAction(nameof(Postings));
        }

        // GET: /Employer/Applications
        public async Task<IActionResult> Applications()
        {
            var user = await GetCurrentUserAsync();
            if (user.CompanyId == null) return RedirectToAction(nameof(Index));

            var apps = await _context.JobApplications
                .Include(a => a.Student)
                .Include(a => a.Company)
                .Include(a => a.Interviews)
                .Where(a => a.CompanyId == user.CompanyId.Value)
                .OrderByDescending(a => a.ApplicationDate)
                .ToListAsync();

            return View(apps);
        }

        // POST: /Employer/UpdateApplicationStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateApplicationStatus(int id, ApplicationStatus status)
        {
            var user = await GetCurrentUserAsync();
            if (user.CompanyId == null) return Forbid();

            var app = await _context.JobApplications
                .FirstOrDefaultAsync(a => a.Id == id && a.CompanyId == user.CompanyId.Value);

            if (app == null) return NotFound();

            app.Status = status;
            await _context.SaveChangesAsync();

            var statusText = status switch
            {
                ApplicationStatus.Pending => "Bekliyor",
                ApplicationStatus.InReview => "Değerlendirmede",
                ApplicationStatus.Rejected => "Reddedildi",
                ApplicationStatus.Offered => "Teklif Gönderildi",
                _ => status.ToString()
            };

            TempData["Success"] = $"Başvuru durumu '{statusText}' olarak güncellendi.";
            return RedirectToAction(nameof(Applications));
        }
    }
}
