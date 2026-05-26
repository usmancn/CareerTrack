using System.ComponentModel.DataAnnotations;

namespace CareerTrack.Models.Entities
{
    public class InternshipPosting
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Şirket seçimi zorunludur.")]
        [Display(Name = "Şirket")]
        public int CompanyId { get; set; }

        [Required(ErrorMessage = "İlan başlığı boş geçilemez!")]
        [StringLength(200, MinimumLength = 5, ErrorMessage = "Başlık 5-200 karakter arasında olmalıdır.")]
        [Display(Name = "İlan Başlığı")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Açıklama boş geçilemez!")]
        [Display(Name = "Açıklama ve Nitelikler")]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Aktif mi?")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Oluşturulma Tarihi")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Son başvuru tarihi zorunludur.")]
        [DataType(DataType.Date)]
        [Display(Name = "Son Başvuru Tarihi")]
        public DateTime Deadline { get; set; }

        // Navigation Properties
        public Company? Company { get; set; }
        public ICollection<JobApplication> Applications { get; set; } = new List<JobApplication>();
    }
}
