using System.ComponentModel.DataAnnotations;
using CareerTrack.Models.Enums;

namespace CareerTrack.Models.Entities
{
    public class DailyLog
    {
        public int Id { get; set; }

        [Required]
        public string StudentId { get; set; } = string.Empty;

        [Required]
        public int JobApplicationId { get; set; }

        [Required(ErrorMessage = "Gün numarası zorunludur.")]
        [Range(1, 365, ErrorMessage = "Gün numarası 1-365 arasında olmalıdır.")]
        [Display(Name = "Gün Numarası")]
        public int DayNumber { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Tarih")]
        public DateTime LogDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Günlük içeriği boş geçilemez!")]
        [StringLength(5000, MinimumLength = 100,
            ErrorMessage = "Günlük içeriği en az 100 karakter olmalıdır. Lütfen daha ayrıntılı yazınız.")]
        [Display(Name = "Bugün Neler Yaptınız?")]
        public string Content { get; set; } = string.Empty;

        [Display(Name = "Durum")]
        public DailyLogStatus Status { get; set; } = DailyLogStatus.Draft;

        [Display(Name = "İşveren Onayı")]
        public bool IsEmployerApproved { get; set; } = false;

        [StringLength(500)]
        [Display(Name = "İşveren Notu")]
        public string? EmployerNote { get; set; }

        [Display(Name = "Okul Onayı")]
        public bool IsSchoolApproved { get; set; } = false;

        [StringLength(500)]
        [Display(Name = "Okul / Danışman Notu")]
        public string? SchoolNote { get; set; }

        // Navigation Property
        public ApplicationUser? Student { get; set; }
        public JobApplication? JobApplication { get; set; }
    }
}
