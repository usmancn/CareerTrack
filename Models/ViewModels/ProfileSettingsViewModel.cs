using System.ComponentModel.DataAnnotations;

namespace CareerTrack.Models.ViewModels
{
    public class ProfileSettingsViewModel
    {
        [Required(ErrorMessage = "Ad Soyad zorunludur.")]
        [Display(Name = "Ad Soyad")]
        public string FullName { get; set; } = string.Empty;

        [Display(Name = "Bölüm / Departman")]
        public string? Department { get; set; }

        [Display(Name = "E-posta Adresi (Değiştirilemez)")]
        public string Email { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Mevcut Şifre")]
        public string? OldPassword { get; set; }

        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "{0} en az {2} ve en fazla {1} karakter uzunluğunda olmalıdır.", MinimumLength = 6)]
        [Display(Name = "Yeni Şifre")]
        public string? NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Yeni Şifre (Tekrar)")]
        [Compare("NewPassword", ErrorMessage = "Yeni şifreler eşleşmiyor.")]
        public string? ConfirmPassword { get; set; }
    }
}
