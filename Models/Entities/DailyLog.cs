using System.ComponentModel.DataAnnotations;

namespace CareerTrack.Models.Entities
{
    public class DailyLog
    {
        public int Id { get; set; }

        [Required]
        public string StudentId { get; set; } = string.Empty;

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

        [Display(Name = "Danışman Onay Durumu")]
        public bool IsApprovedByAdmin { get; set; } = false;

        [StringLength(500)]
        [Display(Name = "Admin / Danışman Notu")]
        public string? AdminNote { get; set; }

        // Navigation Property
        public ApplicationUser? Student { get; set; }
    }
}
