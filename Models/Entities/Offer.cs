using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareerTrack.Models.Entities
{
    public class Offer
    {
        public int Id { get; set; }

        [Required]
        public int JobApplicationId { get; set; }

        [Required(ErrorMessage = "Maaş bilgisi zorunludur.")]
        [Range(0, 9999999, ErrorMessage = "Geçerli bir maaş tutarı giriniz.")]
        [Display(Name = "Aylık Maaş (TL)")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Salary { get; set; }

        [StringLength(500)]
        [Display(Name = "Yan Haklar (Yol, Yemek vb.)")]
        public string? Benefits { get; set; }

        [Required(ErrorMessage = "Son dönüş tarihi zorunludur.")]
        [DataType(DataType.Date)]
        [Display(Name = "Son Dönüş Tarihi")]
        public DateTime DeadlineDate { get; set; }

        [Display(Name = "Kabul Edildi mi?")]
        public bool IsAccepted { get; set; } = false;

        // Navigation Property
        public JobApplication? JobApplication { get; set; }
    }
}
