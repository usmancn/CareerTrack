using System.ComponentModel.DataAnnotations;
using CareerTrack.Models.Constants;
using Microsoft.AspNetCore.Mvc.Rendering;

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
        public string Role { get; set; } = AppRoles.Student;

        [Display(Name = "Şirket Seçimi")]
        public string EmployerCompanyMode { get; set; } = "Existing";

        [Display(Name = "Mevcut Şirket")]
        public int? CompanyId { get; set; }

        [Display(Name = "Şirket Adı")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "Şirket adı 2-200 karakter arasında olmalıdır.")]
        public string? CompanyName { get; set; }

        [Display(Name = "Sektör")]
        [StringLength(100, ErrorMessage = "Sektör en fazla 100 karakter olabilir.")]
        public string? CompanySector { get; set; }

        [Display(Name = "Konum")]
        [StringLength(200, ErrorMessage = "Konum en fazla 200 karakter olabilir.")]
        public string? CompanyLocation { get; set; }

        public SelectList? Companies { get; set; }

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
