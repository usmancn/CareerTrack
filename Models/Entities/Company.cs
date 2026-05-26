using System.ComponentModel.DataAnnotations;

namespace CareerTrack.Models.Entities
{
    public class Company
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Şirket adı boş geçilemez!")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "Şirket adı 2-200 karakter arasında olmalıdır.")]
        [Display(Name = "Şirket Adı")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Sektör boş geçilemez!")]
        [StringLength(100)]
        [Display(Name = "Sektör")]
        public string Sector { get; set; } = string.Empty;

        [Required(ErrorMessage = "Konum boş geçilemez!")]
        [StringLength(200)]
        [Display(Name = "Konum")]
        public string Location { get; set; } = string.Empty;

        /// <summary>
        /// Admin onayı. Admin eklerse true, öğrenci önerirse false başlar.
        /// </summary>
        [Display(Name = "Onay Durumu")]
        public bool IsApproved { get; set; } = true;

        /// <summary>
        /// Öğrenci tarafından önerilmişse, oluşturan kullanıcının Id'si.
        /// Admin oluşturmuşsa null kalır.
        /// </summary>
        public string? CreatedByUserId { get; set; }

        // Navigation Properties
        public ApplicationUser? CreatedBy { get; set; }
        public ICollection<JobApplication> JobApplications { get; set; } = new List<JobApplication>();
        public ICollection<JobPosting> JobPostings { get; set; } = new List<JobPosting>();
    }
}
