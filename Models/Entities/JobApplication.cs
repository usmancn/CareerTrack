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
        public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;

        // Navigation Properties
        public ApplicationUser? Student { get; set; }
        public Company? Company { get; set; }
        public ICollection<Interview> Interviews { get; set; } = new List<Interview>();
        public Offer? Offer { get; set; }
    }
}
