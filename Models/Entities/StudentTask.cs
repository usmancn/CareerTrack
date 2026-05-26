using System.ComponentModel.DataAnnotations;

namespace CareerTrack.Models.Entities
{
    public class StudentTask
    {
        public int Id { get; set; }

        [Required]
        public int JobApplicationId { get; set; }

        [Required]
        public string AssignedByEmployerId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Görev başlığı zorunludur.")]
        [StringLength(200, MinimumLength = 3)]
        [Display(Name = "Görev Başlığı")]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        [Display(Name = "Açıklama")]
        public string? Description { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Bitiş Tarihi")]
        public DateTime? DueDate { get; set; }

        [Display(Name = "Tamamlandı mı?")]
        public bool IsCompleted { get; set; } = false;

        [DataType(DataType.Date)]
        public DateTime? CompletedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation Properties
        public JobApplication? JobApplication { get; set; }
        public ApplicationUser? AssignedByEmployer { get; set; }
    }
}
