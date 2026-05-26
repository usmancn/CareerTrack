using System.ComponentModel.DataAnnotations;
using CareerTrack.Models.Enums;

namespace CareerTrack.Models.Entities
{
    /// <summary>
    /// İşveren tarafından yayınlanan staj ilanı.
    /// </summary>
    public class JobPosting
    {
        public int Id { get; set; }

        [Required]
        public string EmployerId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şirket seçimi zorunludur.")]
        [Display(Name = "Şirket")]
        public int CompanyId { get; set; }

        [Required(ErrorMessage = "Pozisyon başlığı zorunludur.")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Pozisyon başlığı 3-200 karakter arasında olmalıdır.")]
        [Display(Name = "Pozisyon Başlığı")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "İlan açıklaması zorunludur.")]
        [StringLength(3000, MinimumLength = 50, ErrorMessage = "Açıklama en az 50 karakter olmalıdır.")]
        [Display(Name = "İlan Açıklaması")]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Staj Türü")]
        public InternshipType InternshipType { get; set; } = InternshipType.MandatoryShort;

        [Required(ErrorMessage = "Kontenjan zorunludur.")]
        [Range(1, 100, ErrorMessage = "Kontenjan 1-100 arasında olmalıdır.")]
        [Display(Name = "Kontenjan")]
        public int Quota { get; set; } = 1;

        [Required(ErrorMessage = "Başlangıç tarihi zorunludur.")]
        [DataType(DataType.Date)]
        [Display(Name = "Staj Başlangıç Tarihi")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Bitiş tarihi zorunludur.")]
        [DataType(DataType.Date)]
        [Display(Name = "Staj Bitiş Tarihi")]
        public DateTime EndDate { get; set; }

        [Display(Name = "Aktif mi?")]
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation Properties
        public ApplicationUser? Employer { get; set; }
        public Company? Company { get; set; }
    }
}
