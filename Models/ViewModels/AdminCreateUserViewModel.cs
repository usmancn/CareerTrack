using System.ComponentModel.DataAnnotations;
using CareerTrack.Models.Constants;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CareerTrack.Models.ViewModels
{
    public class AdminCreateUserViewModel
    {
        [Required(ErrorMessage = "Ad Soyad alanı zorunludur.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Ad Soyad 3-100 karakter arasında olmalıdır.")]
        [Display(Name = "Ad Soyad")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-posta adresi zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [Display(Name = "E-posta Adresi")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre zorunludur.")]
        [StringLength(100, ErrorMessage = "{0} en az {2} ve en fazla {1} karakter uzunluğunda olmalıdır.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Lütfen kullanıcının rolünü seçiniz.")]
        [Display(Name = "Kullanıcı Rolü")]
        public string Role { get; set; } = AppRoles.Student;

        [Display(Name = "Departman veya Kurum (Opsiyonel)")]
        [StringLength(100)]
        public string? Department { get; set; }

        [Display(Name = "Bağlı Şirket")]
        public int? CompanyId { get; set; }

        public SelectList? Companies { get; set; }
    }
}
