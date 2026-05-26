using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using CareerTrack.Data;
using CareerTrack.Models.Entities;
using Microsoft.AspNetCore.Identity;

namespace CareerTrack.ViewComponents
{
    public class SidebarStatsViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public SidebarStatsViewComponent(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            if (user == null) return Content(string.Empty);

            var roles = await _userManager.GetRolesAsync(user);
            int count = 0;
            string label = "";
            string icon = "";
            string badgeClass = "bg-primary";

            if (roles.Contains("Student"))
            {
                count = await _context.ToDos.CountAsync(t => t.StudentId == user.Id && !t.IsCompleted);
                label = "Bekleyen Görev";
                icon = "bi-list-check";
                badgeClass = "bg-warning text-dark";
            }
            else if (roles.Contains("Employer"))
            {
                count = await _context.JobApplications.CountAsync(a => a.Company.EmployerId == user.Id && a.Status == Models.Enums.ApplicationStatus.Pending);
                label = "Yeni Başvuru";
                icon = "bi-envelope";
                badgeClass = "bg-success";
            }
            else if (roles.Contains("Admin"))
            {
                count = await _context.Companies.CountAsync(c => !c.IsApproved);
                label = "Onay Bekleyen Şirket";
                icon = "bi-building-exclamation";
                badgeClass = "bg-danger";
            }
            else
            {
                return Content(string.Empty);
            }

            var model = new SidebarStatsViewModel 
            { 
                Count = count, 
                Label = label ?? string.Empty, 
                Icon = icon ?? string.Empty,
                BadgeClass = badgeClass ?? string.Empty
            };
            return View(model);
        }
    }

    public class SidebarStatsViewModel
    {
        public int Count { get; set; }
        public string Label { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string BadgeClass { get; set; } = string.Empty;
    }
}
