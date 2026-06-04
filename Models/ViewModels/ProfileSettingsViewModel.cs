using System.ComponentModel.DataAnnotations;

namespace CareerTrack.Models.ViewModels
{
    public class ProfileSettingsViewModel
    {
        [Required(ErrorMessage = "Ad Soyad zorunludur.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Ad Soyad 3-100 karakter arasında olmalıdır.")]
        [Display(Name = "Ad Soyad")]
        public string FullName { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Bölüm / Departman en fazla 100 karakter olabilir.")]
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
