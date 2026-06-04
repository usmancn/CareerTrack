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

        private static bool CanEmployerTransition(ApplicationStatus currentStatus, ApplicationStatus nextStatus)
        {
            if (!Enum.IsDefined(typeof(ApplicationStatus), nextStatus))
                return false;

            if (currentStatus == ApplicationStatus.Pending)
                return nextStatus == ApplicationStatus.PreScreening ||
                       nextStatus == ApplicationStatus.Rejected;

            if (currentStatus >= ApplicationStatus.PreScreening &&
                currentStatus <= ApplicationStatus.Interview)
            {
                return nextStatus == ApplicationStatus.Rejected ||
                       (nextStatus > currentStatus && nextStatus <= ApplicationStatus.EmployerAccepted);
            }

            return false;
        }

        private static string? ValidateStudentTask(string title, string? description, DateTime? dueDate)
        {
            var normalizedTitle = title?.Trim() ?? string.Empty;
            if (normalizedTitle.Length < 3 || normalizedTitle.Length > 200)
                return "Görev başlığı 3-200 karakter arasında olmalıdır.";
            if (!string.IsNullOrEmpty(description) && description.Length > 1000)
                return "Görev açıklaması en fazla 1000 karakter olabilir.";
            if (dueDate.HasValue && dueDate.Value.Date < DateTime.Today)
                return "Görev son tarihi geçmiş bir tarih olamaz.";

            return null;
        }

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
            if (vm.StartDate < DateTime.Today && vm.StartDate.Date != posting.StartDate.Date)
            {
                ModelState.AddModelError(nameof(vm.StartDate), "Başlangıç tarihi geçmiş bir tarih olamaz.");
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

            if (!posting.IsActive && posting.EndDate.Date < DateTime.Today)
            {
                TempData["Error"] = "Bitiş tarihi geçmiş bir ilan tekrar aktif edilemez.";
                return RedirectToAction(nameof(Postings));
            }

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

            if (await _context.JobApplications.AnyAsync(a => a.InternshipPostingId == id))
            {
                TempData["Error"] = "Başvuru almış bir ilan silinemez. İlanı pasif hale getirebilirsiniz.";
                return RedirectToAction(nameof(Postings));
            }

            _context.JobPostings.Remove(posting);
            await _context.SaveChangesAsync();
            TempData["Success"] = "İlan silindi.";
            return RedirectToAction(nameof(Postings));
        }

        // GET: /Employer/Applications — Sadece yeni (Pending) başvurular - Ön Eleme kararı verilir
        public async Task<IActionResult> Applications()
        {
            var user = await GetCurrentUserAsync();
            if (user.CompanyId == null) return RedirectToAction(nameof(Index));

            var apps = await _context.JobApplications
                .Include(a => a.Student)
                .Include(a => a.Company)
                .Include(a => a.ToDos)
                .Where(a => a.CompanyId == user.CompanyId.Value && a.Status == ApplicationStatus.Pending)
                .OrderByDescending(a => a.ApplicationDate)
                .ToListAsync();

            return View(apps);
        }

        // GET: /Employer/ProcessManagement — Ön elemeden geçmiş, süreç devam eden başvurular
        public async Task<IActionResult> ProcessManagement()
        {
            var user = await GetCurrentUserAsync();
            if (user.CompanyId == null) return RedirectToAction(nameof(Index));

            var apps = await _context.JobApplications
                .Include(a => a.Student)
                .Include(a => a.Company)
                .Include(a => a.ToDos)
                .Where(a => a.CompanyId == user.CompanyId.Value &&
                            a.Status >= ApplicationStatus.PreScreening &&
                            a.Status != ApplicationStatus.Rejected)
                .OrderByDescending(a => a.ApplicationDate)
                .ToListAsync();

            return View(apps);
        }

        // POST: /Employer/UpdateApplicationStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateApplicationStatus(int id, ApplicationStatus status, string? employerNote)
        {
            var user = await GetCurrentUserAsync();
            if (user.CompanyId == null) return Forbid();

            var app = await _context.JobApplications
                .Include(a => a.Company)
                .FirstOrDefaultAsync(a => a.Id == id && a.CompanyId == user.CompanyId.Value);

            if (app == null) return NotFound();

            var prevStatus = app.Status;
            if (!CanEmployerTransition(prevStatus, status))
            {
                TempData["Error"] = "Bu başvuru mevcut durumundan seçilen aşamaya geçirilemez.";
                return RedirectToAction(prevStatus == ApplicationStatus.Pending
                    ? nameof(Applications)
                    : nameof(ProcessManagement));
            }

            if (status != ApplicationStatus.Rejected &&
                prevStatus >= ApplicationStatus.PreScreening &&
                prevStatus <= ApplicationStatus.Interview)
            {
                var latestTodo = await _context.ToDos
                    .Where(t => t.JobApplicationId == app.Id)
                    .OrderByDescending(t => t.Id)
                    .FirstOrDefaultAsync();

                if (latestTodo != null && !latestTodo.IsCompleted)
                {
                    TempData["Error"] = "Öğrenci mevcut aşama görevini tamamlamadan başvuru ilerletilemez.";
                    return RedirectToAction(nameof(ProcessManagement));
                }
            }

            app.Status = status;

            if (!string.IsNullOrEmpty(employerNote))
                app.EmployerNote = employerNote;

            // Aşama değişince öğrenciye otomatik ToDo ekle
            if (status != prevStatus)
            {
                string? todoTitle = status switch
                {
                    ApplicationStatus.PreScreening => $"[{app.Company?.Name}] Ön Eleme Aşamasına Girdiniz — Başvurunuzu Takip Edin",
                    ApplicationStatus.AptitudeTest => $"[{app.Company?.Name}] Genel Yetenek Testine Girmeniz Bekleniyor",
                    ApplicationStatus.LanguageTest => $"[{app.Company?.Name}] İngilizce Sınavına Girmeniz Bekleniyor",
                    ApplicationStatus.Interview => $"[{app.Company?.Name}] Mülakata Girmeniz Bekleniyor",
                    ApplicationStatus.EmployerAccepted => $"[{app.Company?.Name}] İşveren sizi staj için kabul etti — Okul Onayı Bekleniyor",
                    ApplicationStatus.Rejected => $"[{app.Company?.Name}] Başvurunuz Sonuçlandı — Yeni Başvuruları İnceleyin",
                    _ => null
                };

                if (todoTitle != null)
                {
                    var todo = new ToDo
                    {
                        StudentId = app.StudentId,
                        JobApplicationId = app.Id, // Bağlantı kuruldu
                        TaskTitle = todoTitle,
                        DueDate = DateTime.Today.AddDays(7),
                        IsCompleted = false,
                        CreatedAt = DateTime.Now
                    };
                    _context.ToDos.Add(todo);
                }
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Aşama başarıyla güncellendi ve öğrenciye bildirildi.";
            // Yönlendirmeyi gelen sayfaya göre yap
            if (status == ApplicationStatus.PreScreening || status == ApplicationStatus.Rejected)
                return RedirectToAction(nameof(Applications));
            return RedirectToAction(nameof(ProcessManagement));
        }

        // POST: /Employer/EvaluateApplication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EvaluateApplication(int id, string? evaluationResult, int? score, string? employerNote)
        {
            var user = await GetCurrentUserAsync();
            if (user.CompanyId == null) return Forbid();

            var app = await _context.JobApplications
                .FirstOrDefaultAsync(a => a.Id == id && a.CompanyId == user.CompanyId.Value);

            if (app == null) return NotFound();

            if (app.Status < ApplicationStatus.PreScreening || app.Status > ApplicationStatus.Interview)
            {
                TempData["Error"] = "Yalnızca devam eden işveren değerlendirme aşamaları güncellenebilir.";
                return RedirectToAction(nameof(ProcessManagement));
            }

            if (score.HasValue && (score.Value < 0 || score.Value > 100))
            {
                TempData["Error"] = "Değerlendirme puanı 0 ile 100 arasında olmalıdır.";
                return RedirectToAction(nameof(ProcessManagement));
            }

            var stageName = app.Status switch
            {
                ApplicationStatus.PreScreening => "Ön Eleme",
                ApplicationStatus.AptitudeTest => "Genel Yetenek Testi",
                ApplicationStatus.LanguageTest => "İngilizce Sınavı",
                ApplicationStatus.Interview => "Mülakat",
                _ => ""
            };

            var finalNoteList = new List<string>();
            if (!string.IsNullOrEmpty(stageName) && !string.IsNullOrEmpty(evaluationResult))
                finalNoteList.Add($"[{stageName}] Sonucu: {evaluationResult}");

            if (score.HasValue)
                finalNoteList.Add($"Puan: {score.Value}");

            if (!string.IsNullOrEmpty(employerNote))
                finalNoteList.Add($"Not: {employerNote}");

            if (finalNoteList.Any())
                app.EmployerNote = string.Join(" | ", finalNoteList);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Değerlendirme sonuçları başarıyla kaydedildi.";
            return RedirectToAction(nameof(ProcessManagement));
        }



        // GET: /Employer/DailyLogs — Sadece SchoolApproved başvuruların günlükleri
        public async Task<IActionResult> DailyLogs()
        {
            var user = await GetCurrentUserAsync();
            if (user.CompanyId == null) return RedirectToAction(nameof(Index));

            // Öğrenci işverene göndermiş (SentToEmployer) günlükleri göster
            var logs = await _context.DailyLogs
                .Include(d => d.Student)
                .Include(d => d.JobApplication)
                .Where(d => d.JobApplication!.CompanyId == user.CompanyId.Value &&
                            d.Status >= DailyLogStatus.SentToEmployer)
                .OrderBy(d => d.Status == DailyLogStatus.SentToEmployer ? 0 : 1)
                .ThenByDescending(d => d.LogDate)
                .ToListAsync();

            return View(logs);
        }

        // POST: /Employer/ApproveLog
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveLog(int id, string? employerNote, bool approve)
        {
            var user = await GetCurrentUserAsync();
            var log = await _context.DailyLogs
                .Include(d => d.JobApplication)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (log == null || log.JobApplication?.CompanyId != user.CompanyId) return NotFound();

            if (log.Status != DailyLogStatus.SentToEmployer)
            {
                TempData["Error"] = "Yalnızca öğrencinin işveren onayına gönderdiği günlükler değerlendirilebilir.";
                return RedirectToAction(nameof(DailyLogs));
            }

            if (!approve && string.IsNullOrWhiteSpace(employerNote))
            {
                TempData["Error"] = "Revize isteği için öğrenciye açıklayıcı bir not yazmalısınız.";
                return RedirectToAction(nameof(DailyLogs));
            }

            if (employerNote?.Trim().Length > 500)
            {
                TempData["Error"] = "İşveren notu en fazla 500 karakter olabilir.";
                return RedirectToAction(nameof(DailyLogs));
            }

            if (approve)
            {
                log.IsEmployerApproved = true;
                log.Status = DailyLogStatus.EmployerApproved; // Artık okula gidecek
            }
            else
            {
                log.IsEmployerApproved = false;
                log.Status = DailyLogStatus.EmployerRejected; // Öğrenciye geri döner
            }
            log.EmployerNote = string.IsNullOrWhiteSpace(employerNote) ? null : employerNote.Trim();
            await _context.SaveChangesAsync();

            TempData["Success"] = approve ? "Günlük onaylandı ve okula iletildi." : "Günlük revize edilmesi için öğrenciye geri gönderildi.";
            return RedirectToAction(nameof(DailyLogs));
        }

        // GET: /Employer/Tasks
        public async Task<IActionResult> Tasks()
        {
            var user = await GetCurrentUserAsync();
            if (user.CompanyId == null) return RedirectToAction(nameof(Index));

            var tasks = await _context.StudentTasks
                .Include(t => t.JobApplication)
                    .ThenInclude(a => a!.Student)
                .Where(t => t.JobApplication!.CompanyId == user.CompanyId.Value)
                .OrderBy(t => t.IsCompleted)
                .ThenByDescending(t => t.CreatedAt)
                .ToListAsync();

            var activeInterns = await _context.JobApplications
                .Include(a => a.Student)
                .Where(a => a.CompanyId == user.CompanyId.Value && a.Status == ApplicationStatus.SchoolApproved)
                .Select(a => new { a.Id, Name = $"{a.Student!.FullName} - {a.Position}" })
                .ToListAsync();

            ViewBag.ActiveInterns = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(activeInterns, "Id", "Name");
            ViewBag.CurrentEmployerId = user.Id;

            return View(tasks);
        }

        // POST: /Employer/AssignTask
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignTask(int jobApplicationId, string title, string? description, DateTime? dueDate)
        {
            var user = await GetCurrentUserAsync();
            if (user.CompanyId == null) return Forbid();

            var app = await _context.JobApplications
                .FirstOrDefaultAsync(a => a.Id == jobApplicationId && a.CompanyId == user.CompanyId.Value);

            if (app == null || app.Status != ApplicationStatus.SchoolApproved)
            {
                TempData["Error"] = "Sadece okul onaylı stajlardaki öğrencilere görev atanabilir.";
                return RedirectToAction(nameof(Tasks));
            }

            var validationError = ValidateStudentTask(title, description, dueDate);
            if (validationError != null)
            {
                TempData["Error"] = validationError;
                return RedirectToAction(nameof(Tasks));
            }

            var task = new StudentTask
            {
                JobApplicationId = app.Id,
                AssignedByEmployerId = user.Id,
                Title = title.Trim(),
                Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
                DueDate = dueDate,
                CreatedAt = DateTime.Now
            };

            _context.StudentTasks.Add(task);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Görev başarıyla atandı.";
            return RedirectToAction(nameof(Tasks));
        }

        // POST: /Employer/EditTask
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTask(int id, string title, string? description, DateTime? dueDate)
        {
            var user = await GetCurrentUserAsync();
            if (user.CompanyId == null) return Forbid();

            var task = await _context.StudentTasks
                .Include(t => t.JobApplication)
                .FirstOrDefaultAsync(t =>
                    t.Id == id &&
                    t.AssignedByEmployerId == user.Id &&
                    t.JobApplication!.CompanyId == user.CompanyId.Value);

            if (task == null) return NotFound();

            if (task.IsCompleted)
            {
                TempData["Error"] = "Tamamlanmış bir görev düzenlenemez.";
                return RedirectToAction(nameof(Tasks));
            }

            var validationError = ValidateStudentTask(title, description, dueDate);
            if (validationError != null)
            {
                TempData["Error"] = validationError;
                return RedirectToAction(nameof(Tasks));
            }

            task.Title = title.Trim();
            task.Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
            task.DueDate = dueDate;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Görev güncellendi.";
            return RedirectToAction(nameof(Tasks));
        }

        // POST: /Employer/DeleteTask
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var user = await GetCurrentUserAsync();
            if (user.CompanyId == null) return Forbid();

            var task = await _context.StudentTasks
                .Include(t => t.JobApplication)
                .FirstOrDefaultAsync(t =>
                    t.Id == id &&
                    t.AssignedByEmployerId == user.Id &&
                    t.JobApplication!.CompanyId == user.CompanyId.Value);

            if (task == null) return NotFound();

            if (task.IsCompleted)
            {
                TempData["Error"] = "Tamamlanmış bir görev silinemez.";
                return RedirectToAction(nameof(Tasks));
            }

            _context.StudentTasks.Remove(task);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Görev silindi.";
            return RedirectToAction(nameof(Tasks));
        }
    }
}
