using System.ComponentModel.DataAnnotations;

namespace CareerTrack.Models.ViewModels
{
    public class DailyLogCreateViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Gün numarası zorunludur.")]
        [Range(1, 365, ErrorMessage = "Gün numarası 1-365 arasında olmalıdır.")]
        [Display(Name = "Gün Numarası (Örn: 14)")]
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
    }
}
