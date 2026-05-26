using System.ComponentModel.DataAnnotations;
using CareerTrack.Models.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CareerTrack.Models.ViewModels
{
    public class ApplicationCreateViewModel
    {
        public int Id { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Şirket seçimi zorunludur.")]
        [Display(Name = "Şirket")]
        public int CompanyId { get; set; }

        [Required(ErrorMessage = "Pozisyon boş geçilemez!")]
        [StringLength(200, MinimumLength = 2)]
        [Display(Name = "Pozisyon / Unvan")]
        public string Position { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Başvuru Tarihi")]
        public DateTime ApplicationDate { get; set; } = DateTime.Today;

        [Display(Name = "Staj Türü")]
        public InternshipType InternshipType { get; set; } = InternshipType.MandatoryShort;

        [DataType(DataType.Date)]
        [Display(Name = "Staj Başlangıç Tarihi")]
        public DateTime? InternshipStartDate { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Staj Bitiş Tarihi")]
        public DateTime? InternshipEndDate { get; set; }

        [Display(Name = "Toplam Staj Günü")]
        public int? TotalInternshipDays { get; set; }

        [Display(Name = "Durum")]
        public ApplicationStatus Status { get; set; } = ApplicationStatus.SchoolPending;

        // Dropdown için
        public SelectList? Companies { get; set; }
    }
}
