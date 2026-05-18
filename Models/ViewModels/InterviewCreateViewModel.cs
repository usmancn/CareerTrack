using System.ComponentModel.DataAnnotations;
using CareerTrack.Models.Enums;

namespace CareerTrack.Models.ViewModels
{
    public class InterviewCreateViewModel
    {
        public int Id { get; set; }
        public int JobApplicationId { get; set; }

        [Required(ErrorMessage = "Mülakat tarihi zorunludur.")]
        [DataType(DataType.Date)]
        [Display(Name = "Mülakat Tarihi")]
        public DateTime InterviewDate { get; set; } = DateTime.Today.AddDays(7);

        [Display(Name = "Aşama Türü")]
        public InterviewStage Stage { get; set; } = InterviewStage.HRInterview;

        [StringLength(1000)]
        [Display(Name = "Notlar")]
        public string? Notes { get; set; }

        [Display(Name = "Sonuç")]
        public InterviewResult ResultStatus { get; set; } = InterviewResult.Pending;

        // View'de bilgi göstermek için
        public string? CompanyName { get; set; }
        public string? Position { get; set; }
    }
}
