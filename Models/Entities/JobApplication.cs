using System.ComponentModel.DataAnnotations;
using CareerTrack.Models.Enums;

namespace CareerTrack.Models.Entities
{
    public class JobApplication
    {
        public int Id { get; set; }

        [Required]
        public string StudentId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şirket seçimi zorunludur.")]
        [Display(Name = "Şirket")]
        public int CompanyId { get; set; }

        [Display(Name = "İlan (Opsiyonel)")]
        public int? InternshipPostingId { get; set; }

        [Required(ErrorMessage = "Pozisyon boş geçilemez!")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "Pozisyon 2-200 karakter arasında olmalıdır.")]
        [Display(Name = "Pozisyon / Unvan")]
        public string Position { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Başvuru Tarihi")]
        public DateTime ApplicationDate { get; set; } = DateTime.Today;

        [Display(Name = "Staj Türü")]
        public InternshipType InternshipType { get; set; } = InternshipType.MandatoryShort;

        [Display(Name = "Durum")]
        public ApplicationStatus Status { get; set; } = ApplicationStatus.SchoolPending;

        [Display(Name = "Okul Notu")]
        public string? SchoolNote { get; set; }

        [Display(Name = "İşveren Notu")]
        public string? EmployerNote { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Staj Başlangıç Tarihi")]
        public DateTime? InternshipStartDate { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Staj Bitiş Tarihi")]
        public DateTime? InternshipEndDate { get; set; }

        [Display(Name = "Toplam Staj Günü")]
        public int? TotalInternshipDays { get; set; }

        // Navigation Properties
        public ApplicationUser? Student { get; set; }
        public Company? Company { get; set; }
        public ICollection<ToDo> ToDos { get; set; } = new List<ToDo>();
    }
}
