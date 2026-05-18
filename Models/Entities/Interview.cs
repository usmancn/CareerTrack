using System.ComponentModel.DataAnnotations;
using CareerTrack.Models.Enums;

namespace CareerTrack.Models.Entities
{
    public class Interview
    {
        public int Id { get; set; }

        [Required]
        public int JobApplicationId { get; set; }

        [Required(ErrorMessage = "Mülakat tarihi zorunludur.")]
        [DataType(DataType.Date)]
        [Display(Name = "Mülakat Tarihi")]
        public DateTime InterviewDate { get; set; }

        [Display(Name = "Aşama Türü")]
        public InterviewStage Stage { get; set; } = InterviewStage.HRInterview;

        [StringLength(1000)]
        [Display(Name = "Notlar")]
        public string? Notes { get; set; }

        [Display(Name = "Sonuç")]
        public InterviewResult ResultStatus { get; set; } = InterviewResult.Pending;

        // Navigation Property
        public JobApplication? JobApplication { get; set; }
    }
}
