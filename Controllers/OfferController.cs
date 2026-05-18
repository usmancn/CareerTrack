using CareerTrack.Data;
using CareerTrack.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CareerTrack.Controllers
{
    [Authorize(Roles = "Student")]
    public class OfferController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public OfferController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private async Task<string> GetUserIdAsync() =>
            (await _userManager.GetUserAsync(User))!.Id;

        // GET: /Offer/Create?applicationId=5
        public async Task<IActionResult> Create(int applicationId)
        {
            var userId = await GetUserIdAsync();
            var app = await _context.JobApplications
                .Include(a => a.Company)
                .FirstOrDefaultAsync(a => a.Id == applicationId && a.StudentId == userId);

            if (app == null) return NotFound();

            // Zaten teklif varsa details'e yönlendir
            var existing = await _context.Offers.FirstOrDefaultAsync(o => o.JobApplicationId == applicationId);
            if (existing != null)
                return RedirectToAction(nameof(Details), new { id = existing.Id });

            ViewBag.Application = app;
            return View(new Offer { JobApplicationId = applicationId, DeadlineDate = DateTime.Today.AddDays(14) });
        }

        // POST: /Offer/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Offer model)
        {
            var userId = await GetUserIdAsync();
            var app = await _context.JobApplications
                .FirstOrDefaultAsync(a => a.Id == model.JobApplicationId && a.StudentId == userId);

            if (app == null) return NotFound();

            // Son dönüş tarihi geçmiş olamaz
            if (model.DeadlineDate < DateTime.Today)
            {
                ModelState.AddModelError("DeadlineDate", "Son dönüş tarihi geçmiş bir tarih olamaz.");
                ViewBag.Application = app;
                return View(model);
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Application = app;
                return View(model);
            }

            _context.Offers.Add(model);

            // Başvuru durumunu "Teklif Alındı" olarak güncelle
            app.Status = Models.Enums.ApplicationStatus.Offered;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Teklif detayları kaydedildi!";
            return RedirectToAction(nameof(Details), new { id = model.Id });
        }

        // GET: /Offer/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var userId = await GetUserIdAsync();
            var offer = await _context.Offers
                .Include(o => o.JobApplication)
                    .ThenInclude(a => a!.Company)
                .FirstOrDefaultAsync(o => o.Id == id && o.JobApplication!.StudentId == userId);

            if (offer == null) return NotFound();
            return View(offer);
        }

        // POST: /Offer/Accept/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Accept(int id)
        {
            var userId = await GetUserIdAsync();
            var offer = await _context.Offers
                .Include(o => o.JobApplication)
                .FirstOrDefaultAsync(o => o.Id == id && o.JobApplication!.StudentId == userId);

            if (offer == null) return NotFound();

            offer.IsAccepted = true;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Teklif kabul edildi! Staj defterinizi yazmaya başlayabilirsiniz.";
            return RedirectToAction("Index", "DailyLog");
        }
    }
}
