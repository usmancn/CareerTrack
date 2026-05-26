using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace CareerTrack.Models.Entities
{
    public class ApplicationUser : IdentityUser
    {
        [Required(ErrorMessage = "Ad Soyad zorunludur.")]
        [StringLength(100, MinimumLength = 3)]
        [Display(Name = "Ad Soyad")]
        public string FullName { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "Bölüm")]
        public string? Department { get; set; }

        /// <summary>
        /// İşveren rolü için bağlı olduğu şirket.
        /// Öğrenci / Okul / Admin için null kalır.
        /// </summary>
        [Display(Name = "Şirket")]
        public int? CompanyId { get; set; }

        // Navigation Properties
        public Company? Company { get; set; }
        public ICollection<JobApplication> JobApplications { get; set; } = new List<JobApplication>();
        public ICollection<DailyLog> DailyLogs { get; set; } = new List<DailyLog>();
        public ICollection<ToDo> ToDos { get; set; } = new List<ToDo>();
        public ICollection<JobPosting> JobPostings { get; set; } = new List<JobPosting>();
    }
}
