using System.ComponentModel.DataAnnotations;

namespace CareerTrack.Models.ViewModels
{
    /// <summary>
    /// Öğrencinin başvuru sırasında yeni şirket önermesi için ViewModel.
    /// </summary>
    public class CompanySuggestViewModel
    {
        [Required(ErrorMessage = "Şirket adı boş geçilemez!")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "Şirket adı 2-200 karakter arasında olmalıdır.")]
        [Display(Name = "Şirket Adı")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Sektör boş geçilemez!")]
        [StringLength(100, ErrorMessage = "Sektör en fazla 100 karakter olabilir.")]
        [Display(Name = "Sektör")]
        public string Sector { get; set; } = string.Empty;

        [Required(ErrorMessage = "Konum boş geçilemez!")]
        [StringLength(200, ErrorMessage = "Konum en fazla 200 karakter olabilir.")]
        [Display(Name = "Konum")]
        public string Location { get; set; } = string.Empty;
    }
}
