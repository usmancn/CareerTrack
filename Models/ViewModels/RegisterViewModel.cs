using System.ComponentModel.DataAnnotations;

namespace CareerTrack.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Ad Soyad zorunludur.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Ad Soyad 3-100 karakter arasında olmalıdır.")]
        [Display(Name = "Ad Soyad")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-posta adresi zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [Display(Name = "E-posta")]
        public string Email { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "Bölüm / Departman")]
        public string? Department { get; set; }

        [Required(ErrorMessage = "Hesap türü seçimi zorunludur.")]
        [Display(Name = "Hesap Türü")]
        public string Role { get; set; } = "Student";

        [Required(ErrorMessage = "Şifre zorunludur.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Şifreler eşleşmiyor.")]
        [Display(Name = "Şifre Tekrar")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
