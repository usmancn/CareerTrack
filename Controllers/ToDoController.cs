using CareerTrack.Data;
using CareerTrack.Models.Constants;
using CareerTrack.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CareerTrack.Controllers
{
    [Authorize(Roles = AppRoles.Student)]
    public class ToDoController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ToDoController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private async Task<string> GetUserIdAsync() =>
            (await _userManager.GetUserAsync(User))!.Id;

        public async Task<IActionResult> Index()
        {
            var userId = await GetUserIdAsync();
            var todos = await _context.ToDos
                .Where(t => t.StudentId == userId)
                .OrderBy(t => t.IsCompleted).ThenBy(t => t.DueDate)
                .ToListAsync();

            var studentTasks = await _context.StudentTasks
                .Include(t => t.JobApplication)
                    .ThenInclude(j => j!.Company)
                .Where(t => t.JobApplication!.StudentId == userId)
                .OrderBy(t => t.IsCompleted).ThenBy(t => t.DueDate)
                .ToListAsync();

            ViewBag.PendingTodos = todos.Count(t => !t.IsCompleted);
            ViewBag.StudentTasks = studentTasks;

            return View(todos);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string taskTitle, DateTime dueDate)
        {
            var normalizedTitle = taskTitle?.Trim() ?? string.Empty;
            if (normalizedTitle.Length < 3 || normalizedTitle.Length > 200)
            {
                TempData["Error"] = "Görev başlığı 3-200 karakter arasında olmalıdır.";
                return RedirectToAction(nameof(Index));
            }
            if (dueDate < DateTime.Today)
            {
                TempData["Error"] = "Bitiş tarihi geçmiş bir tarih olamaz.";
                return RedirectToAction(nameof(Index));
            }

            var userId = await GetUserIdAsync();
            _context.ToDos.Add(new ToDo
            {
                StudentId = userId,
                TaskTitle = normalizedTitle,
                DueDate = dueDate,
                CreatedAt = DateTime.Now
            });
            await _context.SaveChangesAsync();
            TempData["Success"] = "Görev eklendi!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string taskTitle, DateTime dueDate)
        {
            var userId = await GetUserIdAsync();
            var todo = await _context.ToDos.FirstOrDefaultAsync(t => t.Id == id && t.StudentId == userId);
            if (todo == null) return NotFound();

            if (todo.JobApplicationId.HasValue)
            {
                TempData["Error"] = "Başvuru aşaması görevleri düzenlenemez.";
                return RedirectToAction(nameof(Index));
            }

            var normalizedTitle = taskTitle?.Trim() ?? string.Empty;
            if (normalizedTitle.Length < 3 || normalizedTitle.Length > 200)
            {
                TempData["Error"] = "Görev başlığı 3-200 karakter arasında olmalıdır.";
                return RedirectToAction(nameof(Index));
            }

            if (dueDate < DateTime.Today)
            {
                TempData["Error"] = "Bitiş tarihi geçmiş bir tarih olamaz.";
                return RedirectToAction(nameof(Index));
            }

            todo.TaskTitle = normalizedTitle;
            todo.DueDate = dueDate;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Görev güncellendi!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Toggle(int id)
        {
            var userId = await GetUserIdAsync();
            var todo = await _context.ToDos.FirstOrDefaultAsync(t => t.Id == id && t.StudentId == userId);
            if (todo == null) return NotFound();
            todo.IsCompleted = !todo.IsCompleted;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = await GetUserIdAsync();
            var todo = await _context.ToDos.FirstOrDefaultAsync(t => t.Id == id && t.StudentId == userId);
            if (todo == null) return NotFound();

            if (todo.JobApplicationId.HasValue)
            {
                TempData["Error"] = "Başvuru aşaması görevleri silinemez.";
                return RedirectToAction(nameof(Index));
            }

            _context.ToDos.Remove(todo);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStudentTask(int id)
        {
            var userId = await GetUserIdAsync();
            var task = await _context.StudentTasks
                .Include(t => t.JobApplication)
                .FirstOrDefaultAsync(t => t.Id == id && t.JobApplication!.StudentId == userId);

            if (task == null) return NotFound();

            task.IsCompleted = !task.IsCompleted;
            task.CompletedAt = task.IsCompleted ? DateTime.Now : null;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
