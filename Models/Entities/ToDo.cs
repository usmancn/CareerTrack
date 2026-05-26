using System.ComponentModel.DataAnnotations;

namespace CareerTrack.Models.Entities
{
    public class ToDo
    {
        public int Id { get; set; }

        [Required]
        public string StudentId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Görev başlığı boş geçilemez!")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Görev başlığı 3-200 karakter arasında olmalıdır.")]
        [Display(Name = "Görev")]
        public string TaskTitle { get; set; } = string.Empty;

        [Display(Name = "Tamamlandı mı?")]
        public bool IsCompleted { get; set; } = false;

        [Required(ErrorMessage = "Bitiş tarihi zorunludur.")]
        [DataType(DataType.Date)]
        [Display(Name = "Bitiş Tarihi")]
        public DateTime DueDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public int? JobApplicationId { get; set; }

        // Navigation Property
        public ApplicationUser? Student { get; set; }
        public JobApplication? JobApplication { get; set; }
    }
}
